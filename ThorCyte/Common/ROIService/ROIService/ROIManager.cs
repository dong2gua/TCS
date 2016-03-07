using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ComponentDataService;
using ComponentDataService.Types;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ROIService.Region;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Types;

namespace ROIService
{

    public class ROIManager : IRegionService
    {
        #region Singleton

        private static readonly  ROIManager InstanceField;

        public static ROIManager Instance
        {
            get { return InstanceField; }
        }

        #endregion

        #region Field

        private static readonly BioEvent[] EmptyBioEvents = new BioEvent[0];
        private const int DefaultRegionCount = 5;
        private readonly List<int> _bitmap = new List<int>();

        private readonly Dictionary<string, MaskRegion> _roiDictionary =
            new Dictionary<string, MaskRegion>(DefaultRegionCount);

        private readonly Dictionary<string, IList<BioEvent>> _regionEventsDictionary =
            new Dictionary<string, IList<BioEvent>>(DefaultRegionCount);
        private IList<int> _activeWells;
        private readonly IEventAggregator _eventAggregator;
        #endregion


        #region Properties

        public int WellNo { get; private set; }
        public int RegionNo { get; private set; }

        #endregion
        #region Constructor

        private ROIManager()
        {
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            RegisterMessage();
        }

        static ROIManager()
        {
            InstanceField = new ROIManager();
        }
        #endregion

        #region Method

        #region Public

        public IList<BioEvent> GetEvents(string id)
        {
            return GetEvents(id, _roiDictionary, _regionEventsDictionary);
        }

        public IList<BioEvent> GetEvents(string lhsId, string rhsId, OperationType type)
        {
            return GetEvents(lhsId, rhsId, type, _roiDictionary, _regionEventsDictionary);
        }

        public int GetRegionId()
        {
            int regionId;
            var index = _bitmap.FindIndex(w => w == 0);
            if (index >= 0)
            {
                _bitmap[index] = 1;
                regionId = index + 1;
            }
            else
            {
                _bitmap.Add(0x01);
                regionId = _bitmap.Count;
            }
            return regionId;
        }

        public bool AddRegion(MaskRegion region)
        {
            string id = string.Format("R{0}", region.Id);
            if (_roiDictionary.ContainsKey(id))
                return false;
            _roiDictionary[id] = region;
            _regionEventsDictionary[id] = EmptyBioEvents;
            region.Calculate();
            AddRelationShip(id);
            if (_activeWells.Count == 1)
                UpdateEventsDictionary(id);
            else if (_activeWells.Count > 1)
                UpdateRegionOnMultiWells(_activeWells, id);
            return true;
        }

        public void ChangeActiveWells(IList<int> activeWells)
        {
          
            var n = activeWells.Count;
            _activeWells = activeWells;
            if (n > 0)
            {
                InitRegionEvents(activeWells);
                if (n != 1) return;
                WellNo = activeWells.First();
                RegionNo = WellNo;
            }
            else
            {
                WellNo = 0;
                RegionNo = 0;
                InitRegionEvents(WellNo, RegionNo);
            }
        }

        public void SetRegion(MaskRegion region)
        {
            string id = string.Format("R{0}", region.Id);
            int count = _roiDictionary.Count;
            _roiDictionary[id] = region;
            //region.Calculate();
            _regionEventsDictionary[id] = EmptyBioEvents;
            int no = int.Parse(id.TrimStart('R')) - 1;
            if (count < no + 1)
            {
                int len = (no + 1) - count;
                var added = new int[len];
                _bitmap.AddRange(added);
            }
            _bitmap[no] = 1;
            
        }
        
       

        public bool RemoveRegions(IEnumerable<string> ids)
        {           
            var rst =  ids.Aggregate(true, (current, id) => current && RemoveRegion(id));           
            return rst;
        }

       
        public IEnumerable<string> GetRegionIdList()
        {
            return _roiDictionary.Keys;
        }

