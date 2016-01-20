using ComponentDataService.Types;
using ImageProcess;
using ImageProcess.DataType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ComponentDataService
{
    public class ComponentDataManager : IComponentDataService
    {
        #region Fields

        internal const int ScanId = 1;
        private const int DefaultSize = 5;
        private string _basePath;
        private readonly List<string> _componentNames = new List<string>(DefaultSize);
        private IExperiment _experiment;
        private readonly Dictionary<string, BioComponent> _bioComponentDict =
            new Dictionary<string, BioComponent>(DefaultSize);

        private static readonly ComponentDataManager InstanceField;
        #endregion

        #region Constructors

        static ComponentDataManager()
        {
            InstanceField = new ComponentDataManager();
        }

        private ComponentDataManager()
        {
            
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

        private List<string> GetComponentNamesFromXml()
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
            string[] files = Directory.GetFiles(_basePath, "*.evt.xml");
            string file = files.FirstOrDefault();
            if (string.IsNullOrEmpty(file))
                throw new FileNotFoundException(string.Format("no evt xml found at {0}", _basePath));
            else
            {
                string destFileName = Path.Combine(fileFolder, Path.GetFileName(file));
                File.Copy(file, destFileName);
            }
            foreach (BioComponent bioComponent in _bioComponentDict.Values)
            {
                bioComponent.SaveToEvtXml(fileFolder);
            }
        }

        private void Clear()
        {
            _componentNames.Clear();
            _bioComponentDict.Clear();
        }

        #endregion

        #region Methods in Interface

        public IList<string> GetComponentNames()
        {
            return _componentNames.Count > 0 ? _componentNames : GetComponentNamesFromXml();
        }

        public void Load(IExperiment experiment)
        {
            _experiment = experiment;
            ScanInfo info = experiment.GetScanInfo(ScanId);
            string dataPath = info.DataPath;
            _basePath = Path.Combine(dataPath, "carrier1", "EVT");
            if (Directory.Exists(_basePath) == false)
                throw new DirectoryNotFoundException(string.Format("{0} not found", _basePath));
            Clear();        
            _componentNames.AddRange(GetComponentNamesFromXml());
            string softwareVersion = experiment.GetExperimentInfo().SoftwareVersion;
            foreach (string name in _componentNames)
            {
                _bioComponentDict[name] = new BioComponent(experiment, name)
                {
                    SoftwareVersion = new Version(softwareVersion)
                };
            }
        }

        public IList<Blob> GetBlobs(string componentName,int wellId, int tileId, BlobType type)
        {          
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            ScanInfo info = _experiment.GetScanInfo(ScanId);
            int scanRegionCount = info.ScanRegionList.Count;
            if (wellId < 0 || wellId >= scanRegionCount)
                throw new ArgumentOutOfRangeException("wellId");
            int tileCount = info.ScanRegionList[wellId].ScanFieldList.Count;
            if (tileId <= 0 || tileId > tileCount) throw new ArgumentOutOfRangeException("tileId");
            try
            {
                BioComponent comp = _bioComponentDict[componentName];
                return comp.GetTileBlobs(ScanId, wellId, tileId, type);
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
            ScanInfo info = _experiment.GetScanInfo(ScanId);
            int scanRegionCount = info.ScanRegionList.Count;
            if (wellId <= 0 || wellId > scanRegionCount)
                throw new ArgumentOutOfRangeException("wellId");
            try
            {
                BioComponent comp = _bioComponentDict[componentName];
                return comp.GetEvents(ScanId, wellId);
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

        public void SaveBlobs(string fileFolder)
        {
            foreach (BioComponent bioComponent in _bioComponentDict.Values)
            {
                bioComponent.SaveTileBlobs(fileFolder);
            }
        }

        public void SaveEvents(string fileFolder)
        {
            foreach (BioComponent bioComponent in _bioComponentDict.Values)
            {
                bioComponent.SaveEvents(fileFolder);
            }
            SaveEvtXml(fileFolder);
        }

        public void AddComponent(string componentName, IList<Feature> features)
        {
            var component = new BioComponent(_experiment, componentName);
            component.Update(features);
            _bioComponentDict[componentName] = component;
        }

        public void ClearComponents()
        {
            Clear();
        }

        public IList<Blob> CreateContourBlobs(string componentName, int scanId, int wellId, int tileId,
            ImageData data, double minArea, double maxArea = int.MaxValue)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            BioComponent component = _bioComponentDict[componentName];
            return component.CreateContourBlobs(scanId, wellId, tileId, data, minArea, maxArea);
        }

        public IList<BioEvent> CreateEvents(string componentName, int scanId, int wellId, int tileId,
            IDictionary<string, ImageData> imageDict, BlobDefine define)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            BioComponent component = _bioComponentDict[componentName];
            return component.CreateEvents(scanId, wellId, tileId, imageDict, define);
        }
        

        #endregion


        public IList<Channel> GetChannels(string componentName)
        {
            if (_bioComponentDict.ContainsKey(componentName) == false)
                throw new ArgumentException("invaild componenet name", "componentName");
            return _bioComponentDict[componentName].Channels;
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
    }


}
