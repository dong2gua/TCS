using ComponentDataService.Types;
using ImageProcess;
using ImageProcess.DataType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ComponentDataService
{
    public enum EventSource
    {
        FromDisk = 0,
        FromMemory
    };

    public class ComponentDataManager : IComponentDataService
    {
        #region Fields

        internal const string DataStoredFolder = "EVT";
        internal const string ContourXmlFolder = "contours";
        private const int DefaultSize = 5;
        private string _basePath;
        private IExperiment _experiment;
        private readonly Dictionary<string, BioComponent> _bioComponentDict =
            new Dictionary<string, BioComponent>(DefaultSize);

        private static readonly ComponentDataManager InstanceField;
        private EventSource _eventSource;
        #endregion

        #region Constructors

        static ComponentDataManager()
        {
            InstanceField = new ComponentDataManager();
        }

        private ComponentDataManager()
        {
            //RegisterMessage();
        }
        #endregion

        #region Properties

        public static ComponentDataManager Instance
        {
            get { return InstanceField; }
        }

        #endregion

        #region Methods

        #region Private

        private void RegisterMessage()
        {
            IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            if (eventAggregator != null)
            {
                eventAggregator.GetEvent<ExperimentLoadedEvent>()
                    .Subscribe(e => { _eventSource = EventSource.FromDisk; });
                eventAggregator.GetEvent<MacroRunEvent>().Subscribe(e => { _eventSource = EventSource.FromMemory; });
            }
        }

        private IEnumerable<string> GetComponentNamesFromXml()
        {
            var names = new List<string>();
            var doc = new XmlDocument();
            string[] files = Directory.GetFiles(_basePath, "*.evt.xml");
            string file = files.FirstOrDefault();
            if (string.IsNullOrEmpty(file)) return new List<string>(0);
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            if (root == null) throw new XmlSyntaxException("no root element found");
            var comps = root.SelectSingleNode("descendant::components");
            if (comps == null) return names;
            var children = comps.ChildNodes;
            names.AddRange(from XmlElement child in children select child.ParseAttributeToString("name"));
            return names;
        }

        private void SaveEvtXml(string fileFolder)
        {           
            foreach (BioComponent bioComponent in _bioComponentDict.Values)
            {
                bioComponent.SaveToEvtXml(fileFolder);
            }
        }


        private static void Association(IEnumerable<Blob> masterDataBlobs, IList<BioEvent> masterEvents,
            IList<Blob> slaveDataBlobs, IList<BioEvent> slavEvents)
        {
            foreach (Blob masterDataBlob in masterDataBlobs)
            {
                foreach (Blob slaveDataBlob in slaveDataBlobs)
                {
                    if (masterDataBlob.Contains(slaveDataBlob))
                    {
                        BioEvent master = masterEvents.BinarySearch(masterDataBlob.EventId);
                        BioEvent slave = slavEvents.BinarySearch(slaveDataBlob.EventId);
                        if (master != null && slave != null)
                        {
                            master.Children.Add(slave);
                            slave.Parent = master;
                        }
                    }
                }
            }
        }

        private bool CheckPhantomDefine(PhantomDefine define)
        {
            if (define.Radius <= 0 || define.Distance <= 0) return false;
            if (define.Pattern == PhantomDefine.PhantomPattern.Random) return define.Count > 0;
            return true;
        }

        #endregion

        #region Methods in Interface

        public IList<string> GetComponentNames()
        {
            return _bioComponentDict.Keys.ToList();
        }

        public void Load(IExperiment experiment)
        {
            _experiment = experiment;
            ExperimentInfo info = experiment.GetExperimentInfo();
            string analysisPath = info.AnalysisPath;
            _basePath = Path.Combine(analysisPath, DataStoredFolder);
            if (Directory.Exists(_basePath) == false) return;//for not analysed data
                         
            ClearComponents();
            IEnumerable<string> componentNames = GetComponentNamesFromXml();       
            foreach (string name in componentNames)
            {
                _bioComponentDict[name] = new RealComponent(experiment, name);
            }
        }

        public IList<Blob> GetBlobs(string componentName,int wellId, int tileId, BlobType type)
        {          
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            BioComponent comp = _bioComponentDict[componentName];
            int scanId = comp.ScanId;
            ScanInfo info = _experiment.GetScanInfo(scanId);
            int scanRegionCount = info.ScanRegionList.Count;
            if (wellId < 0 || wellId >= scanRegionCount)
                throw new ArgumentOutOfRangeException("wellId");
            int tileCount = info.ScanRegionList[wellId].ScanFieldList.Count;
            if (tileId <= 0 || tileId > tileCount) throw new ArgumentOutOfRangeException("tileId");
            try
            {          
                return comp.GetTileBlobs(wellId, tileId, type);
            }
            catch (FileNotFoundException)
            {
                return BioComponent.EmptyBlobs;
            }
            
        }

        public IList<BioEvent> GetEvents(string componentName, int wellId)
        {
           
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            BioComponent comp = _bioComponentDict[componentName];
            int scanId = comp.ScanId;
            ScanInfo info = _experiment.GetScanInfo(scanId);
            int scanRegionCount = info.ScanRegionList.Count;
            if (wellId <= 0 || wellId > scanRegionCount)
                throw new ArgumentOutOfRangeException("wellId");
            try
            {
                return comp.GetEvents(wellId, _eventSource);
            }
            catch (FileNotFoundException)
            {
                return BioComponent.EmptyEvents;
            }
        }

        public IList<Feature> GetFeatures(string componentName)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            BioComponent comp = _bioComponentDict[componentName];
                return comp.Features;
           
        }

        public int GetFeatureIndex(string componentName, FeatureType type, string channelName = null)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            if (type == FeatureType.None) return -1;
            BioComponent component = _bioComponentDict[componentName];
            IList<Feature> features = component.Features;
            IList<Channel> channels = component.Channels;
            int channelId = 0;
            if (string.IsNullOrEmpty(channelName) == false)
            {
                Channel channel = channels.FirstOrDefault(chn => chn.ChannelName == channelName);
                if (channel != null)
                {
                    channelId = channel.ChannelId;
                }
                else
                    throw new ArgumentException("invaild channel name", "channelName");
            }
            Feature feature = features.FirstOrDefault(f => f.FeatureType == type);
            if (feature == null) throw new ArgumentException("invaild feature type", "type");
            return feature.IsPerChannel ? feature.Index + channelId : feature.Index;
        }

        public void Save(string fileFolder)
        {
            string evtFolder = Path.Combine(fileFolder, DataStoredFolder);
            if (Directory.Exists(evtFolder) == false)
                Directory.CreateDirectory(evtFolder);
            foreach (BioComponent bioComponent in _bioComponentDict.Values)
            {
                string componentFolder = Path.Combine(evtFolder, bioComponent.Name);
                if (Directory.Exists(componentFolder) == false)
                    Directory.CreateDirectory(componentFolder);
                string contourFolder = Path.Combine(componentFolder, ContourXmlFolder);
                if (Directory.Exists(contourFolder) == false)
                    Directory.CreateDirectory(contourFolder);              
                bioComponent.SaveTileBlobs(componentFolder);             
                bioComponent.SaveEvents(componentFolder);
            
            }
            SaveEvtXml(evtFolder);
        }

        public void AddComponent(string componentName, IList<Feature> features)
        {
            AddComponent(componentName, features, false);
        }

       

        public void ClearComponents()
        {
           _bioComponentDict.Clear();
        }

        public IList<Blob> CreateContourBlobs(string componentName, int scanId, int wellId, int tileId,
            ImageData data, double minArea, double maxArea = int.MaxValue)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            RealComponent component = _bioComponentDict[componentName] as RealComponent;
            if (component != null)
            {
                return component.CreateContourBlobs(wellId, tileId, data, minArea, maxArea);             
            }
            else
            {
                throw new InvalidOperationException("invaild opeation in phantom");
            }
        }

        public IList<BioEvent> CreateEvents(string componentName, int scanId, int wellId, int tileId,
            IDictionary<string, ImageData> imageDict, BlobDefine define)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            BioComponent component = _bioComponentDict[componentName];
            return component.CreateEvents(wellId, tileId, imageDict, define);
        }

        public int GetComponentScanId(string componentName)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            return _bioComponentDict[componentName].ScanId;
        }


        public void Association(string masterComponentName, string slaveComponentName)
        {
            if (_bioComponentDict.ContainsKey(masterComponentName) == false ||
                _bioComponentDict.ContainsKey(slaveComponentName) == false)
                throw new ArgumentException("invaild component name");
            BioComponent master = _bioComponentDict[masterComponentName];
            BioComponent slave = _bioComponentDict[slaveComponentName];
            ScanInfo info = _experiment.GetScanInfo(master.ScanId);
            IList<ScanRegion> regions = info.ScanRegionList;
            foreach (ScanRegion region in regions)
            {
                int wellId = region.WellId;
                IList<Scanfield> fields = region.ScanFieldList;
                foreach (Scanfield field in fields)
                {
                    int tileId = field.ScanFieldId;
                    IList<Blob> masterDataBlob = master.GetTileBlobs(wellId, tileId, BlobType.Data);
                    IList<Blob> slaveDataBlob = slave.GetTileBlobs(wellId, tileId, BlobType.Data);
                    IList<BioEvent> slaveEvents = slave.GetEvents(wellId, _eventSource);
                    IList<BioEvent> masterEvents = master.GetEvents(wellId, _eventSource);
                    Association(masterDataBlob, masterEvents, slaveDataBlob, slaveEvents);
                }
            }

        }

        public void Association(string masterComponentName, string firstSlaveComponentName,
            string secondSlaveComponentName)
        {
            Association(masterComponentName, firstSlaveComponentName);
            Association(masterComponentName, secondSlaveComponentName);
        }

        public IList<Channel> GetChannels(string componentName)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            return _bioComponentDict[componentName].Channels;
        }

        
        #endregion
        public void AddComponent(string componentName, IList<Feature> features, bool isPhantom)
        {
            BioComponent component = isPhantom
                ? (BioComponent)new PhantomComponent(_experiment, componentName)
                : new RealComponent(_experiment, componentName);
            component.Update(features);
            _bioComponentDict[componentName] = component;
        }

        public IList<Blob> CreatePhantomBlobs(string componentName, int wellId, int tileId, PhantomDefine define)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
            {
                throw new ArgumentException("invaild componenet name", "componentName");
            }
            PhantomComponent component = _bioComponentDict[componentName] as PhantomComponent;
            if (component == null)
                throw new InvalidOperationException("can not CreatePhantomBlobs on non-Phantom component");
            else if (CheckPhantomDefine(define) == false)
                throw new ArgumentException("invaild phantom define", "define");
            return component.CreatePhantomBlobs(wellId, tileId, define);
        }


        public BitmapSource Draw(ImageData data, IList<Blob> blobs, Color color)
        {
            throw new NotImplementedException();
        }

        public BitmapSource Draw(BitmapSource source, IList<Blob> blobs, Color color)
        {
            byte[] buffer = source.GetBuffer();
            int height = source.PixelHeight;
            int width = source.PixelWidth;
            int stride = buffer.Length/height;
            int channels = stride/width;
           
            foreach (Blob blob in blobs)
            {
                Point[] points = blob.PointsArray;
                foreach (Point point in points)
                {
                    int x = (int) point.X;
                    int y = (int) point.Y;
                    int index = y*stride + x*channels;
                    buffer[index] = color.B;
                    buffer[index + 1] = color.G;
                    buffer[index + 2] = color.R;
                    buffer[index + 3] = byte.MaxValue;
                }
            }
            BitmapSource bmp = BitmapSource.Create(width, height, source.DpiX, source.DpiY, source.Format,
                source.Palette, buffer, stride);
            return bmp;
        }

        #endregion

    }

    internal static class Utils
    {
        #region XmlElement
        internal static double ParseAttributeToDouble(this XmlElement element, string attribute)
        {
            double value = 0;
            if (element == null) return value;
            else
            {
                if (element.HasAttribute(attribute))
                {
                    string text = element.Attributes[attribute].InnerText;
                    double.TryParse(text, out value);

                }
                return value;
            }
        }

        internal static int ParseAttributeToInt32(this XmlElement element, string attribute)
        {
            int value = 0;
            if (element == null) return value;
            else
            {
                if (element.HasAttribute(attribute))
                {
                    string text = element.Attributes[attribute].InnerText;
                    int.TryParse(text, out value);
                }
                return value;
            }
        }

        internal static string ParseAttributeToString(this XmlElement element, string attribute)
        {
            var value = string.Empty;
            if (element == null) return value;
            else
            {
                if (element.HasAttribute(attribute))
                {
                    value = element.Attributes[attribute].InnerText;
                }
                return value;
            }
        }

        internal static bool ParseAttributeToBoolean(this XmlElement element, string attribute)
        {
            bool value = false;
            if (element == null) return false;
            else
            {
                if (element.HasAttribute(attribute))
                {
                    string text = element.Attributes[attribute].InnerText;
                    bool.TryParse(text, out value);
                }
                return value;
            }
        }
        #endregion

        #region IList<T>

        public static BioEvent BinarySearch(this IList<BioEvent> bioEvents, int id)
        {
            int n = bioEvents.Count;
            int low = 0;
            int hi = n - 1;
            while (low<=hi)
            {
                int mid = (hi - low) >> 1 + low;
                int curId = bioEvents[mid].Id;
                if (curId == id)
                {
                    return bioEvents[mid];
                }                   
                else if (curId > id)
                {
                    hi = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
                    
            }
            return null;
        }

        #endregion

        #region BitmapSource
        internal static void GetDataBuffer(this ImageData image, BitmapSource source)
        {
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            // Create data array to hold source pixel data
            var data = new byte[stride * source.PixelHeight];
            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);
            short[] array = Array.ConvertAll(data, b => (short)(b << 6));
            Marshal.Copy(array, 0, image.DataBuffer, array.Length);
        }

        internal static byte[] GetBuffer(this BitmapSource source)
        {
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            // Create data array to hold source pixel data
            var data = new byte[stride * source.PixelHeight];
            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);
            return data;
        }
        #endregion
    }


}