        public IEnumerable<string> GetRegionIdList(string componentName)
        {
            return (from entry in _roiDictionary
                let key = entry.Key
                let region = entry.Value
                where region.ComponentName.Equals(componentName)
                select key).ToList();
        }

        public MaskRegion GetRegion(string id)
        {
            return !_roiDictionary.ContainsKey(id) ? null : _roiDictionary[id];
        }

      

        public bool UpdateRegions(ICollection<MaskRegion> regions)
        {
            return regions.Aggregate(true, (current, region) => current && UpdateRegion(region));
        }

        public void InitRegions(IEnumerable<MaskRegion> regions)
        {
            foreach (MaskRegion region in regions)
            {
                string id = string.Format("R{0}", region.Id);
                _roiDictionary[id] = region;
                region.Calculate();
            }

            _eventAggregator.GetEvent<InitRegionEvent>().Publish(0);
        }

        public void Clear()
        {
            _roiDictionary.Clear();
            _regionEventsDictionary.Clear();
            _bitmap.Clear();
        }
        #endregion

        #region Internal
        internal void InitRegionEvents(int wellNo, int regionNo)
        {
            InitRegionEvents(wellNo, regionNo, _roiDictionary, _regionEventsDictionary);
        }


      
        #endregion

        #region Private

        private IDictionary<string, IList<BioEvent>> InitRegionEvents(int wellNo, int regionNo,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            return InitRegionEvents(wellNo, regionNo, _roiDictionary, regionEventsDictionary);
        }

        private IDictionary<string, IList<BioEvent>> InitRegionEvents(int wellNo, int regionNo,
            IDictionary<string, MaskRegion> roiDictionary, IDictionary<string, IList<BioEvent>> regionEventsDictionary)
            
        {
            if (wellNo == 0 || regionNo == 0)
            {
                foreach (var key in roiDictionary.Keys.ToList())
                {
                    regionEventsDictionary[key] = EmptyBioEvents;
                }
                return regionEventsDictionary;
            }
            foreach (var key in roiDictionary.Keys.ToList())
            {
                var region = roiDictionary[key];
                if (string.IsNullOrEmpty(region.LeftParent) && string.IsNullOrEmpty(region.RightParent))
                    regionEventsDictionary[key] = GetEvents(region, wellNo);
            }
            UpdateAll(wellNo, regionNo, roiDictionary, regionEventsDictionary);
            
            return regionEventsDictionary;
        }

        private static IList<BioEvent> GetEvents(string id, IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            if (!(roiDictionary.ContainsKey(id) && regionEventsDictionary.ContainsKey(id)))
                return EmptyBioEvents;
            return regionEventsDictionary[id];
        }

       

        private bool RemoveRegion(string id)
        {
            SetRegionEventsDefaultColor(id);
            RemoveRegionEventsRecursively(id);
            RemoveRelationship(id);
            var result = _roiDictionary.Remove(id) && _regionEventsDictionary.Remove(id);
          
            foreach (var key in _roiDictionary.Keys.ToList())
            {
                SetRegionEventsColor(key);
            }
            if (result == false)
                return false;
            var no = int.Parse(id.TrimStart('R')) - 1;
            _bitmap[no] = 0;         
            return true;
        }
        
        private bool UpdateRegion(MaskRegion region)
        {

            string id = string.Format("R{0}", region.Id);
            if (!_roiDictionary.ContainsKey(id)) return false;            
            _roiDictionary[id] = region;
            region.Calculate();
            if (_activeWells == null) return false;
            var n = _activeWells.Count;
            if (n == 1)
            {
                int activeWell = _activeWells.First();
                SetRegionEventsDefaultColor(id);
                UpdateEventsDictionary(id, activeWell, activeWell);              
                foreach (var key in _roiDictionary.Keys.ToList())
                {
                    SetRegionEventsColor(key);
                }              
            }
            else if (n > 1)
            {
                UpdateRegionOnMultiWells(_activeWells, id);
            }
            return true;
        }

