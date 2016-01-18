using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Xml;
using ImageProcess;
using ImageProcess.DataType;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ComponentDataService.Types
{

    public class BioComponent
    {

        #region Fields

        public static readonly Blob[] EmptyBlobs = new Blob[0];
        public static readonly BioEvent[] EmptyEvents = new BioEvent[0];
        private const int DefaultBlobsCount = 25;
        private const int DefaultEventsCount = 25;
        private const int DefaultFeatureCount = 25;
        private const double Tolerance = 1e-6;
        private readonly Dictionary<int, List<Blob>> _contourBlobs = new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        private readonly Dictionary<int, List<Blob>> _dataBlobs = new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        private readonly Dictionary<int, List<Blob>> _backgroudBlobs = new Dictionary<int, List<Blob>>(DefaultBlobsCount);

        private readonly Dictionary<int, List<Blob>> _peripheralBlobs =
            new Dictionary<int, List<Blob>>(DefaultBlobsCount);

        private readonly Dictionary<int, List<BioEvent>> _eventsDict =
            new Dictionary<int, List<BioEvent>>(DefaultEventsCount);

        private string _basePath;
        private string _contourXmlPath;
        private readonly FeatureCollection _features = new FeatureCollection(DefaultFeatureCount);
        private readonly string _componentName = string.Empty;
        private readonly Dictionary<int, int> _eventsCountDict = new Dictionary<int, int>(DefaultEventsCount);
        private int _featureCount;
        private readonly List<Channel> _channels = new List<Channel>();
        private readonly IExperiment _experiment;
        private ImageData _contourImageData;
        private int _dataBlobCount;

        #endregion

        #region Delegate

        public delegate void WriteTileBlobsFile(string folder, int wellId, int tileId, BlobType type);

        public delegate void CopyTileBlobsFile(string folder, int wellId, int tileId, BlobType type);

        #endregion

        #region Properties

        public int ChannelCount
        {
            get { return Channels.Count; }
        }

        public IList<Channel> Channels
        {
            get { return _channels; }
        }

        public IList<Feature> Features
        {
            get { return _features; }
        }

        public int FeatureCount { get; private set; }
        public Version SoftwareVersion { get; set; }

        #endregion

        public BioComponent(IExperiment experiment, string name)
        {
            _experiment = experiment;
            _componentName = name;
            Init();
        }

        #region Methods

        #region Internal

        internal void Update(IList<Feature> features)
        {
            _features.Clear();
            _features.AddRange(features);
            ScanInfo info = _experiment.GetScanInfo(ComponentDataManager.ScanId);
            IList<Channel> physicalChannels = info.ChannelList;
            IList<VirtualChannel> virtualChannels = info.VirtualChannelList;
            _channels.Clear();
            _channels.AddRange(physicalChannels);
            _channels.AddRange(virtualChannels);
            int n = _features.Count;
            for (int i = 1; i < n; i++)
            {
                Feature prev = _features[i - 1];
                Feature cur = _features[i];
                cur.Index = prev.IsPerChannel ? prev.Index + ChannelCount : prev.Index + 1;
            }
            Feature last = _features.LastOrDefault();
            FeatureCount = last != null ? (last.IsPerChannel ? last.Index + ChannelCount : last.Index + 1) : 0;

            SaveToEvtXml(_basePath);
        }

        internal List<Blob> GetTileBlobs(int scanId, int wellId, int tileId, BlobType type)
        {
            int key = GetBlobKey(scanId, wellId, tileId);
            switch (type)
            {
                case BlobType.Contour:
                {
                    if (_contourBlobs.ContainsKey(key) == false)
                    {
                        List<Blob> blobs = ReadBlobsInfoFromBinary(wellId, tileId, type);
                        _contourBlobs[key] = blobs;
                    }
                    return _contourBlobs[key];
                }

                case BlobType.Data:
                {
                    if (_dataBlobs.ContainsKey(key) == false)
                    {
                        List<Blob> blobs = ReadBlobsInfoFromBinary(wellId, tileId, type);
                        _dataBlobs[key] = blobs;
                    }
                    return _dataBlobs[key];
                }
                case BlobType.Background:
                {
                    return _backgroudBlobs.ContainsKey(key) ? _backgroudBlobs[key] : EmptyBlobs.ToList();
                }
                case BlobType.Peripheral:
                    return _peripheralBlobs.ContainsKey(key) ? _peripheralBlobs[key] : EmptyBlobs.ToList();
                default:
                    throw new ArgumentException("invaild blob type");

            }

        }

        internal List<BioEvent> GetEvents(int scanId, int wellId)
        {
            if (_eventsDict.ContainsKey(wellId) == false)
            {
                _eventsDict[wellId] = ReadEventsFromBinary(wellId);
            }
            return _eventsDict[wellId];

        }

        internal void SaveTileBlobs(string baseFolder)
        {
            string folder = Path.Combine(baseFolder, _componentName);
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);
            WriteBlobsXml(folder);
            WriteBlobsBinary(folder);
        }

        internal void SaveEvents(string baseFolder)
        {
            string folder = Path.Combine(baseFolder, _componentName);
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);
            WriteEventsBinary(folder);
        }

        internal void SaveToEvtXml(string baseFolder)
        {
            string[] files = Directory.GetFiles(baseFolder, "*.evt.xml");
            string file = files.FirstOrDefault();
            if (string.IsNullOrEmpty(file))
                throw new FileNotFoundException(string.Format("no evt xml file found at {0}", baseFolder));
            var doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            if (root == null) return;
            var compsNode = root.SelectSingleNode("descendant::components") as XmlElement;
            if (compsNode == null) return;
            var compNode =
                root.SelectSingleNode(string.Format("descendant::component[@name='{0}']", _componentName)) as XmlElement;
            XmlElement newlyCompNode = CreateComponent(doc);
            newlyCompNode.AppendChild(CreateChannels(doc));
            newlyCompNode.AppendChild(CreateFeatures(doc));
            newlyCompNode.AppendChild(CreateWells(doc));
            if (compNode == null)
            {
                compsNode.AppendChild(newlyCompNode);
            }
            else
            {
                compsNode.ReplaceChild(newlyCompNode, compNode);
            }

        }

        internal IList<Blob> CreateContourBlobs(int scanId, int wellId, int tileId,
            ImageData data, double minArea, double maxArea = int.MaxValue)
        {
            IList<Blob> contours = data.FindContour(minArea, maxArea);
            int key = GetBlobKey(scanId, wellId, tileId);
            _contourBlobs[key] = contours.ToList();
            _contourImageData = data.Clone();
            return contours;
        }

        internal IList<BioEvent> CreateEvents(int scanId, int wellId, int tileId,
            IDictionary<string, ImageData> imageDict, BlobDefine define)
        {
            int key = GetBlobKey(scanId, wellId, tileId);
            List<Blob> contours = _contourBlobs[key];
            var evs = new List<BioEvent>(contours.Count);
            if (_eventsDict.ContainsKey(wellId) == false)
            {
                _eventsDict[wellId] = new List<BioEvent>();
            }
            List<BioEvent> stored = _eventsDict[wellId];
            int id = stored.Count;
            foreach (Blob contour in contours)
            {
                Blob dataBlob = contour.CreateExpanded(define.DataExpand, (int) _contourImageData.XSize,
                    (int) _contourImageData.YSize);
                if (dataBlob != null)
                {
                    dataBlob.Id = id;
                    id++;
                    BioEvent ev = CreateEvent(contour, dataBlob, define, imageDict, scanId, wellId, tileId);
                    evs.Add(ev);
                }

            }
            stored.AddRange(evs);
            return evs;
        }

        #endregion

        #region Private

        private void Init()
        {
            ScanInfo info = _experiment.GetScanInfo(ComponentDataManager.ScanId);
            string basePath = Path.Combine(info.DataPath, "carrier1", "EVT");
            if (Directory.Exists(basePath) == false)
                Directory.CreateDirectory(basePath);
            _basePath = basePath;
            _contourXmlPath = Path.Combine(_basePath, _componentName, "contours");
            if (Directory.Exists(_contourXmlPath) == false)
                Directory.CreateDirectory(_contourXmlPath);
            ParseEvtXml();
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }

        private void ParseEvtXml()
        {
            XmlElement root = GetRootOfEvtXml();
            string query = string.Format("descendant::component[@name='{0}']", _componentName);
            var compNode = root.SelectSingleNode(query) as XmlElement;
            if (compNode == null) return;
            var channelsNode = compNode.SelectSingleNode("descendant::channels") as XmlElement;
            _channels.AddRange(ReadChannelInfoFromXml(channelsNode));
            var featuresNode = compNode.SelectSingleNode("descendant::features") as XmlElement;
            _features.AddRange(ReadFeatureInfoFromXml(featuresNode));
            var wellsNode = compNode.SelectSingleNode("descendant::wells") as XmlElement;
            ReadEventsCountFromXml(wellsNode);
        }

        private int GetBlobKey(int scanId, int wellId, int tileId)
        {
            int scanIdBits = (scanId - 1) & 0x01; // 1 bit for scanId
            int wellIdBits = (wellId - 1) & 0x7FFF; //15 bits for wellId
            int tileIdBits = (tileId - 1) & 0xFFFF; //16 bits for tileId
            return (wellIdBits << 15) | (tileIdBits) | (scanIdBits << 31);
        }

        private List<Blob> ReadTileBlobsInfoFromXml(int wellId, int tileId, BlobType type)
        {
            string filename = string.Format("contours_{0}_{1}.xml", wellId, wellId);
            var doc = new XmlDocument();
            doc.Load(Path.Combine(_contourXmlPath, filename));
            var root = doc.DocumentElement;
            if (root == null) throw new XmlSyntaxException("no root element found");
            string query = string.Format("descendant::field[@no='{0}']", tileId);
            var fieldNode = root.SelectSingleNode(query) as XmlElement;
            if (fieldNode == null) return EmptyBlobs.ToList();
            else
            {
                if (type == BlobType.Data)
                {
                    query = string.Format("descendant::data-blobs");
                }
                else if (type == BlobType.Contour)
                {
                    query = string.Format("descendant::contour-blobs");
                }
                var countNode = fieldNode.SelectSingleNode(query) as XmlElement;
                if (countNode == null) return EmptyBlobs.ToList();
                int count = countNode.ParseAttributeToInt32("count");
                var blobs = new List<Blob>(count);
                var children = countNode.ChildNodes;
                foreach (XmlElement child in children)
                {
                    int id = child.ParseAttributeToInt32("id");
                    int capacity = child.ParseAttributeToInt32("point-count");
                    var blob = new Blob(capacity, id);
                    blobs.Add(blob);
                }
                return blobs;
            }
        }

        private List<Blob> ReadBlobsInfoFromBinary(int wellId, int tileId, BlobType type)
        {
            List<Blob> blobs = ReadTileBlobsInfoFromXml(wellId, tileId, type);
            string filename;
            if (type == BlobType.Contour)
            {
                filename = string.Format("t_{0}_{1}_{2}", wellId, wellId, tileId);
            }
            else if (type == BlobType.Data)
            {
                filename = string.Format("d_{0}_{1}_{2}", wellId, wellId, tileId);
            }
            else
            {
                return EmptyBlobs.ToList();
            }
            using (
                var reader =
                    new BinaryReader(File.Open(Path.Combine(_basePath, _componentName, filename), FileMode.Open,
                        FileAccess.Read)))
            {
                foreach (Blob blob in blobs)
                {
                    int n = blob.PointCapacity;
                    var points = new Point[n];
                    for (int i = 0; i < n; i++)
                    {
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        points[i] = new Point(x, y);
                    }
                    blob.AddContours(points);

                }
            }

            return blobs;
        }

        private XmlElement GetRootOfEvtXml()
        {
            string[] files = Directory.GetFiles(_basePath, "*.evt.xml");
            string file = files.FirstOrDefault();
            if (string.IsNullOrEmpty(file))
                throw new FileNotFoundException(string.Format("no evt xml file found at {0}", _basePath));
            var doc = new XmlDocument();
            doc.Load(file);
            return doc.DocumentElement;

        }

        private IEnumerable<Feature> ReadFeatureInfoFromXml(XmlElement featureNode)
        {
            var features = new FeatureCollection(DefaultFeatureCount);
            XmlNodeList children = featureNode.ChildNodes;
            foreach (XmlElement child in children)
            {
                string name = child.ParseAttributeToString("name");
                bool isPerChannel = child.ParseAttributeToBoolean("per-channel");
                bool isInteger = child.ParseAttributeToBoolean("integer");
                var feature = new Feature(name, isPerChannel, isInteger);
                features.Add(feature);
                feature.Index = _featureCount;
                _featureCount = isPerChannel ? _featureCount + ChannelCount : _featureCount + 1;
            }
            return features;
        }

        private void ReadEventsCountFromXml(XmlElement wellsNode)
        {
            XmlNodeList children = wellsNode.ChildNodes;
            foreach (XmlElement child in children)
            {
                var region = child.FirstChild as XmlElement;
                int no = region.ParseAttributeToInt32("no");
                int count = region.ParseAttributeToInt32("count");
                if (count >= 0)
                {
                    _eventsCountDict[no] = count;
                }
            }

        }

        private IEnumerable<Channel> ReadChannelInfoFromXml(XmlElement channelsNode)
        {
            var channels = new List<Channel>();
            XmlNodeList children = channelsNode.ChildNodes;
            foreach (XmlElement child in children)
            {
                int index = child.ParseAttributeToInt32("index");
                string name = child.ParseAttributeToString("label");
                var channel = new Channel {ChannelId = index, ChannelName = name};
                channels.Add(channel);
            }
            return channels;
        }

        private List<BioEvent> ReadEventsFromBinary(int wellId)
        {
            string filename = string.Format("{0}_{1}.evt", wellId, wellId);
            string filepath = Path.Combine(_basePath, _componentName, filename);
            if (File.Exists(filepath) == false)
                throw new FileNotFoundException(string.Format("{0} not found", filepath));
            else
            {
                using (var reader = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read)))
                {
                    int count = _eventsCountDict[wellId];
                    var evs = new List<BioEvent>(count);
                    for (int i = 0; i < count; i++)
                    {
                        var ev = new BioEvent(_featureCount);
                        for (int j = 0; j < _featureCount; j++)
                        {
                            ev[j] = reader.ReadSingle();
                        }
                        evs.Add(ev);
                    }
                    return evs;
                }
            }
        }


        private void WriteBlobsFile(string folder, WriteTileBlobsFile writer, CopyTileBlobsFile copier)
        {
            ScanInfo info = _experiment.GetScanInfo(ComponentDataManager.ScanId);
            IList<ScanRegion> regions = info.ScanRegionList;
            foreach (ScanRegion region in regions)
            {
                int wellId = region.WellId;
                IList<Scanfield> fields = region.ScanFieldList;
                foreach (Scanfield field in fields)
                {
                    int tileId = field.ScanFieldId;
                    int key = GetBlobKey(ComponentDataManager.ScanId, wellId, tileId);
                    if (_contourBlobs.ContainsKey(key))
                    {
                        writer(folder, wellId, tileId, BlobType.Contour);
                    }
                    else
                    {
                        copier(folder, wellId, tileId, BlobType.Contour);
                    }
                    if (_dataBlobs.ContainsKey(key))
                    {
                        writer(folder, wellId, tileId, BlobType.Data);
                    }
                    else
                    {
                        copier(folder, wellId, tileId, BlobType.Contour);
                    }
                }
            }
        }

        private void WriteBlobsXml(string folder)
        {
            string destDirName = Path.Combine(folder, "contours");
            string sourceDirName = Path.Combine(_basePath, "contours");
            bool needCopy = NormalizePath(folder) == NormalizePath(_basePath);
            if (needCopy)
            {
                DirectoryCopy(sourceDirName, destDirName, true);
            }
            WriteBlobsFile(destDirName, WriteBlobsXml, (s, id, tileId, type) => { });
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                //throw new DirectoryNotFoundException(
                //    "Source directory does not exist or could not be found: "
                //    + sourceDirName);
                return;
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, true);
                }
            }

        }

        private void WriteBlobsXml(string filepath, int wellId, int tileId, BlobType type)
        {
            var doc = new XmlDocument();
            string filename = Path.Combine(filepath,
                string.Format("contours_{0}_{1}.xml", wellId, wellId));
            doc.Load(filename);
            string query = string.Format("descendant::field[@no='{0}']", tileId);
            var fieldNode = doc.SelectSingleNode(query) as XmlElement;
            if (fieldNode != null)
            {
                switch (type)
                {
                    case BlobType.Contour:
                        query = string.Format("descendant::contour-blobs");
                        break;
                    case BlobType.Data:
                        query = string.Format("descendant::data-blobs");
                        break;
                    default:
                        return;
                }
                XmlElement newly = CreateBlobNode(doc, wellId, tileId, type);
                var blobNode = fieldNode.SelectSingleNode(query) as XmlElement;
                if (blobNode == null)
                    fieldNode.AppendChild(newly);
                else
                {
                    fieldNode.ReplaceChild(newly, blobNode);
                }
                doc.Save(filepath);
            }
        }

        private XmlElement CreateBlobNode(XmlDocument doc, int wellId, int tileId, BlobType type)
        {
            var blobName = string.Empty;
            List<Blob> blobs = EmptyBlobs.ToList();
            int key = GetBlobKey(ComponentDataManager.ScanId, wellId, tileId);
            switch (type)
            {
                case BlobType.Contour:
                    blobName = "contour-blobs";
                    blobs = _contourBlobs[key];
                    break;
                case BlobType.Data:
                    blobName = "data-blobs";
                    blobs = _dataBlobs[key];
                    break;
            }
            int n = blobs.Count;
            XmlElement blobNode = doc.CreateElement(blobName);
            blobNode.SetAttribute("count", n.ToString(CultureInfo.InvariantCulture));

            foreach (var b in blobs)
            {
                var node = doc.CreateElement("contour");
                node.SetAttribute("id", b.Id.ToString(CultureInfo.InvariantCulture));
                node.SetAttribute("shape", "contours");
                node.SetAttribute("point-count", b.PointCapacity.ToString(CultureInfo.InvariantCulture));
                blobNode.AppendChild(node);
            }
            return blobNode;
        }

        private void CopyBlobsBinary(string folder, int wellId, int tileId, BlobType type)
        {
            string filename;
            switch (type)
            {
                case BlobType.Contour:
                    filename = Path.Combine(folder,
                        string.Format("t_{0}_{1}_{2}", wellId, wellId, tileId));

                    break;
                case BlobType.Data:
                    filename = Path.Combine(folder,
                        string.Format("d_{0}_{1}_{2}", wellId, wellId, tileId));

                    break;
                default:
                    return;
            }
            string source = Path.Combine(_basePath, filename);
            string dest = Path.Combine(folder, filename);
            File.Copy(source, dest, false);
        }

        private void WriteBlobsBinary(string folder)
        {
            WriteBlobsFile(folder, WriteBlobsBinary, CopyBlobsBinary);
        }

        private void WriteBlobsBinary(string folder, int wellId, int tileId, BlobType type)
        {
            string filename;
            int key = GetBlobKey(ComponentDataManager.ScanId, wellId, tileId);
            List<Blob> blobs;
            switch (type)
            {
                case BlobType.Contour:
                    filename = Path.Combine(folder,
                        string.Format("t_{0}_{1}_{2}", wellId, wellId, tileId));
                    blobs = _contourBlobs[key];
                    break;
                case BlobType.Data:
                    filename = Path.Combine(folder,
                        string.Format("d_{0}_{1}_{2}", wellId, wellId, tileId));
                    blobs = _dataBlobs[key];
                    break;
                default:
                    return;
            }
            using (var writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                foreach (Blob blob in blobs)
                {
                    foreach (Point point in blob.PointsArray)
                    {
                        writer.Write((int) point.X);
                        writer.Write((int) point.Y);
                    }
                }
            }
        }

        private void WriteEventsBinary(string folder)
        {
            ScanInfo info = _experiment.GetScanInfo(ComponentDataManager.ScanId);
            IList<ScanRegion> regions = info.ScanRegionList;
            foreach (ScanRegion region in regions)
            {
                int wellId = region.WellId;
                if (_eventsDict.ContainsKey(wellId))
                {
                    WriteEventsBinary(folder, wellId);
                }
                else
                {
                    CopyEventsBinary(folder, wellId);
                }
            }
        }

        private void CopyEventsBinary(string folder, int wellId)
        {
            string filename = string.Format("{0}_{1}.evt", wellId, wellId);
            string source = Path.Combine(_basePath, filename);
            string dest = Path.Combine(folder, filename);
            File.Copy(source, dest, false);
        }

        private void WriteEventsBinary(string folder, int wellId)
        {
            List<BioEvent> evs = _eventsDict[wellId];
            string filename = Path.Combine(folder, string.Format("{0}_{1}.evt", wellId, wellId));
            using (var writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                foreach (BioEvent ev in evs)
                {
                    int n = ev.Buffer.Length;
                    for (int i = 0; i < n; i++)
                    {
                        writer.Write(ev[i]);
                    }
                }
            }
        }

        private XmlElement CreateComponent(XmlDocument doc)
        {
            ScanInfo info = _experiment.GetScanInfo(ComponentDataManager.ScanId);
            var compNode = doc.CreateElement("component");
            compNode.SetAttribute("name", _componentName);
            compNode.SetAttribute("pixel-width", info.XPixcelSize.ToString(CultureInfo.InvariantCulture));
            compNode.SetAttribute("pixel-height", info.YPixcelSize.ToString(CultureInfo.InvariantCulture));
            compNode.SetAttribute("blob-type", "Field");
            compNode.SetAttribute("scan-no", ComponentDataManager.ScanId.ToString(CultureInfo.InvariantCulture));
            compNode.SetAttribute("array", "false");
            return compNode;
        }

        private XmlElement CreateChannels(XmlDocument doc)
        {
            XmlElement chnsNode = doc.CreateElement("channels");
            foreach (Channel chn in Channels)
            {
                var node = doc.CreateElement("channel");
                node.SetAttribute("label", chn.ChannelName);
                node.SetAttribute("index", chn.ChannelId.ToString(CultureInfo.InvariantCulture));
                chnsNode.AppendChild(node);
            }
            return chnsNode;
        }

        private XmlElement CreateFeatures(XmlDocument doc)
        {
            XmlElement featuresNode = doc.CreateElement("features");
            foreach (var f in Features)
            {
                var node = doc.CreateElement("feature");
                node.SetAttribute("name", f.Name);
                node.SetAttribute("per-channel", f.IsPerChannel.ToString());
                node.SetAttribute("integer", f.IsInteger.ToString());
                featuresNode.AppendChild(node);
            }
            return featuresNode;
        }

        private XmlElement CreateWells(XmlDocument doc)
        {
            var wellsNode = doc.CreateElement("wells");
            foreach (KeyValuePair<int, List<BioEvent>> entry in _eventsDict)
            {
                int key = entry.Key;
                int count = entry.Value.Count;
                _eventsCountDict[key] = count;
            }
            foreach (KeyValuePair<int, int> entry in _eventsCountDict)
            {
                XmlElement node = doc.CreateElement("well");
                node.SetAttribute("no", entry.Key.ToString(CultureInfo.InvariantCulture));
                XmlElement rgnNode = doc.CreateElement("region");
                rgnNode.SetAttribute("no", entry.Key.ToString(CultureInfo.InvariantCulture));
                rgnNode.SetAttribute("count", entry.Value.ToString(CultureInfo.InvariantCulture));
                node.AppendChild(rgnNode);
                wellsNode.AppendChild(node);
            }
            return wellsNode;

        }

