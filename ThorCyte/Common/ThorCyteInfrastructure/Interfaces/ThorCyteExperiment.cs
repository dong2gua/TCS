using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        private const double Tolerance = 1e-6;
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

        private static XmlDocument GetRunXml(string filepath)
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

        private static XmlElement GetActiveRunElement(XmlDocument doc)
        {
            string query = "descendant::Run";
            XmlElement root = doc.DocumentElement;
            if (root == null) return null;
            XmlNodeList runNodes = root.SelectNodes(query);
            if (runNodes == null) return null;
            return (from XmlElement runNode in runNodes
                let activeRun = runNode.SelectSingleNode("descendant::ActiveRun") as XmlElement
                where activeRun != null
                where activeRun.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase)
                select runNode).FirstOrDefault();
        }

        private void ParseActiveRunElement(XmlNode runNode)
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
                    string original = child.InnerText;
                    _experimentInfo.Date = ParseTimeString(original);
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

        private string ParseTimeString(string original)
        {
            const string separator = "[a-zA-Z]";
            Match match = Regex.Match(original.TrimStart(), separator);
            string time = string.Empty;
            if (match.Success)
            {
                int index = match.Index;
                time = original.Substring(0, index);
            }
            return time;
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
            string query = "descendant::InstrumentState";
            XmlElement root = doc.DocumentElement;
            if (root == null) return null;
            XmlNodeList nodes = root.SelectNodes(query);
            return nodes != null
                ? (from XmlElement node in nodes
                    let e = node.SelectSingleNode("descendant::RunNum") as XmlElement
                    where e != null
                    where e.InnerText.Equals(_activeRunNum, StringComparison.OrdinalIgnoreCase)
                    select node).FirstOrDefault()
                : null;
        }

        private void ParseActiveInstrumentStateElement(XmlNode element)
        {
            XmlElement node = element.ChildNodes.Cast<XmlElement>()
                .FirstOrDefault(n => n.Name.Equals("SoftwareVersion"));
            _experimentInfo.SoftwareVersion = node == null ? string.Empty : node.InnerText;
        }

        private static XmlDocument GetWorkspaceXml(string filepath)
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

        private bool ParseWorkspaceXml(XmlDocument doc, ScanInfo scanInfo)
        {
            // carrier xml element
            string query = "descendant::carrier";
            XmlElement root = doc.DocumentElement;
            if (root != null)
            {
                var node = root.SelectSingleNode(query) as XmlElement;
                if (ParseCarrierElement(node) == false) return false;
                

                // scan-region xml element
                query = "descendant::scan-area";
                node =root.SelectSingleNode(query) as XmlElement;
                if (ParseScanRegionElement(node, scanInfo) == false) return false;
         

                query = "descendant::modules";
                node = root.SelectSingleNode(query) as XmlElement;
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
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ParseCarrierElement(XmlElement element)
        {
            if(element==null) return false;
            else
            {
                if (element.HasAttribute("ref"))
                {
                    _carrierId = element.Attributes["ref"].InnerText;
                }
                else 
                    return false;

                XmlNode child = element.FirstChild;
                if (child != null)
                {
                    _carrierType = child.Name.Equals("slide", StringComparison.OrdinalIgnoreCase)
                        ? CarrierType.Slide
                        : CarrierType.Well;
                }
                else 
                    return false;
                return true;
            }
            
            
        }

        private bool ParseScanRegionElement(XmlElement element, ScanInfo scanInfo)
        {
            if (element != null)
            {
                _fieldHeight = element.ParseAttributeToDouble("field-height");
                _fieldWidth = element.ParseAttributeToDouble("field-width");
                _rows = element.ParseAttributeToInt32("rows");
                _cols = element.ParseAttributeToInt32("cols");
                if (Math.Abs(_fieldHeight) < Tolerance || Math.Abs(_fieldWidth) < Tolerance)
                    return false;
                IEnumerable<ScanRegion> regions = GetScanRegions(element);
                foreach (var region in regions)
                {
                    Well well = new Well(region.WellId, region.Bound);
                    scanInfo.ScanWellList.Add(well);
                    scanInfo.ScanRegionList.Add(region);
                }
                return true;
            }
            else
                return false;
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
            string query = "descendant::well";
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
                    string query = "descendant::points";
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
                        points.AddRange(from XmlElement pointNode in pointNodes
                            let x = pointNode.ParseAttributeToDouble("x")
                            let y = pointNode.ParseAttributeToDouble("y")
                            select new Point(x, y));

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
                string query = "descendant::well";
                if (doc.DocumentElement != null)
                {
                    var nodes = doc.DocumentElement.SelectNodes(query);
                    if (nodes != null)
                        regions.AddRange(
                            nodes.Cast<XmlElement>()
                                .Select(node => GetScanRegionInWell(node, shape))
                                .Where(scanRegion => scanRegion != null));
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
                var regionNode = element.SelectSingleNode("descendant::region") as XmlElement;
                if (regionNode != null)
                {
                    regionId = regionNode.ParseAttributeToInt32("no") - 1;//well id 1-base, regionId 0-base
                }
                var boundNode = element.SelectSingleNode("descendant::bounding-rect") as XmlElement;
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
            string query = "descendant::RunScan";
            if (doc.DocumentElement != null)
            {
                var nodes = doc.DocumentElement.SelectNodes(query);
                if (nodes != null)
                    foreach (XmlElement node in nodes)
                    {
                        var e = node.SelectSingleNode("descendant::RunNum") as XmlElement;
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
            }

            return string.Empty;
        }
              
        private IEnumerable<Channel> GetChannels(XmlNode element)
        {
            var channels = new List<Channel>();
            if (element == null) return channels;
            else
            {
                string query = "descendant::channels";
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

        private IEnumerable<Channel> ParsePhsicalChannelsElement(XmlNode element)
        {
            var channels = new List<Channel>();
            if (element == null) return channels;
            else
            {
                var nodes = element.ChildNodes;
                channels.AddRange(nodes.Cast<XmlElement>().Select(GetChannel).Where(channel => channel != null));
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

        private bool ParseRawXml()
        {
            var doc = new XmlDocument();
            string rawFile = GetFile(FileType.Raw);
            doc.Load(rawFile);
            XmlElement root = doc.DocumentElement;
            string imageTypeString = root.ParseAttributeToString("format");
            ThorCyteData.ImageType type;
            if (Enum.TryParse(imageTypeString, true, out type) == false) type = ThorCyteData.ImageType.None;
            ImageType = type;
            if (root != null)
            {
                var node = root.SelectSingleNode("descendant::pixel-size") as XmlElement;
                double xpixelsize = node.ParseAttributeToDouble("width");
                if (Math.Abs(xpixelsize) < Tolerance) return false;
                _firstScanInfo.XPixcelSize = xpixelsize;
                double ypixelsize = node.ParseAttributeToDouble("height");
                if (Math.Abs(ypixelsize) < Tolerance) return false;
                _firstScanInfo.YPixcelSize = ypixelsize;
                node = root.SelectSingleNode("descendant::field-size-in-pixel") as XmlElement;
                if (node == null) return false;
                else
                {
                    int width = node.ParseAttributeToInt32("width");
                    if(width==0) return false;
                    _firstScanInfo.TileWidth = width;
                    int height = node.ParseAttributeToInt32("height");
                    if (height == 0) return false;
                    _firstScanInfo.TiledHeight = height;
                }
                return true;

            }
            else 
                return false;
        }
        private ScanInfo GetFirstScanInfo()
        {
            if (_hasLoadFirstScanInfo)
                return _firstScanInfo;
            else
            {
                if (string.IsNullOrEmpty(_activeRunNum)) _activeRunNum = GetActiveRunNum();
                XmlDocument doc = GetWorkspaceXml();
                if (ParseWorkspaceXml(doc, _firstScanInfo)==false) return null;
                if (ParseRawXml()==false) return null;
                BuildTiles(_firstScanInfo.ScanRegionList);
                _hasLoadFirstScanInfo = true;
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
            return scanId == 1 ? GetFirstScanInfo() : null;
        }

        public string GetCarrierType()
        {
            if (string.IsNullOrEmpty(_carrierId))
            {
                XmlDocument doc = GetWorkspaceXml();
                string query = "descendant::carrier";
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
                XmlDocument doc = GetRunXml();
                XmlElement runNode = GetActiveRunElement(doc);
                ParseActiveRunElement(runNode);
                XmlElement instNode = GetActiveInstrumentStateElement(doc);
                ParseActiveInstrumentStateElement(instNode);
                _hasLoadExprimentInfo = true;
            }
            return _experimentInfo;
        }

        public int GetScanCount()
        {
            return ScanCount;
        }

        public bool Load(string experimentPath)
        {
            Clear();
            BasePath = Path.GetDirectoryName(experimentPath);
            _firstScanInfo.DataPath = BasePath;
            _experimentInfo.ExperimentPath = BasePath;
            bool hasError = false;
            //_experimentInfo.AnalysisPath = BasePath + "\\Analysis";
            try
            {
                GetExperimentInfo();
                if (GetScanInfo(ScanCount) == null)
                    return false;
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
            return !hasError;

        }

        public void SetAnalysisPath(string path)
        {
            _experimentInfo.AnalysisPath = path;
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