        private void SetRegionEventsColor(string id)
        {
            SetRegionEventsColor(id, _roiDictionary[id].Color);
        }
        private void SetRegionEventsColor(string id, Color color)
        {
            var evs = _regionEventsDictionary[id];
            foreach (var ev in evs)
            {
                SetEventColor(ev, color);
            }
        }

       
        private void SetRegionEventsDefaultColor(string id)
        {
            var evs = _regionEventsDictionary[id];
            foreach (var ev in evs)
            {
                ev.ColorIndex = RegionColorIndex.White;
            }
        }

        private IList<BioEvent> GetEvents(string lhsId, string rhsId, OperationType type,
           IDictionary<string, MaskRegion> roiDictionary, IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            if (!roiDictionary.ContainsKey(lhsId) && !roiDictionary.ContainsKey(rhsId))
                return EmptyBioEvents;
            switch (type)
            {
                case OperationType.None:
                    return GetEvents(lhsId, roiDictionary, regionEventsDictionary);
                case OperationType.And:
                    return GetEventsByAnd(lhsId, rhsId, roiDictionary, regionEventsDictionary);
                case OperationType.Not:
                    return GetEventsByNot(lhsId, rhsId, roiDictionary, regionEventsDictionary);
                case OperationType.Or:
                    return GetEventsByOr(lhsId, rhsId, roiDictionary, regionEventsDictionary);
                default:
                    return EmptyBioEvents;
            }
        }
      

        private void InitRegionEvents(IList<int> activeWells)
        {
            InitRegionEvents(activeWells, _roiDictionary.Keys);
        }

        private void InitRegionEvents(IList<int> activeWells, IEnumerable<string> regionIds )
        {
            var n = activeWells.Count;
            var tasks = new Task<IDictionary<string, IList<BioEvent>>>[n];
            for (var i = 0; i < n; i++)
            {
                int wellNo = activeWells[i];
                tasks[i] = new Task<IDictionary<string, IList<BioEvent>>>(InitRegionEvents, wellNo);
                tasks[i].Start();
            }

            Task.WaitAll(tasks.Cast<Task>().ToArray());
            foreach (string key in regionIds)
            {
                SetRegionEventsDefaultColor(key);
                var events = new List<BioEvent>();
                for (var i = 0; i < n; i++)
                {
                    var rst = tasks[i].Result;
                    events.AddRange(rst[key]);
                }
                _regionEventsDictionary[key] = events;
            }
            foreach (var key in _roiDictionary.Keys.ToList())
            {
                SetRegionEventsColor(key);
            }
        }

        private IDictionary<string, IList<BioEvent>> InitRegionEvents(object state)
        {
            var regionEventsDictionary = new Dictionary<string, IList<BioEvent>>();         
            var wellNo = Convert.ToInt32(state);
            return InitRegionEvents(wellNo, wellNo, regionEventsDictionary);
        }

        private void AddRelationShip(string id)
        {
            if (!_roiDictionary.ContainsKey(id)) return;
            var child = _roiDictionary[id];
            foreach (var key in _roiDictionary.Keys)
            {
                if (key != child.LeftParent && key != child.RightParent) continue;
                var parent = _roiDictionary[key];
                parent.Children.Add(id);
            }
        }

       
        private void RemoveRelationship(string id)
        {
            var removed = _roiDictionary[id];
            foreach (var region in removed.Children.Select(child => _roiDictionary[child]))
            {
                if (region.LeftParent == id)
                    region.LeftParent = string.Empty;
                else
                {
                    region.RightParent = string.Empty;
                }
            }
            if (_roiDictionary.ContainsKey(removed.LeftParent))
            {
                var leftParent = _roiDictionary[removed.LeftParent];
                leftParent.Children.Remove(id);
            }

            if (_roiDictionary.ContainsKey(removed.RightParent))
            {
                var rightParent = _roiDictionary[removed.RightParent];
                rightParent.Children.Remove(id);
            }
           
        }

