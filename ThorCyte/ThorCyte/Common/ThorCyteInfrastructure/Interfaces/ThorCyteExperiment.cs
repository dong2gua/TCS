using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.Infrastructure.Interfaces
{
    public class ThorCyteExperiment : IExperiment
    {

        #region Enum

        private enum CarrierType
        {
            None = 0,
            Well,
            Slide
        }

        private enum FileType
        {
            Mosaic,
            Raw
        }

        #endregion

        #region Fields
        private const int ScanCount = 1;
        private const string InstrumentType = "ThorCyte";
        private bool _hasLoadExprimentInfo;
        private bool _hasLoadFirstScanInfo;
        private ScanInfo _firstScanInfo;
        private ExperimentInfo _experimentInfo;
        private string _activeRunNum = string.Empty;
        private string _carrierId = string.Empty;
        private CarrierType _carrierType = CarrierType.None;
        private double _fieldWidth;
        private double _fieldHeight;
        private int _rows;
        private int _cols;
        #endregion

        #region Properties

        public string BasePath { get; private set; }
        public int XInterval { get; private set; }
        public int YInterval { get; private set; }
        internal ThorCyteData.ImageType ImageType { get; private set; }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// clear experiment info, call it before next load
        /// </summary>
        private void Clear()
        {
            BasePath = string.Empty;
            ClearExperimentInfo();
            ClearScanInfo();      
        }

        private void ClearScanInfo()
        {
            _hasLoadFirstScanInfo = false;
            _firstScanInfo = new ScanInfo
            {
                Mode = CaptureMode.Mode2D,
                ResolutionUnit = ResUnit.Micron,
                ScanId = 1
            };
            _carrierId = string.Empty;
            XInterval = 0;
            YInterval = 0;
        }

        private void ClearExperimentInfo()
        {
            _hasLoadExprimentInfo = false;
            _experimentInfo = new ExperimentInfo
            {
                InstrumentType = InstrumentType,
                UserName = string.Empty,
                IntensityBits = 14 //14bits
            };
        }
        private static string FindspecifiedFile(string path, string extension)
        {
            var files = Directory.GetFiles(path, extension);
            return files.FirstOrDefault();
        }

        private XmlDocument GetRunXml(string filepath)
        {
            var file = Path.Combine(filepath, "run.xml");
            var doc = new XmlDocument();
            doc.Load(file);
            return doc;
        }

        private XmlDocument GetRunXml()
        {
            return GetRunXml(BasePath);
        }

        private XmlElement GetActiveRunElement(XmlDocument doc)
        {
            var query = string.Format("descendant::Run");
            XmlElement root = doc.DocumentElement;
            if (root == null) return null;
            XmlNodeList runNodes = root.SelectNodes(query);
            if (runNodes == null) return null;
            foreach (XmlElement runNode in runNodes)
            {
                var activeRun = runNode.SelectSingleNode(string.Format("descendant::ActiveRun")) as XmlElement;
                if (activeRun != null)
                {
                    if (activeRun.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
                        return runNode;
                }
            }
            return null;

        }

        private void ParseActiveRunElement(XmlElement runNode)
        {
            XmlNodeList childern = runNode.ChildNodes;
            foreach (XmlElement child in childern)
            {
                if (child.Name.Equals("RunNum", StringComparison.OrdinalIgnoreCase))
                {
                    _activeRunNum = child.InnerText;
                }
                else if (child.Name.Equals("RunName", StringComparison.OrdinalIgnoreCase))
                {
                    _experimentInfo.Name = child.InnerText;
                }
                else if (child.Name.Equals("CreationTime", StringComparison.OrdinalIgnoreCase))
                {
                    _experimentInfo.Date = child.InnerText;
                }
                else if (child.Name.Equals("Annotation", StringComparison.OrdinalIgnoreCase))
                {
                    _experimentInfo.Notes = child.InnerText;
                }
                else if (child.Name.Equals("InstrumentID", StringComparison.OrdinalIgnoreCase))
                {
                    _experimentInfo.ComputerName = child.InnerText;
                }

            }

        }

        private string GetActiveRunNum()
        {
            XmlDocument doc = GetRunXml();
            XmlElement run = GetActiveRunElement(doc);
            XmlNodeList childern = run.ChildNodes;
            foreach (XmlElement child in childern)
            {
                if (child.Name.Equals("RunNum", StringComparison.OrdinalIgnoreCase))
                {
                    return child.InnerText;
                }
            }
            return string.Empty;
        }

        private XmlElement GetActiveInstrumentStateElement(XmlDocument doc)
        {
            var query = string.Format("descendant::InstrumentState");
            XmlElement root = doc.DocumentElement;
            if (root == null) return null;
            var nodes = root.SelectNodes(query);
            foreach (XmlElement node in nodes)
            {
                var e = node.SelectSingleNode(string.Format("descendant::RunNum")) as XmlElement;
                if (e != null)
                {
                    if (e.InnerText.Equals(_activeRunNum, StringComparison.OrdinalIgnoreCase))
                        return node;
                }
            }
            return null;
        }

        private void ParseActiveInstrumentStateElement(XmlElement element)
        {
            XmlElement node = element.ChildNodes.Cast<XmlElement>()
                .FirstOrDefault(n => n.Name.Equals("SoftwareVersion"));
            _experimentInfo.SoftwareVersion = node == null ? string.Empty : node.InnerText;
        }

        private XmlDocument GetWorkspaceXml(string filepath)
        {
            var file = FindspecifiedFile(filepath, "*.ws.xml");
            var doc = new XmlDocument();
            doc.Load(file);
            return doc;
        }

        private XmlDocument GetWorkspaceXml()
        {
            return GetWorkspaceXml(BasePath);
        }

        private void ParseWorkspaceXml(XmlDocument doc, ScanInfo scanInfo)
        {
            // carrier xml element
            var query = string.Format("descendant::carrier");
            var node = doc.DocumentElement.SelectSingleNode(query) as XmlElement;
            ParseCarrierElement(node);

            // scan-region xml element
            query = string.Format("descendant::scan-area");
            node = doc.DocumentElement.SelectSingleNode(query) as XmlElement;
            ParseScanRegionElement(node, scanInfo);
         

            query = string.Format("descendant::modules");
            node = doc.DocumentElement.SelectSingleNode(query) as XmlElement;
            if (node != null)
            {
                XmlElement detectors = null;
                XmlElement fieldScan = null;
                foreach ( XmlElement element in node.ChildNodes)
                {
                    string name = element.ParseAttributeToString("name");
                    if (name.Equals("Detectors", StringComparison.OrdinalIgnoreCase))
                    {
                        detectors = element;
                    }
                    else if (name.Equals("FieldScan", StringComparison.OrdinalIgnoreCase))
                    {
                        fieldScan = element;
                    }
                    if (fieldScan != null && detectors != null) break;
                }
                IEnumerable<Channel> channels = GetChannels(detectors);
                int id = 0;
                foreach (var channel in channels)
                {
                    channel.ChannelId = id;
                    scanInfo.ChannelList.Add(channel);
                    id++;
                }
                scanInfo.ObjectiveType = GetObjectiveType(fieldScan);
                XInterval = fieldScan.ParseAttributeToInt32("xinterval");
                YInterval = fieldScan.ParseAttributeToInt32("yinterval");              
                scanInfo.ScanPathMode = ScanPathType.LeftToRight;
               

            }
        }

        private void ParseCarrierElement(XmlElement element)
        {
            if (element != null)
            {
                if (element.HasAttribute("ref"))
                {
                    _carrierId = element.Attributes["ref"].InnerText;
                }

                XmlNode child = element.FirstChild;
                if (child != null)
                {
                    _carrierType = child.Name.Equals("slide", StringComparison.OrdinalIgnoreCase)
                        ? CarrierType.Slide
                        : CarrierType.Well;
                }
            }
        }

        private void ParseScanRegionElement(XmlElement element, ScanInfo scanInfo)
        {
            if (element != null)
            {
                _fieldHeight = element.ParseAttributeToDouble("field-height");
                _fieldWidth = element.ParseAttributeToDouble("field-width");
                _rows = element.ParseAttributeToInt32("rows");
                _cols = element.ParseAttributeToInt32("cols");
                IEnumerable<ScanRegion> regions = GetScanRegions(element);
                foreach (var region in regions)
                {
                    Well well = new Well(region.WellId, region.Bound);
                    scanInfo.ScanWellList.Add(well);
                    scanInfo.ScanRegionList.Add(region);
                }
            }
        }

        private IEnumerable<ScanRegion> GetScanRegions(XmlElement element)
        {
            var regions = new List<ScanRegion>();
            switch (_carrierType)
            {
                case CarrierType.Slide:
                    regions.AddRange(GetScanRegionsInSlide(element));
                    break;
                case CarrierType.Well:
                    regions.AddRange(GetScanRegionsInWell(element));
                    break;
            }
          
            return regions;
        }

        private void BuildTiles(IEnumerable<ScanRegion> scanRegions)
        {
           
            foreach (var scanRegion in scanRegions)
            {
                scanRegion.BulidTiles(_fieldWidth, _fieldHeight, XInterval, YInterval, ScanPathType.LeftToRight);
            }
        }

        /// <summary>
        /// Get all scan region coordinate from *.ws.xml file
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private IEnumerable<ScanRegion> GetScanRegionsInSlide(XmlElement element)
        {
            var regions = new List<ScanRegion>();
            var query = string.Format("descendant::well");
            var childs = element.SelectNodes(query);
            if (childs == null) return regions;
            regions.AddRange(childs.Cast<XmlElement>().Select(GetScanRegionInSlide).Where(region => region != null));
            return regions;
        }

        private ScanRegion GetScanRegionInSlide(XmlElement element)
        {
            if (element == null) return null;
            else
            {
                int wellId = 0;
                int regionId = 0;
                var points = new Point[0];
                var shape = RegionShape.None;
                if (element.HasAttribute("no"))
                {
                    string text = element.Attributes["no"].InnerText;
                    int.TryParse(text, out wellId);
                    var children = element.ChildNodes;
                    foreach (XmlElement child in children)
                    {

                        regionId = child.ParseAttributeToInt32("id") - 1;//well id 1-base, regionId 0-base
                        if (child.HasAttribute("shape"))
                        {
                            text = child.Attributes["shape"].InnerText;
                            if (!Enum.TryParse(text, out shape))
                            {
                                shape = RegionShape.None;
                            }
                        }

                        points = ReadScanRegionBound(child, shape).ToArray();
                    }
                }
                var region = new ScanRegion(wellId, regionId, shape, points) {Bound = GetBoundRect(points, shape)};
                return region;
            }
        }


        private IEnumerable<Point> ReadScanRegionBound(XmlElement element, RegionShape shape)
        {
            
            if (element == null) return new Point[0];
            switch (shape)
            {
               case RegionShape.Rectangle:
               case RegionShape.Ellipse:
                {
                    double x = ParseAttributeTextToDouble(element, "x");
                    double y = ParseAttributeTextToDouble(element, "y");
                    double h = 0;
                    double w = 0;
                    if (_carrierType == CarrierType.Slide)
                    {
                        w = ParseAttributeTextToDouble(element, "w");
                        h = ParseAttributeTextToDouble(element, "h");
                    }
                    else if (_carrierType == CarrierType.Well)
                    {
                        if (_rows == 0 || _cols == 0)
                        {
                            w = ParseAttributeTextToDouble(element, "width");
                            h = ParseAttributeTextToDouble(element, "height");
                        }
                        else
                        {
                            w = _fieldWidth*_cols;
                            h = _fieldHeight*_rows;
                        }
                    }
                 
                    var points = new List<Point>(4)
                    {
                        new Point(x, y),
                        new Point(x + w, y),
                        new Point(x + w, y + h),
                        new Point(x, y + h)
                    };
                    return points;
                }
                case RegionShape.Polygon:
                {
                    var query = string.Format("descendant::points");
                    var pointsNode = element.SelectSingleNode(query) as XmlElement;
                    int n = 0;
                    if (pointsNode != null)
                    {
                        if (pointsNode.HasAttribute("count"))
                        {
                            string text = pointsNode.Attributes["count"].InnerText;
                            int.TryParse(text, out n);
                        }
                    }
                    var points = new List<Point>(n);
                    var pointNodes = element.SelectNodes("descendant::point");
                    if (pointNodes == null) return points;
                    else
                    {
                        foreach (XmlElement pointNode in pointNodes)
                        {
                            double x = pointNode.ParseAttributeToDouble("x");
                            double y = pointNode.ParseAttributeToDouble("y");
                            points.Add(new Point(x, y));
                        }

                        return points;
                    }
                        
                }
                default:
                    return new Point[0];
            }
            
        }
        private IEnumerable<ScanRegion> GetScanRegionsInWell(XmlElement element)
        {
            RegionShape shape;
            string text = element.ParseAttributeToString("shape");
            if (!Enum.TryParse(text, true, out shape))
                shape = RegionShape.None;
                     
            string mosFile = GetFile(FileType.Mosaic);
            if (string.IsNullOrEmpty(mosFile)) return new ScanRegion[0];
            else
            {
                var regions = new List<ScanRegion>();
                var file = Path.Combine(BasePath, mosFile);
                var doc = new XmlDocument();
                doc.Load(file);
                var query = string.Format("descendant::well");
                var nodes = doc.DocumentElement.SelectNodes(query);
                foreach (XmlElement node in nodes)
                {
                    ScanRegion scanRegion = GetScanRegionInWell(node, shape);
                    if (scanRegion != null)
                        regions.Add(scanRegion);
                }
                return regions;


            }
        }

        private ScanRegion GetScanRegionInWell(XmlElement element, RegionShape shape)
        {
            if (element == null) return null;
            else
            {
                int regionId = 0;
                var points = new List<Point>();
                int wellId = element.ParseAttributeToInt32("no");
                var regionNode = element.SelectSingleNode(string.Format("descendant::region")) as XmlElement;
                if (regionNode != null)
                {
                    regionId = regionNode.ParseAttributeToInt32("no") - 1;//well id 1-base, regionId 0-base
                }
                var boundNode = element.SelectSingleNode(string.Format("descendant::bounding-rect")) as XmlElement;
                if (boundNode != null)
                {
                    points.AddRange(ReadScanRegionBound(boundNode, shape));
                }

                var region = new ScanRegion(wellId, regionId, shape, points.ToArray())
                {
                    Bound = GetBoundRect(points, shape)
                };
                return region;
            }
           
        }

        private Rect GetBoundRect(IList<Point> points, RegionShape shape)
        {
            switch (shape)
            {
                case RegionShape.Ellipse:
                case RegionShape.Rectangle:
                    return new Rect(points[0], points[2]);
                case RegionShape.Polygon:
                    return GetBoundRectOfPolygon(points);
                default:
                    return new Rect();

            }
        }

        private static Rect GetBoundRectOfPolygon(ICollection<Point> points)
        {
            double minx = points.Min(p => p.X);
            double miny = points.Min(p => p.Y);
            double maxx = points.Max(p => p.X);
            double maxy = points.Max(p => p.Y);
            return new Rect(new Point(minx, miny), new Point(maxx, maxy));
        }

        private string GetFile(FileType fileType)
        {
            string file = fileType == FileType.Mosaic ? "MosaicFile" : "RAWFile";
            XmlDocument doc = GetRunXml();
            var query = string.Format("descendant::RunScan");
            var nodes = doc.DocumentElement.SelectNodes(query);
            foreach (XmlElement node in nodes)
            {
                var e = node.SelectSingleNode(string.Format("descendant::RunNum")) as XmlElement;
                if (e != null)
                {
                    if (e.InnerText.Equals(_activeRunNum, StringComparison.OrdinalIgnoreCase))
                    {
                        var fileNode =
                            node.SelectSingleNode(string.Format("descendant::{0}", file)) as XmlElement;
                        return fileNode == null ? string.Empty : Path.Combine(BasePath, fileNode.InnerText);
                    }
                }
            }
            
            return string.Empty;
        }
              
        private IEnumerable<Channel> GetChannels(XmlElement element)
        {
            var channels = new List<Channel>();
            if (element == null) return channels;
            else
            {
                var query = string.Format("descendant::channels");
                var nodes = element.SelectNodes(query);
                if (nodes == null) return channels;
                foreach (XmlElement node in nodes)
                {
                    if (!node.HasAttribute("type"))
                    {
                        var pchns = ParsePhsicalChannelsElement(node);
                        channels.AddRange(pchns);
                    }                   
                }
            }
            return channels;
        }

        private IEnumerable<Channel> ParsePhsicalChannelsElement(XmlElement element)
        {
            var channels = new List<Channel>();
            if (element == null) return channels;
            else
            {
                var nodes = element.ChildNodes;
                
                foreach (XmlElement node in nodes)
                {
                    Channel channel = GetChannel(node);
                    if (channel != null)
                    {                     
                        channels.Add(channel);               
                    }
                }
                return channels;
            }  

        }

        private Channel GetChannel(XmlElement element)
        {
            if (element == null) return null;
            else
            {
                string enable = element.ParseAttributeToString("enabled");
                if (!enable.Equals("true", StringComparison.OrdinalIgnoreCase)) return null;
                else
                {
                    string name = element.ParseAttributeToString("label");
                    return new Channel {ChannelName = name};

                }
            }
        }

        private string GetObjectiveType(XmlElement element)
        {
            return element.ParseAttributeToString("objective");
        }

        private double ParseAttributeTextToDouble(XmlElement element, string attribute)
        {
            double value;
            if (element.HasAttribute(attribute))
            {
                string text = element.Attributes[attribute].InnerText;
                double.TryParse(text, out value);
            }
            else
            {
                value = 0;
            }
            return value;
        }

        private void ParseRawXml()
        {
            var doc = new XmlDocument();
            string rawFile = GetFile(FileType.Raw);
            doc.Load(rawFile);
            XmlElement root = doc.DocumentElement;
            string imageTypeString = root.ParseAttributeToString("format");
            ThorCyteData.ImageType type;
            if (Enum.TryParse(imageTypeString, true, out type) == false) type = ThorCyteData.ImageType.None;
            ImageType = type;            
            var node = root.SelectSingleNode(string.Format("descendant::pixel-size")) as XmlElement;
            _firstScanInfo.XPixcelSize = node.ParseAttributeToDouble("width");
            _firstScanInfo.YPixcelSize = node.ParseAttributeToDouble("height");
            node = root.SelectSingleNode(string.Format("descendant::field-size-in-pixel")) as XmlElement;
            _firstScanInfo.TileWidth = node.ParseAttributeToInt32("width");
            _firstScanInfo.TiledHeight = node.ParseAttributeToInt32("height");
        }
        private ScanInfo GetFirstScanInfo()
        {
            if (_hasLoadFirstScanInfo)
                return _firstScanInfo;
            else
            {
                bool hasException = false;
                try
                {
                    if (string.IsNullOrEmpty(_activeRunNum)) _activeRunNum = GetActiveRunNum();
                    var doc = GetWorkspaceXml();
                    ParseWorkspaceXml(doc, _firstScanInfo);
                    ParseRawXml();
                    BuildTiles(_firstScanInfo.ScanRegionList);
                    _hasLoadFirstScanInfo = true;
                }
                catch (ArgumentNullException)
                {
                    hasException = true;
                }
                catch (ArgumentException)
                {
                    hasException = true;
                }
                catch (NullReferenceException)
                {
                    hasException = true;
                }
                catch (FileNotFoundException)
                {
                    hasException = true;
                }
                catch (DirectoryNotFoundException)
                {
                    hasException = true;
                }
                finally
                {
                    if (hasException)
                        ClearScanInfo();                
                }
                return _firstScanInfo;

            }
        }

        #endregion

        #region Methods in interface
        /// <summary>
        /// only for OCT
        /// </summary>
        /// <returns></returns>
        public BitmapSource GetCameraView()
        {
            return null;
        }

        public int GetCurrentScanId()
        {
            return 1;
        }

        public ScanInfo GetScanInfo(int scanId)
        {
            switch (scanId)
            {
                case 1:
                    return GetFirstScanInfo();
                case 2:
                    throw new NotSupportedException("not support mosaic scan yet.");
                default:
                    throw new ArgumentOutOfRangeException("scanId", "scanId could only be 1 or 2");
            }
        }

        public string GetCarrierType()
        {
            if (string.IsNullOrEmpty(_carrierId))
            {
                XmlDocument doc = GetWorkspaceXml();
                var query = string.Format("descendant::carrier");
                XmlElement root = doc.DocumentElement;
                if (root == null) return string.Empty;
                var node = root.SelectSingleNode(query) as XmlElement;
                if (node != null)
                {                    
                    _carrierId = node.ParseAttributeToString("ref");
                }
            }
            return _carrierId;
        }

        /// <summary>
        /// Only search run whose active state is true in run.xml
        /// </summary>
        /// <returns></returns>
        public ExperimentInfo GetExperimentInfo()
        {
            if (_hasLoadExprimentInfo == false)
            {
                bool hasError = false;
                try
                {
                    var doc = GetRunXml();
                    var runNode = GetActiveRunElement(doc);
                    ParseActiveRunElement(runNode);
                    var instNode = GetActiveInstrumentStateElement(doc);
                    ParseActiveInstrumentStateElement(instNode);
                    _hasLoadExprimentInfo = true;
                }
                catch (ArgumentNullException)
                {
                    hasError = true;
                }
                catch (ArgumentException)
                {
                    hasError = true;
                }
                catch (NullReferenceException)
                {
                    hasError = true;
                }
                catch (FileNotFoundException)
                {
                    hasError = true;
                }
                catch (DirectoryNotFoundException)
                {
                    hasError = true;
                }

                finally
                {
                    if (hasError) ClearExperimentInfo();
                }
            }
            return _experimentInfo;
        }

        public int GetScanCount()
        {
            return ScanCount;
        }

        public void Load(string experimentPath)
        {
            Clear();
            BasePath = Path.GetDirectoryName(experimentPath);
            _firstScanInfo.DataPath = BasePath;
            _experimentInfo.AnalysisPath = BasePath + "\\Analysis";
            if (!Directory.Exists(BasePath))
                throw new DirectoryNotFoundException(string.Format("{0} not found!", BasePath));
        }
        #endregion

        #endregion
    }

    internal static class Utils
    {
      
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
    }
}
