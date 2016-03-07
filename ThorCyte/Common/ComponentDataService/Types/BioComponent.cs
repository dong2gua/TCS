using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml;
using ImageProcess;
using ImageProcess.DataType;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ComponentDataService.Types
{
    internal abstract class BioComponent
    {

        #region Fields
        public static readonly Blob[] EmptyBlobs = new Blob[0];
        public static readonly BioEvent[] EmptyEvents = new BioEvent[0];
        public const int DefaultBlobsCount = 25;
        public const int DefaultEventsCount = 25;
        public const int DefaultFeatureCount = 25;
        public const int DefaultChannelCount = 5;
        public const double Tolerance = 1e-6;      
        private readonly Dictionary<int, List<BioEvent>> _eventsDict =
            new Dictionary<int, List<BioEvent>>(DefaultEventsCount);
        private readonly FeatureCollection _features = new FeatureCollection(DefaultFeatureCount);
        private readonly string _componentName;
        private readonly Dictionary<int, int> _eventsCountDict = new Dictionary<int, int>(DefaultEventsCount);        
        private readonly List<Channel> _channels = new List<Channel>(DefaultChannelCount);
        private readonly IExperiment _experiment;       
        private readonly int _scanId;
        private short[] _imageBuffer;
        private int _featureCount;
        #endregion

        #region Delegate
        public delegate void WriteTileBlobsFile(string folder, int wellId, int tileId, BlobType type);
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
        public Version SoftwareVersion { get; private set; }

        public int ScanId
        {
            get { return _scanId; }
        }

        public string Name
        {
            get { return _componentName; }
        }

        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }


        //protected string ContourXmlPath { get; private set; }

        protected IExperiment Experiment
        {
            get { return _experiment; }
        }

        protected string BasePath { get; private set; }

        protected IDictionary<int, List<BioEvent>> EventsDict
        {
            get { return _eventsDict; }
        }

        #endregion

        #region Constructors
        protected BioComponent(IExperiment experiment, string name) : this(experiment, name, 1)
        {           
        }

        protected BioComponent(IExperiment experiment, string name, int scanId)
        {
            _experiment = experiment;
            _componentName = name;
            _scanId = scanId;
            Init();
        }

        #endregion

        #region Methods

        #region Internal

        internal void Update(IList<Feature> features)
        {
            _channels.Clear();
            _channels.AddRange(ResortChannels());
            _features.Clear();
            ResortFeatures(features);
            _features.AddRange(features);
        }

        internal List<BioEvent> GetEvents(int wellId, EventSource source)
        {         
            if (_eventsDict.ContainsKey(wellId) == false)
            {
                switch (source)
                {
                    case EventSource.FromDisk:
                        _eventsDict[wellId] = ReadEventsFromBinary(wellId);
                        return _eventsDict[wellId];
                    case EventSource.FromMemory:
                        return EmptyEvents.ToList();
                }
                
            }
            return _eventsDict[wellId];
        }

        internal void SaveEvents(string baseFolder)
        {
            WriteEventsBinary(baseFolder);
        }

        internal void SaveToEvtXml(string baseFolder)
        {
            var doc = new XmlDocument();
            string[] files = Directory.GetFiles(baseFolder, "*.evt.xml");
            string file = files.FirstOrDefault();
            if (string.IsNullOrEmpty(file))
            {
                string expName = _experiment.GetExperimentInfo().Name;
                string filename = string.Format("{0}-Cr1.evt.xml", expName);
                file = Path.Combine(baseFolder, filename);
                FileStream fs = File.Create(file);
                fs.Close();
                CreateCommonNodesForEvtXml(doc);
                doc.Save(file);
            }

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
            doc.Save(file);

        }

        internal abstract List<Blob> GetTileBlobs(int wellId, int tileId, BlobType type);
        internal abstract void SaveTileBlobs(string baseFolder);      
        internal abstract IList<BioEvent> CreateEvents(int wellId, int tileId,
            IDictionary<string, ImageData> imageDict, BlobDefine define);
       
        #endregion

        #region Protected
        protected int GetBlobKey(int wellId, int tileId)
        {
            int scanIdBits = (_scanId - 1) & 0x01; // 1 bit for scanId
            int wellIdBits = (wellId - 1) & 0x7FFF; //15 bits for wellId
            int tileIdBits = (tileId - 1) & 0xFFFF; //16 bits for tileId
            return (wellIdBits << 15) | (tileIdBits) | (scanIdBits << 31);
        }

        protected void ResetEventCountDict()
        {
            _eventsCountDict.Clear();
            foreach (KeyValuePair<int, List<BioEvent>> entry in _eventsDict)
            {
                int key = entry.Key;
                int count = entry.Value.Count;
                _eventsCountDict[key] = count;
            }
        }

        protected BioEvent CreateEvent(Blob blobOrg, Blob blobData,
       BlobDefine define, IDictionary<string, ImageData> imageDict, int wellId, int tileId)
        {
            ScanInfo info = _experiment.GetScanInfo(ScanId);
            ScanRegion regions = info.ScanRegionList[wellId - 1];
            Scanfield field = regions.ScanFieldList[tileId - 1];
            double pixelWidth = info.XPixcelSize;
            double pixelHeight = info.YPixcelSize;
            double area = blobOrg.Area * pixelWidth * pixelHeight;
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
                string channelName = channel.ChannelName;
                ImageData image = imageDict[channelName];
                Marshal.Copy(image.DataBuffer, _imageBuffer, 0, _imageBuffer.Length);
                const int rejectPercent = 200;
                // dynamic background
                int bkgnd = 0;
                if (bkBlob != null)
                {
                    bool correctBk = define.DynamicBkCorrections[i];
                    if (correctBk)
                    {
                        bkgnd = bkBlob.ComputeDynamicBackground(_imageBuffer, ImageWidth,
                            define.BackgroundLowBoundPercent,
                            define.BackgroundHighBoundPercent, rejectPercent);


                        Feature fb = GetFeature(FeatureType.Background);
                        if (fb != null)
                            ev[fb.Index + i] = bkgnd;
                    }
                }

                // integral, max-pixel
                int maxPixel;

                float integral = ComputeIntegral(_imageBuffer, blobData, bkgnd, out maxPixel);


                Feature fi = GetFeature(FeatureType.Integral);
                ev[fi.Index + i] = integral;

                Feature fintensity = GetFeature(FeatureType.Intensity);
                if (fintensity != null)
                    ev[fintensity.Index + i] = integral / blobData.Area;

                //YAK 4-8-2011: stdv of the intensities

                float stdv = ComputeStdv(_imageBuffer, blobData, bkgnd, integral / blobData.Area);

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
                        (int)ComputeIntegral(_imageBuffer, periBlob, bkgnd, out maxPixel);

                    Feature fp = GetFeature(FeatureType.PeripheralIntegral);
                    if (fp != null)
                        ev[fp.Index + i] = periIntegral;

                    fp = GetFeature(FeatureType.PeripheralIntensity); // jcl-7492
                    if (fp != null)
                        ev[fp.Index + i] = periIntegral / (float)periBlob.Area;

                    fp = GetFeature(FeatureType.PeripheralMax);
                    if (fp != null)
                        ev[fp.Index + i] = maxPixel;
                }
            }


            // common features

            Point center = blobData.Centroid();
            double px = field.SFRect.X + (ImageWidth - center.X) * pixelWidth;
            //double px = field.SFRect.X + (center.X) * pixelWidth;
            double py = field.SFRect.Y + center.Y * pixelHeight;
            ev[GetFeature(FeatureType.XPos).Index] = (int)px;
            ev[GetFeature(FeatureType.YPos).Index] = (int)py;

            Feature f = GetFeature(FeatureType.Area);
            if (f != null)
                ev[f.Index] = (float)area;

            f = GetFeature(FeatureType.Time);
            if (f != null)
                ev[f.Index] = 0;

            f = GetFeature(FeatureType.Scan);
            if (f != null)
                ev[f.Index] = (float)blobData.Centroid().Y;

            int perimeter = blobOrg.Perimeter(pixelWidth, pixelHeight); // perimeter on threshold blob
            f = GetFeature(FeatureType.Perimeter);
            if (f != null)
                ev[f.Index] = perimeter;

            f = GetFeature(FeatureType.Circularity);
            if (f != null)
                ev[f.Index] = perimeter * (float)perimeter / (float)area;

            f = GetFeature(FeatureType.HalfRadius);
            if (f != null)
                ev[f.Index] = (float)area / perimeter; //the old diameter value (which is actually diameter/4)

            f = GetFeature(FeatureType.Diameter);
            if (f != null)
                ev[f.Index] = (float)(area / perimeter) * 4;
            //(Pi * R^2) / (2 * Pi * R)  = A/P = Diameter/4..so we multiply by 4 to get the correct diameter.

            float xMean;
            float yMean;
            float oxx;
            float oyy;
            float oxy;
            blobOrg.ComputeXyMean(out xMean, out yMean);
            blobOrg.ComputeCovarianceElements(xMean, yMean, out oxx, out oyy, out oxy);
            //YAK 5_11_2011: convert the mean vector and Cov matrix into microns
            oxx *= (float)(pixelWidth * pixelWidth);
            oyy *= (float)(pixelHeight * pixelHeight);
            oxy *= (float)(pixelWidth * pixelHeight);
            //End conversion///////////////////////////////

            float lambda0 = ((oxx + oyy) / 2) - ((float)Math.Sqrt(4 * oxy * oxy + ((oxx - oyy) * (oxx - oyy))) / 2);
            float lambda1 = ((oxx + oyy) / 2) + ((float)Math.Sqrt(4 * oxy * oxy + ((oxx - oyy) * (oxx - oyy))) / 2);

            float majorAxis = 4 * (float)Math.Sqrt(lambda1);
            float minorAxis = 4 * (float)Math.Sqrt(lambda0);

            f = GetFeature(FeatureType.MajorAxis);
            if (f != null)
                ev[f.Index] = majorAxis;

            f = GetFeature(FeatureType.MinorAxis);
            if (f != null)
                ev[f.Index] = minorAxis;

            //YAK 6-17-2011: When the object is very small (or very thin) a rounding error may occure that may result in a zero minor axis. 
            //Fix that using a lower bound on the minor axis which should be at least one pixel but converted into microns
            var mn = (float)Math.Min(pixelWidth, pixelHeight);
            if (Math.Abs(minorAxis) < Tolerance)
                minorAxis = mn;

            //YAK 4-11-2011: The following two features (elongation and eccentricity) are computed using the major and minor axes
            float elongation = majorAxis / minorAxis;
            var eccentricity = (float)Math.Sqrt((lambda1 - lambda0) / lambda1);
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
            {
                // editing existing blob, set event id to the blob id so that the new event replaces the original event when added to the list
                ev.Id = blobData.Id;
                blobData.EventId = ev.Id;
            }

            ev.DataBlob = blobData;
            ev.ContourBlob = blobOrg;
            blobOrg.Id = blobData.Id = ev.Id;
            // set blob id equal to the event id (1-based event id is set when an event is added to the list)
            int key = GetBlobKey(wellId, tileId);
            if (bkBlob != null)
            {
                bkBlob.Id = ev.Id;
                ev.BackgroundBlob = bkBlob;
                SetTileBlobs(key, bkBlob, BlobType.Background);

            }
            if (periBlob != null)
            {
                periBlob.Id = ev.Id;
                ev.PeripheralBlob = periBlob;
                SetTileBlobs(key, periBlob, BlobType.Peripheral);
            }
            return ev;
        }

        protected virtual void SetTileBlobs(int key, Blob blob, BlobType type)
        {

        }

     
        #endregion

        #region Private

        private void Init()
        {
            ExperimentInfo info = _experiment.GetExperimentInfo();
            ScanInfo scanInfo = _experiment.GetScanInfo(ScanId);
            SoftwareVersion = new Version(info.SoftwareVersion);
            BasePath = Path.Combine(info.AnalysisPath, ComponentDataManager.DataStoredFolder);
            ImageWidth = scanInfo.TileWidth;
            ImageHeight = scanInfo.TiledHeight;
            _imageBuffer = new short[ImageWidth*ImageHeight];
            ParseEvtXml();
        }

        private void ResortFeatures()
        {
            ResortFeatures(_features);
        }

        private IEnumerable<Channel> ResortChannels()
        {
            ScanInfo info = _experiment.GetScanInfo(ScanId);
            IList<Channel> physicalChannels = info.ChannelList;
            IList<VirtualChannel> virtualChannels = info.VirtualChannelList;
            var channels = new List<Channel>(physicalChannels.Count + virtualChannels.Count);
            channels.AddRange(physicalChannels.OrderBy(chn => chn.ChannelId));
            channels.AddRange(virtualChannels);
            for (int i = physicalChannels.Count; i < channels.Count; i++)
                channels[i].ChannelId = i;
            return channels;
        }

        private void ResortFeatures(IList<Feature> features)
        {
            int idIndex = CheckIdFeature(features);
            features.RemoveAt(idIndex);
            Feature first = features.FirstOrDefault();
            if (first == null) return;
            first.Index = 1;
            int n = features.Count;
            for (int i = 1; i < n; i++)
            {
                Feature prev = features[i - 1];
                Feature cur = features[i];
                cur.Index = prev.IsPerChannel ? prev.Index + ChannelCount : prev.Index + 1;
            }
            Feature last = features.LastOrDefault();
            FeatureCount = last != null ? (last.IsPerChannel ? last.Index + ChannelCount : last.Index + 1) : 0;
            var idFeature = new Feature(FeatureType.Id);
            features.Insert(idIndex, idFeature);
        }

        private void ParseEvtXml()
        {
            XmlElement root = GetRootOfEvtXml();
            if(root==null) return;
            string query = string.Format("descendant::component[@name='{0}']", _componentName);
            var compNode = root.SelectSingleNode(query) as XmlElement;
            if (compNode == null) return;
            var channelsNode = compNode.SelectSingleNode("descendant::channels") as XmlElement;
            _channels.AddRange(ReadChannelInfoFromXml(channelsNode));
            var featuresNode = compNode.SelectSingleNode("descendant::features") as XmlElement;
            _features.AddRange(ReadFeatureInfoFromXml(featuresNode));
            var wellsNode = compNode.SelectSingleNode("descendant::wells") as XmlElement;
            ReadEventsCountFromXml(wellsNode);
            ResortFeatures();
        }

        
        private XmlElement GetRootOfEvtXml()
        {
            if (Directory.Exists(BasePath) == false) return null;
            string[] files = Directory.GetFiles(BasePath, "*.evt.xml");
            string file = files.FirstOrDefault();
            if (string.IsNullOrEmpty(file))
                return null;
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
            string filepath = Path.Combine(BasePath, _componentName, filename);
            if (File.Exists(filepath) == false)
                return EmptyEvents.ToList();
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

        private void WriteEventsBinary(string folder)
        {
            ScanInfo info = _experiment.GetScanInfo(ScanId);
            IList<ScanRegion> regions = info.ScanRegionList;
            foreach (ScanRegion region in regions)
            {
                int wellId = region.WellId;
                if (_eventsDict.ContainsKey(wellId))
                {
                    WriteEventsBinary(folder, wellId);
                }                
            }
        }
     
        private void WriteEventsBinary(string folder, int wellId)
        {
            List<BioEvent> evs = _eventsDict[wellId];
            string filename = Path.Combine(folder, string.Format("{0}_{1}.evt", wellId, wellId));
            if (File.Exists(filename) == false)
            {
                FileStream fs = File.Create(filename);
                fs.Close();
            }
               
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
            ScanInfo info = _experiment.GetScanInfo(ScanId);
            var compNode = doc.CreateElement("component");
            compNode.SetAttribute("name", _componentName);
            compNode.SetAttribute("pixel-width", info.XPixcelSize.ToString(CultureInfo.InvariantCulture));
            compNode.SetAttribute("pixel-height", info.YPixcelSize.ToString(CultureInfo.InvariantCulture));
            compNode.SetAttribute("blob-type", "Field");
            compNode.SetAttribute("scan-no",ScanId.ToString(CultureInfo.InvariantCulture));
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

        private void CreateCommonNodesForEvtXml(XmlDocument doc)
        {           
            var rootNode = doc.CreateElement("data-list");
            rootNode.SetAttribute("version", SoftwareVersion.ToString());
            doc.AppendChild(rootNode);
            var compsNode = doc.CreateElement("components");
            rootNode.AppendChild(compsNode);
        }

        private Feature GetFeature(FeatureType type)
        {
            return Features.FirstOrDefault(f => f.FeatureType == type);
        }

        private int CheckIdFeature(IList<Feature> features)
        {
            Feature idFeature = features.FirstOrDefault(f => f.FeatureType == FeatureType.Id);
            if (idFeature == null) throw new ArgumentException("Must contain id in feature list");
            int index = features.IndexOf(idFeature);
            return index;
        }
  
        private Blob CreateBackgroundBlob(Blob blobData, BlobDefine define)
        {
            int width = define.BackgroundWidth;
            int dist = define.BackgroundDistance;
            int extent = width + dist;
            Blob bkBlob = null;
            if (blobData.TouchesEdge(extent, ImageWidth, ImageHeight) == false)
            {
               bkBlob = blobData.CreateRing(dist, width, false, 0, 0);
            }
            return bkBlob;
        }

        private Blob CreatePeripheralBlob(Blob blobOrg, BlobDefine define)
        {
            int width = define.PeripheralWidth;
            int dist = define.PeripheralDistance;
            int extent = width + dist;
            Blob periBlob = null;
            if (blobOrg.TouchesEdge(extent, ImageWidth, ImageHeight) == false)
            {
                periBlob = blobOrg.CreateRing(dist, width, false, 0, 0);
            }            
            return periBlob;
        }


        private float ComputeIntegral(short[] data, Blob blob, int bkgnd, out int maxPixel)
        {
            maxPixel = 0;

            if (blob.Area <= 0) return 0;

            int height = blob.Bound.Height;
            var yValues = new int[height];
            float integral = 0;

            foreach (VLine line in blob.Lines)
            {
                int offset = line.Y1 - blob.Bound.Y;
                if (line.X < 0 || line.X >= ImageWidth || line.Y1 + line.Length >= ImageHeight)
                    continue; // can happen when region was created interactively

                for (int i = 0; i < line.Length; i++)
                {                  
                    int nTmp;
                    yValues[offset + i] += (nTmp = data[(line.Y1 + i) * ImageWidth + line.X] - bkgnd);

                    if (nTmp > maxPixel)
                    {
                        maxPixel = nTmp;
                    }
                }
            }
            for (int i = 0; i < height; i++)
                integral += yValues[i];       
            return integral;
        }

        //YAK 4-8-2011: compute the standard deviation of the intensities of the blob
        private float ComputeStdv(short[] data, Blob blob, int bkgnd, float meanInt)
        {
            if (blob.Area <= 0) return 0;

            int height = blob.Bound.Height;
            var yValues = new float[height];
            float stdv = 0;
            int numPixels = 0;
            foreach (VLine line in blob.Lines)
            {
                int offset = line.Y1 - blob.Bound.Y;
                if (line.X < 0 || line.X >= ImageWidth || line.Y1 + line.Length >= ImageHeight)
                    continue; // can happen when region was created interactively

                for (int i = 0; i < line.Length; i++)
                {
                    numPixels++;                
                    double tmpN = (data[(line.Y1 + i) * ImageWidth + line.X] - bkgnd - meanInt) *
                                  (data[(line.Y1 + i) * ImageWidth + line.X] - bkgnd - meanInt);
                    yValues[offset + i] += (float)tmpN;                                     
                }
            }
            for (int i = 0; i < height; i++)
                stdv += yValues[i];
            stdv /= numPixels;
            stdv = (float)Math.Sqrt(stdv);
            return stdv;
        }

      

        #endregion

        #endregion


    }
}