        private void RegisterMessage()
        {          
            _eventAggregator.GetEvent<RemoveRegionsEvent>().Subscribe(OnRemoveRegions);
        }

        private void UpdateRegionOnMultiWells(IList<int> activeWells, string id)
        {
            InitRegionEvents(activeWells, new[] {id});
        }

        private void OnRemoveRegions(IEnumerable<string> ids )
        {
            foreach (var id in ids)
            {
                RemoveRegion(id);
            }
        }

        private static IList<BioEvent> GetEventsByAnd(string lhsId, string rhsId, IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            var lhsData = GetEvents(lhsId, roiDictionary, regionEventsDictionary);
            var rhsData = GetEvents(rhsId, roiDictionary, regionEventsDictionary);
            var result = lhsData.Intersect(rhsData).OrderBy(w => w.Id);
            return result.ToList();
        }

        private static IList<BioEvent> GetEventsByOr(string lhsId, string rhsId, IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            var lhsData = GetEvents(lhsId, roiDictionary, regionEventsDictionary);
            var rhsData = GetEvents(rhsId, roiDictionary, regionEventsDictionary);
            var result = lhsData.Union(rhsData).OrderBy(w => w.Id);
            return result.ToList();
        }

        private static IList<BioEvent> GetEventsByNot(string lhsId, string rhsId, IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            var lhsData = GetEvents(lhsId, roiDictionary, regionEventsDictionary);
            var rhsData = GetEvents(rhsId, roiDictionary, regionEventsDictionary);
            var result = lhsData.Except(rhsData).OrderBy(w => w.Id);
            return result.ToList();
        }

        private static IList<BioEvent> GetEvents(MaskRegion mask, int wellNo)
        {
            var name = mask.ComponentName;
            var data = ComponentDataManager.Instance.GetEvents(name, wellNo);
            if(data==null)
                return EmptyBioEvents;
            var n = data.Count;
            var rst = new List<BioEvent>(n);
            for (var i = 0; i < n; i++)
            {
                var ev = data[i];
                if (!IsContainEvent(ev, mask)) continue;
                //SetEventColor(ev, mask.Color);
                rst.Add(ev);
            }
            return rst;
        }

        private static IList<BioEvent> GetEvents(MaskRegion mask, IList<BioEvent> source)
        {
            var n = source.Count;
            var rst = new List<BioEvent>(n);
            for (var i = 0; i < n; i++)
            {
                var ev = source[i];
                if (!IsContainEvent(ev, mask)) continue;
                //SetEventColor(ev, mask.Color);
                rst.Add(ev);
            }
            return rst;
        }

        private void UpdateEventsDictionary(string id)
        {
            UpdateEventsDictionary(id, WellNo, RegionNo);
        }

