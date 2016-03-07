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
    internal class RealComponent : BioComponent
    {
        #region Fields
        private readonly Dictionary<int, List<Blob>> _contourBlobs = new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        private readonly Dictionary<int, List<Blob>> _dataBlobs = new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        private readonly Dictionary<int, List<Blob>> _backgroudBlobs = new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        private readonly string _contourXmlPath;
        private readonly Dictionary<int, List<Blob>> _peripheralBlobs =
            new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        #endregion

        #region Constructors
        internal RealComponent(IExperiment experiment, string name, int scanId) : base(experiment, name, scanId)
        {
            _contourXmlPath = Path.Combine(BasePath, Name, ComponentDataManager.ContourXmlFolder);
        }

        internal RealComponent(IExperiment experiment, string name) : this(experiment, name, 1)
        {
            
        }
        #endregion

        #region Methods

        #region Internal
        internal override List<Blob> GetTileBlobs(int wellId, int tileId, BlobType type)
        {
            int key = GetBlobKey(wellId, tileId);
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

        internal override void SaveTileBlobs(string baseFolder)
        {
            string contourXml = Path.Combine(baseFolder, ComponentDataManager.ContourXmlFolder);
            WriteBlobsXml(contourXml);
            WriteBlobsBinary(baseFolder);
        }

        internal IList<Blob> CreateContourBlobs(int wellId, int tileId, ImageData data,
            double minArea, double maxArea = int.MaxValue)
        {
            IList<Blob> contours = data.FindContours(minArea, maxArea);
            int key = GetBlobKey(wellId, tileId);
            _contourBlobs[key] = contours.ToList();
            //ImageWidth = (int)data.XSize;
            //ImageHeight = (int) data.YSize;
            return contours;
        }

        #endregion

        #region Private
        private List<Blob> ReadTileBlobsInfoFromXml(int wellId, int tileId, BlobType type)
        {
            string filename = string.Format("contours_{0}_{1}.xml", wellId, wellId);
            string filepath = Path.Combine(_contourXmlPath, filename);
            if (File.Exists(filepath) == false) return EmptyBlobs.ToList();
            var doc = new XmlDocument();
            doc.Load(filepath);
            var root = doc.DocumentElement;
            if (root == null) throw new XmlSyntaxException("no root element found");
            string query = string.Format("descendant::field[@no='{0}']", tileId);
            var fieldNode = root.SelectSingleNode(query) as XmlElement;
            if (fieldNode == null) return EmptyBlobs.ToList();
            else
            {
                if (type == BlobType.Data)
                {
                    query = "descendant::data-blobs";
                }
                else if (type == BlobType.Contour)
                {
                    query = "descendant::contour-blobs";
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
            if (blobs.Count <= 0) return blobs;
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
                    new BinaryReader(File.Open(Path.Combine(BasePath, Name, filename), FileMode.Open,
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


        private void WriteBlobsFile(string folder, WriteTileBlobsFile writer)
        {
            ScanInfo info = Experiment.GetScanInfo(ScanId);
            IList<ScanRegion> regions = info.ScanRegionList;
            foreach (ScanRegion region in regions)
            {
                int wellId = region.WellId;
                IList<Scanfield> fields = region.ScanFieldList;
                foreach (Scanfield field in fields)
                {
                    int tileId = field.ScanFieldId;
                    int key = GetBlobKey(wellId, tileId);
                    if (_contourBlobs.ContainsKey(key))
                    {
                        writer(folder, wellId, tileId, BlobType.Contour);
                    }
                    if (_dataBlobs.ContainsKey(key))
                    {
                        writer(folder, wellId, tileId, BlobType.Data);
                    }

                }
            }
        }

        private void WriteBlobsXml(string folder)
        {
            WriteBlobsFile(folder, WriteBlobsXml);
        }

        private void WriteBlobsXml(string filepath, int wellId, int tileId, BlobType type)
        {
            var doc = new XmlDocument();
            string filename = Path.Combine(filepath,
                string.Format("contours_{0}_{1}.xml", wellId, wellId));
            if (File.Exists(filename) == false)
            {
                FileStream fs = File.Create(filename);
                fs.Close();
                CreateCommonNodeForContourXml(doc, wellId);
                doc.Save(filename);
            }
            doc.Load(filename);
            string query = string.Format("descendant::field[@no='{0}']", tileId);
            var fieldNode = doc.SelectSingleNode(query) as XmlElement ??
                            CreateFieldNodeForContourXml(doc, tileId);
            if (fieldNode == null) return;
            switch (type)
            {
                case BlobType.Contour:
                    query = "descendant::contour-blobs";
                    break;
                case BlobType.Data:
                    query = "descendant::data-blobs";
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
            doc.Save(filename);

        }

        private static XmlElement CreateFieldNodeForContourXml(XmlDocument doc, int tileId)
        {
            XmlElement root = doc.DocumentElement;
            if (root != null)
            {
                var regionNode = root.SelectSingleNode("descendant::region");
                if (regionNode == null) return null;
                var fieldNode = doc.CreateElement("field");
                fieldNode.SetAttribute("no", tileId.ToString(CultureInfo.InvariantCulture));
                regionNode.AppendChild(fieldNode);
                return fieldNode;
            }
            else return null;

        }
        private void CreateCommonNodeForContourXml(XmlDocument doc, int wellId)
        {

            var rootNode = doc.CreateElement("contours");
            rootNode.SetAttribute("version", SoftwareVersion.ToString());
            doc.AppendChild(rootNode);

            var compNode = doc.CreateElement("component");
            compNode.SetAttribute("name", Name);
            rootNode.AppendChild(compNode);

            var wellNode = doc.CreateElement("well");
            wellNode.SetAttribute("no", wellId.ToString(CultureInfo.InvariantCulture));
            compNode.AppendChild(wellNode);

            var regionNode = doc.CreateElement("region");
            regionNode.SetAttribute("no", wellId.ToString(CultureInfo.InvariantCulture));
            wellNode.AppendChild(regionNode);


        }
        private XmlElement CreateBlobNode(XmlDocument doc, int wellId, int tileId, BlobType type)
        {
            var blobName = string.Empty;
            List<Blob> blobs = EmptyBlobs.ToList();
            int key = GetBlobKey(wellId, tileId);
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

        private void WriteBlobsBinary(string folder)
        {
            WriteBlobsFile(folder, WriteBlobsBinary);
        }

        private void WriteBlobsBinary(string folder, int wellId, int tileId, BlobType type)
        {
            string filename;
            int key = GetBlobKey(wellId, tileId);
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

            if (File.Exists(filename) == false)
            {
                FileStream fs = File.Create(filename);
                fs.Close();
            }

            using (var writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                foreach (Blob blob in blobs)
                {
                    foreach (Point point in blob.PointsArray)
                    {
                        writer.Write((int)point.X);
                        writer.Write((int)point.Y);
                    }
                }
            }
        }

        #endregion
      
        #region Protected
        protected override void SetTileBlobs(int key, Blob blob, BlobType type)
        {
            IDictionary<int, List<Blob>> dict;
            switch (type)
            {
                case BlobType.Contour:
                    dict = _contourBlobs;
                    break;
                case BlobType.Data:
                    dict = _dataBlobs;
                    break;
                case BlobType.Background:
                    dict = _backgroudBlobs;
                    break;
                case BlobType.Peripheral:
                    dict = _peripheralBlobs;
                    break;
                default:
                    return;
            }
            SetTileBlobs(key, dict, blob);
        }

        #endregion
        private static void SetTileBlobs(int key, IDictionary<int, List<Blob>> dict, Blob blob)
        {
           
            if (dict.ContainsKey(key) == false)
            {
                dict[key] = EmptyBlobs.ToList();
            }
            List<Blob> blobs = dict[key];
            blobs.Add(blob);
        }

       
        #endregion

        internal override IList<BioEvent> CreateEvents(int wellId, int tileId, IDictionary<string, ImageData> imageDict,
            BlobDefine define)
        {
            int key = GetBlobKey(wellId, tileId);
            List<Blob> contours = _contourBlobs[key];
            var evs = new List<BioEvent>(contours.Count);
            var dataBlobs = new List<Blob>(contours.Count);
            var removed = new List<Blob>();
            if (EventsDict.ContainsKey(wellId) == false)
            {
                EventsDict[wellId] = new List<BioEvent>();
            }
            List<BioEvent> stored = EventsDict[wellId];
            int id = stored.Count + 1;//1 base
            foreach (Blob contour in contours)
            {
                if (contour.TouchesEdge(define.DataExpand, ImageWidth, ImageHeight) == false)
                {
                    Blob dataBlob = contour.CreateExpanded(define.DataExpand, 0, 0);
                    if (dataBlob != null)
                    {
                        dataBlob.Id = id;                     
                        id++;
                        BioEvent ev = CreateEvent(contour, dataBlob, define, imageDict, wellId, tileId);
                        if (ev != null)
                        {
                            evs.Add(ev);
                            dataBlobs.Add(dataBlob);
                        }
                        else
                        {
                            removed.Add(contour);
                        }
                    }
                }
            }
            stored.AddRange(evs);
            _dataBlobs[key] = dataBlobs;
            foreach (Blob blob in removed)
            {
                contours.Remove(blob);
            }
            ResetEventCountDict();
            return evs;
        }
    }
}
