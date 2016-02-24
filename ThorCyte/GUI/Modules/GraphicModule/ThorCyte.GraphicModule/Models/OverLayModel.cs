using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ComponentDataService;
using ComponentDataService.Types;
using Prism.Mvvm;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Models
{
    public class OverLayModel : BindableBase
    {
        #region Properties and Fields


        private double _xscale;

        public HistogramVm ParentGraph { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }
                SetProperty(ref _name, value);
            }
        }

        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isEnabled, value);
            }
        }

        private IList<int> _wellNoList = new List<int>(); // wells that provide the Data 

        public IList<int> WellNoList
        {
            get { return _wellNoList; }
            set { _wellNoList = value; }
        }

        private readonly List<double> _datas = new List<double>(); // wells that provide the Data 

        public List<double> Datas
        {
            get { return _datas; }
        }

        private ColorInfo _overlayColorInfo;

        public ColorInfo OverlayColorInfo
        {
            get { return _overlayColorInfo; }
            set
            {
                if (value == _overlayColorInfo)
                {
                    return;
                }
                SetProperty(ref _overlayColorInfo, value);
            }
        }

        private string _wellIdListStr;

        public string WellIdListStr
        {
            get { return _wellIdListStr; }
            set
            {
                if (value == _wellIdListStr)
                {
                    return;
                }
                SetProperty(ref _wellIdListStr, value);
            }
        }

        #region Constructor

        public OverLayModel(string name, ColorInfo colorInfo, IList<int> wells)
        {
            _name = name;
            _overlayColorInfo = colorInfo;
            _wellNoList = wells;
            WellIdListStr = ToString();
        }

        #endregion

        #region

        public override string ToString()
        {
            var strBuilder = new StringBuilder("Well Id : ");
            for (var index = 0; index < _wellNoList.Count; index++)
            {
                strBuilder.Append(_wellNoList[index]);
                if (index != _wellNoList.Count - 1)
                {
                    strBuilder.Append(",");
                }
            }
            return strBuilder.ToString();
        }

        public void RefreshData()
        {
            _datas.Clear();
            _datas.AddRange(new double[ParentGraph.Width]);
            _xscale = (ParentGraph.XAxis.MaxValue - ParentGraph.XAxis.MinValue) / (ParentGraph.Width - 1);
            foreach (var wellId in _wellNoList)
            {
                var events = ComponentDataManager.Instance.GetEvents(ParentGraph.SelectedComponent, wellId);
                if (events == null || events.Count == 0)
                {
                    continue;
                }
                events.ToList().ForEach(ProcessEvent);
            }
        }

        /// <summary>
        ///  Updates bin count based on event data
        /// </summary>
        /// <param name="ev"></param>
        private void ProcessEvent(BioEvent ev)
        {
            if (ev == null) return;
            Point pt ;
            if (!ParentGraph.IsVisible(ev,out pt))
                return;
           // var binWidth = ParentGraph.SelectedChannelCount / ParentGraph.SelectedBinCount;
            //pt.X /= binWidth;	// convert into plot coordinate
            pt.X = (int)((pt.X - ParentGraph.XAxis.MinValue) / _xscale);
            _datas[(int)pt.X] = _datas[(int)pt.X] + 1D;
        }

        #endregion

        #endregion

    }
}