        private void UpdateEventsDictionary(string id, int wellNo, int regionNo)
        {
            if (_roiDictionary.ContainsKey(id))
            {
                UpdateRegionEventsRecursively(id, wellNo, regionNo);
            }
        }
      
       
        private void UpdateAll(int wellNo, int regionNo, IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {

            foreach (var key in roiDictionary.Keys.ToList())
            {
                UpdateRegionEventsRecursively(key, wellNo, regionNo, roiDictionary, regionEventsDictionary);
            }
        }

        private static bool IsContainEvent(BioEvent ev, MaskRegion mask)
        {
            var indexNumX = mask.IndexNumX;
            var indexDenoX = mask.IndexDenoX;
            var indexNumY = mask.IndexNumY;
            var indexDenoY = mask.IndexDenoY;
            var fx = GetAxisData(ev, indexNumX, indexDenoX);
            var fy = GetAxisData(ev, indexNumY, indexDenoY);        
            return mask.Contains(new Point(fx, fy));
        }

        private static double GetAxisData(BioEvent ev,int indexNum, int indexDeno)
        {
            double data;
            if (indexNum >= 0)
            {
                if (indexDeno >= 0)
                {
                    data = ev[indexNum]/ev[indexDeno];
                }

                else
                {
                    data = ev[indexNum];
                }                   
            }
            else
            {
                data = 0;
            }
            return data;
        }

        private void UpdateRegionEventsRecursively(string root, int wellNo, int regionNo)
        {
            UpdateRegionEventsRecursively(root, wellNo, regionNo, _roiDictionary, _regionEventsDictionary);
        }

        private void UpdateRegionEventsRecursively(string root, int wellNo, int regionNo,
            IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            if (wellNo == 0 || regionNo == 0) return;
            var queue = new List<string> {root};
            while (queue.Count > 0)
            {
                var first = queue.First();
                var region = roiDictionary[first];
                regionEventsDictionary[first] = GetEvents(region,
                    GetEventsSource(first, region.LeftParent, region.RightParent, wellNo, roiDictionary,
                        regionEventsDictionary));
                if (region.Children != null)
                    queue.AddRange(region.Children);
                queue.RemoveAt(0);

            }
        }

        private void RemoveRegionEventsRecursively(string removedId)
        {
            var queue = new List<string>();
            var removedRegion = _roiDictionary[removedId];
            var children = removedRegion.Children;
            foreach (var region in children.Select(child => _roiDictionary[child]))
            {
                if (region.LeftParent == removedId)
                    region.LeftParent = string.Empty;
                else
                {
                    region.RightParent = string.Empty;
                }
            }
            queue.AddRange(removedRegion.Children);
            
            while (queue.Count>0)
            {
                var first = queue.First();
                 var region = _roiDictionary[first];
                if (WellNo != 0 && RegionNo != 0)
                    _regionEventsDictionary[first] = GetEvents(region,
                        GetEventsSource(first, region.LeftParent, region.RightParent));
                if (region.Children != null)
                    queue.AddRange(region.Children);
                queue.RemoveAt(0);
            }
        }

        private IList<BioEvent> GetEventsSource(string childId, string leftParentId, string rightParentId)
        {
            return GetEventsSource(childId, leftParentId, rightParentId, WellNo, _roiDictionary,
                _regionEventsDictionary);
        }

        private IList<BioEvent> GetEventsSource(string childId, string leftParentId, string rightParentId, int wellNo, IDictionary<string, MaskRegion> roiDictionary,
            IDictionary<string, IList<BioEvent>> regionEventsDictionary)
        {
            if (!roiDictionary.ContainsKey(childId)) return EmptyBioEvents;
            var child = roiDictionary[childId];
            return !roiDictionary.ContainsKey(leftParentId)
                ? ComponentDataManager.Instance.GetEvents(child.ComponentName, wellNo)
                : GetEvents(leftParentId, rightParentId, roiDictionary[childId].Operation, roiDictionary,
                    regionEventsDictionary);
        }

        private static void SetEventColor(BioEvent ev, Color maskColor)
        {
           
            var index = GetColorIndexFromColor(maskColor);
            if ((int)index < (int) ev.ColorIndex)
                ev.ColorIndex = index;
        }
        private static RegionColorIndex GetColorIndexFromColor(Color color)
        {
            if (color == Colors.Red)
            {
                return RegionColorIndex.Red;
            }
            if (color == Colors.LawnGreen)
            {
                return RegionColorIndex.LawnGreen;
            }
            if (color == Colors.Orange)
            {
                return RegionColorIndex.Orange;
            }
            if (color == Colors.Yellow)
            {
                return RegionColorIndex.Yellow;
            }
            if (color == Colors.Magenta)
            {
                return RegionColorIndex.Magenta;
            }
            if (color == Colors.Cyan)
            {
                return RegionColorIndex.Cyan;
            }
            if (color == Colors.White)
            {
                return RegionColorIndex.White;
            }
            return RegionColorIndex.White;
        }

        #endregion

        #endregion



       

    }
}