/*
        private List<Blob> GetDataBlobs(ImageData data, int expand, double minArea, double maxArea)
        {
            var dst = data.Dilate(expand);
            return dst.FindContour(minArea, maxArea).ToList();
        }
*/

        private Feature GetFeature(FeatureType type)
        {
            return Features.FirstOrDefault(f => f.FeatureType == type);
        }

        public BioEvent CreateEvent(Blob blobOrg, Blob blobData,
            BlobDefine define, IDictionary<string, ImageData> imageDict, int scanId, int wellId, int tileId)
            // jcl-cycCleanup , bool changedCycle) // jcl-cycles
        {
            ScanInfo info = _experiment.GetScanInfo(ComponentDataManager.ScanId);
            ScanRegion regions = info.ScanRegionList[wellId - 1];
            Scanfield field = regions.ScanFieldList[tileId - 1];
            double pixelWidth = info.XPixcelSize;
            double pixelHeight = info.YPixcelSize;
            double area = blobOrg.Area*pixelWidth*pixelHeight;
            if (Math.Abs(area) < Tolerance) return null;

            Blob bkBlob = define.IsDynamicBackground ? CreateBackgroundBlob(blobData, define) : null;
            if (define.IsDynamicBackground && bkBlob == null) // unable to create background blob
                return null;

            // create peripheral blob if peripheral feature is selected
            Blob periBlob = define.IsPeripheral ? CreatePeripheralBlob(blobOrg, define) : null;
            if (define.IsPeripheral && periBlob == null)
                return null;

            // Create event
            var ev = new BioEvent(FeatureCount);

            // compute features			
            // per-channel features first
            for (int i = 0; i < ChannelCount; i++)
            {
                Channel channel = Channels[i];
                // dynamic background
                int bkgnd = 0;
                if (bkBlob != null)
                {
                    bool correctBk = define.DynamicBkCorrections[i];
                    if (correctBk)
                    {
                        bkgnd = bkBlob.ComputeDynamicBackground(imageDict[channel.ChannelName],
                            define.BackgroundLowBoundPercent, define.BackgroundLowBoundPercent, 200);


                        Feature fb = GetFeature(FeatureType.Background);
                        if (fb != null)
                            ev[fb.Index + i] = bkgnd;
                    }
                }

                // integral, max-pixel
                int maxPixel;

                float integral = ComputeIntegral(imageDict[channel.ChannelName], blobData, bkgnd, out maxPixel);


                Feature fi = GetFeature(FeatureType.Integral);
                ev[fi.Index + i] = integral;

                Feature fintensity = GetFeature(FeatureType.Intensity);
                if (fintensity != null)
                    ev[fintensity.Index + i] = integral/blobData.Area;

                //YAK 4-8-2011: stdv of the intensities

                float stdv = ComputeStdv(imageDict[channel.ChannelName], blobData, bkgnd, integral/blobData.Area);

                Feature fstdv = GetFeature(FeatureType.Stdv);
                if (fstdv != null)
                    ev[fstdv.Index + i] = stdv;
                //////////////////////////////////////

                Feature fmax = GetFeature(FeatureType.MaxPixel);
                if (fmax != null)
                    ev[fmax.Index + i] = maxPixel;

                // peripheral
                if (periBlob != null)
                {


                    var periIntegral =
                        (int) ComputeIntegral(imageDict[channel.ChannelName], periBlob, bkgnd, out maxPixel);

                    Feature fp = GetFeature(FeatureType.PeripheralIntegral);
                    if (fp != null)
                        ev[fp.Index + i] = periIntegral;

                    fp = GetFeature(FeatureType.PeripheralIntensity); // jcl-7492
                    if (fp != null)
                        ev[fp.Index + i] = periIntegral/(float) periBlob.Area;

                    fp = GetFeature(FeatureType.PeripheralMax);
                    if (fp != null)
                        ev[fp.Index + i] = maxPixel;
                }
            }


            // common features

            Point center = blobData.Centroid();
            double px = field.SFRect.X + (_contourImageData.XSize - center.X)*pixelWidth;
            double py = field.SFRect.Y + center.Y*pixelHeight;
            ev[GetFeature(FeatureType.XPos).Index] = (int) px;
            ev[GetFeature(FeatureType.YPos).Index] = (int) py;

            Feature f = GetFeature(FeatureType.Area);
            if (f != null)
                ev[f.Index] = (float) area;

            f = GetFeature(FeatureType.Time);
            if (f != null)
                ev[f.Index] = 0;

            f = GetFeature(FeatureType.Scan);
            if (f != null)
                ev[f.Index] = (float) blobData.Centroid().Y;

            int perimeter = blobOrg.Perimeter(pixelWidth, pixelHeight); // perimeter on threshold blob
            f = GetFeature(FeatureType.Perimeter);
            if (f != null)
                ev[f.Index] = perimeter;

            f = GetFeature(FeatureType.Circularity);
            if (f != null)
                ev[f.Index] = perimeter*(float) perimeter/(float) area;

            f = GetFeature(FeatureType.HalfRadius);
            if (f != null)
                ev[f.Index] = (float) area/perimeter; //the old diameter value (which is actually diameter/4)

            f = GetFeature(FeatureType.Diameter);
            if (f != null)
                ev[f.Index] = (float) (area/perimeter)*4;
                    //(Pi * R^2) / (2 * Pi * R)  = A/P = Diameter/4..so we multiply by 4 to get the correct diameter.

            float xMean;
            float yMean;
            float oxx;
            float oyy;
            float oxy;
            blobOrg.ComputeXyMean(out xMean, out yMean);
            blobOrg.ComputeCovarianceElements(xMean, yMean, out oxx, out oyy, out oxy);
            //YAK 5_11_2011: convert the mean vector and Cov matrix into microns
            oxx *= (float) (pixelWidth*pixelWidth);
            oyy *= (float) (pixelHeight*pixelHeight);
            oxy *= (float) (pixelWidth*pixelHeight);
            //End conversion///////////////////////////////

            float lambda0 = ((oxx + oyy)/2) - ((float) Math.Sqrt(4*oxy*oxy + ((oxx - oyy)*(oxx - oyy)))/2);
            float lambda1 = ((oxx + oyy)/2) + ((float) Math.Sqrt(4*oxy*oxy + ((oxx - oyy)*(oxx - oyy)))/2);

            float majorAxis = 4*(float) Math.Sqrt(lambda1);
            float minorAxis = 4*(float) Math.Sqrt(lambda0);

            f = GetFeature(FeatureType.MajorAxis);
            if (f != null)
                ev[f.Index] = majorAxis;

            f = GetFeature(FeatureType.MinorAxis);
            if (f != null)
                ev[f.Index] = minorAxis;

            //YAK 6-17-2011: When the object is very small (or very thin) a rounding error may occure that may result in a zero minor axis. 
            //Fix that using a lower bound on the minor axis which should be at least one pixel but converted into microns
            var mn = (float) Math.Min(pixelWidth, pixelHeight);
            if (Math.Abs(minorAxis) < Tolerance)
                minorAxis = mn;

            //YAK 4-11-2011: The following two features (elongation and eccentricity) are computed using the major and minor axes
            float elongation = majorAxis/minorAxis;
            var eccentricity = (float) Math.Sqrt((lambda1 - lambda0)/lambda1);
            f = GetFeature(FeatureType.Elongation);
            if (f != null)
                ev[f.Index] = elongation;

            f = GetFeature(FeatureType.Eccentricity);
            if (f != null)
                ev[f.Index] = eccentricity;
            //////////////////////////////////////////

            int wno = wellId;
            ev[GetFeature(FeatureType.WellNo).Index] = wno;

            f = GetFeature(FeatureType.RegionNo);
            if (f != null)
                ev[f.Index] = wellId;


            // event should be added to the list after all the values have been set in order for the min/max to be updated correctly

            if (blobData.Id != 0)
                // editing existing blob, set event id to the blob id so that the new event replaces the original event when added to the list
                ev.Id = blobData.Id;
            ev.DataBlob = blobData;
            ev.ContourBlob = blobOrg;
            blobOrg.Id = blobData.Id = ev.Id;
                // set blob id equal to the event id (1-based event id is set when an event is added to the list)
            int key = GetBlobKey(scanId, wellId, tileId);
            if (bkBlob != null)
            {
                bkBlob.Id = ev.Id;
                ev.BackgroundBlob = bkBlob;
                if (_backgroudBlobs.ContainsKey(key) == false)
                {
                    _backgroudBlobs[key] = new List<Blob>(DefaultBlobsCount);
                }
                _backgroudBlobs[key].Add(bkBlob);

            }
            if (periBlob != null)
            {
                periBlob.Id = ev.Id;
                ev.PeripheralBlob = periBlob;
                if (_peripheralBlobs.ContainsKey(key) == false)
                {
                    _peripheralBlobs[key] = new List<Blob>(DefaultBlobsCount);
                }
                _peripheralBlobs[key].Add(periBlob);

            }


            return ev;


        }

        private Blob CreateBackgroundBlob(Blob blobData, BlobDefine define)
        {
            Blob bkBlob = blobData.CreateRing(define.BackgroundDistance, define.BackgroundWidth, false,
                (int) _contourImageData.XSize, (int) _contourImageData.YSize);

            return bkBlob;
        }

        private Blob CreatePeripheralBlob(Blob blobOrg, BlobDefine define)
        {
            Blob periBlob = blobOrg.CreateRing(define.PeripheralDistance, define.PeripheralDistance, false,
                (int) _contourImageData.XSize, (int) _contourImageData.YSize);
            return periBlob;
        }


        public float ComputeIntegral(ImageData data, Blob blob, int bkgnd, out int maxPixel)
        {
            maxPixel = 0;

            if (blob.Area <= 0) return 0;

            int height = blob.Bound.Height;
            var yValues = new int[height];

            //int xOffset = GetOffset(channel);
            //ushort[,] buf = GetBuffer(channel);
            var maxY = (int) data.YSize;
            var width = (int) data.XSize;
            float integral = 0;

            foreach (VLine line in blob.Lines)
            {
                int offset = line.Y1 - blob.Bound.Y;
                if (line.X < 0 || line.X >= width || line.Y1 + line.Length >= maxY)
                    continue; // can happen when region was created interactively

                for (int i = 0; i < line.Length; i++)
                {
                    //yValues[offset + i] += (nTmp = buf[xOffset + line.X, line.Y1 + i] - bkgnd);
                    int nTmp;
                    yValues[offset + i] += (nTmp = data[(line.Y1 + i)*width + line.X] - bkgnd);

                    if (nTmp > maxPixel)
                    {
                        maxPixel = nTmp;
                    }
                }
            }


            for (int i = 0; i < height; i++)
                integral += yValues[i];

            //m_maxPixelPos.X = peakX;
            //m_maxPixelPos.Y = peakY;
            return integral;
        }


        //YAK 4-8-2011: compute the standard deviation of the intensities of the blob
        public float ComputeStdv(ImageData data, Blob blob, int bkgnd, float meanInt)
        {
            if (blob.Area <= 0) return 0;

            int height = blob.Bound.Height;
            var yValues = new float[height];

            //int xOffset = GetOffset(channel);
            //ushort[,] buf = GetBuffer(channel);
            var maxY = (int) data.YSize;
            var width = (int) data.XSize;
            float stdv = 0;
            int numPixels = 0;
            foreach (VLine line in blob.Lines)
            {
                int offset = line.Y1 - blob.Bound.Y;
                if (line.X < 0 || line.X >= width || line.Y1 + line.Length >= maxY)
                    continue; // can happen when region was created interactively

                for (int i = 0; i < line.Length; i++)
                {
                    numPixels++;
                    //double tmpN = (buf[xOffset + line.X, line.Y1 + i] - bkgnd - MeanInt) * (buf[xOffset + line.X, line.Y1 + i] - bkgnd - MeanInt);
                    double tmpN = (data[(line.Y1 + 1)*width + line.X] - bkgnd - meanInt)*
                                  (data[(line.Y1 + 1)*width + line.X] - bkgnd - meanInt);
                    yValues[offset + i] += (float) tmpN;
                    // (nTmp = buf[xOffset + line.X, line.Y1 + i] - bkgnd);                  
                }
            }


            for (int i = 0; i < height; i++)
                stdv += yValues[i];

            stdv /= numPixels;
            stdv = (float) Math.Sqrt(stdv);
            return stdv;
        }

        #endregion

        #endregion


    }
}
