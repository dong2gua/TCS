using FreeImageAPI;
using ImageProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.Infrastructure.Interfaces
{
    internal class ThorCyteImageInfo
    {
        public ScanRegion SRegion;
        public uint XSize;
        public uint YSize;
        public double XPixselSize;
        public double YPixselSize;
        public int ChannelId;
        public int TileHeight;
        public int TileWidth;
    }

    internal class MergeInfo
    {
        public ThorCyteImageInfo Info { get; set; }
        public Scanfield ScanField { get; set; }
        public ushort[] DataBuffer { get; private set; }

        public MergeInfo(ushort[] dataBuffer)
        {
            DataBuffer = dataBuffer;
        }
    }

    public class ThorCyteData : IData
    {

        #region Fields

        private const int ScanCount = 1;
        private const int DefaultSize = 10;
        private string _imageBasePath = string.Empty;
        private ThorCyteExperiment _experiment;
        private const string Jpeg = ".jpg";
        private const string Tiff = ".tif";
        private ImageType _imageType = ImageType.None;
        private string _extension = string.Empty;

        private readonly Dictionary<int, ThorCyteImageInfo> _imageInfoDict =
            new Dictionary<int, ThorCyteImageInfo>(DefaultSize);

        internal enum ImageType
        {
            None = 0,
            Jpeg,
            Tiff
        }

        #endregion

        #region Methods

        #region Private

        private static int GetKey(int scanId, int scanRegionId, int channelId)
        {
            int scanIdBits = (scanId - 1) & 0x01; //bit 0 for scanId
            int scanRegionIdBits = (scanRegionId) & 0xFFFFFF; // bits 1-24 for scanRegionId
            int channelIdBits = channelId & 0x7F;
            int key = (scanIdBits << 31) | (scanRegionIdBits << 7) | (channelIdBits);
            return key;
        }

        private static string ToImageExtension(ImageType type)
        {
            switch (type)
            {
                case ImageType.Jpeg:
                    return Jpeg;
                case ImageType.Tiff:
                    return Tiff;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanId"></param>
        /// <param name="scanRegionId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        private ImageData GetImageData(int scanId, int scanRegionId, int channelId)
        {
            Stopwatch watch = Stopwatch.StartNew();
            int key = GetKey(scanId, scanRegionId, channelId);
            if (_imageInfoDict.ContainsKey(key) == false)
            {
                _imageInfoDict[key] = GetImageInfo(scanId, scanRegionId, channelId);
            }
            ThorCyteImageInfo imageInfo = _imageInfoDict[key];
            //ImageData data = SyncMerge(imageInfo);
            ImageData data = ParallelMerge(imageInfo);
            Stopwatch.StartNew();
            Debug.WriteLine("times : {0} ms", watch.ElapsedMilliseconds);
            return data;
        }

        private ThorCyteImageInfo GetImageInfo(int scanId, int scanRegionId, int channelId)
        {
            ScanInfo info = _experiment.GetScanInfo(scanId);
            TryInitImageType();
            ScanRegion scanRegion = info.ScanRegionList[scanRegionId];
            double xpixelSize = info.XPixcelSize;
            double ypixelSize = info.YPixcelSize;
            Rect bound = scanRegion.Bound;
            var totalWidth = (uint) Math.Ceiling(bound.Width/info.XPixcelSize);
            var totalHeight = (uint) Math.Ceiling(bound.Height/info.YPixcelSize);
            return new ThorCyteImageInfo
            {
                SRegion = scanRegion,
                ChannelId = channelId,
                XPixselSize = xpixelSize,
                YPixselSize = ypixelSize,
                XSize = totalWidth,
                YSize = totalHeight,
                TileHeight = info.TiledHeight,
                TileWidth = info.TileWidth
            };

        }

        private void TryInitImageType()
        {
            if (_imageType == ImageType.None)
            {
                _imageType = _experiment.ImageType;
                if (_imageType != ImageType.None)
                {
                    _extension = ToImageExtension(_imageType);
                }
                else
                    throw new FileNotFoundException(
                        "no image file of specify extension found, only support jpeg, png and tiff file");
            }
        }

        private string GetImageFileName(int scanRegionId, int channelId, int fieldNo)
        {
            return _imageType == ImageType.None
                ? string.Empty
                : Path.Combine(_imageBasePath,
                    string.Format("{0}_{1}_{2}{3}", scanRegionId + 1, fieldNo, channelId, _extension));
        }

        private ImageData SyncMerge(ThorCyteImageInfo imageInfo)
        {
            ScanRegion scanRegion = imageInfo.SRegion;
            Rect bound = scanRegion.Bound;
            uint totalWidth = imageInfo.XSize;
            uint totalHeight = imageInfo.YSize;
            int tileHeight = imageInfo.TileHeight;
            int tileWidth = imageInfo.TileWidth;
            int channelId = imageInfo.ChannelId;
            double xpixelSize = imageInfo.XPixselSize;
            double ypixelSize = imageInfo.YPixselSize;
            List<Scanfield> scanFields = scanRegion.ScanFieldList;
            var data = new ImageData(totalWidth, totalHeight);
            foreach (Scanfield scanField in scanFields)
            {
                int scanFieldId = scanField.ScanFieldId;
                string filename = GetImageFileName(scanRegion.RegionId, channelId, scanFieldId);
                Rect rect = scanField.SFRect;
                int copiedX = rect.Right > bound.Right
                    ? CalcPixelsToBeCopied(rect.Width + (bound.Right - rect.Right), xpixelSize)
                    : tileWidth;
                int copiedY = rect.Bottom > bound.Bottom
                    ? CalcPixelsToBeCopied(rect.Height + (bound.Bottom - rect.Bottom), ypixelSize)
                    : tileHeight;
                var startX = (int) (Math.Floor(rect.X - bound.X)/xpixelSize);
                var startY = (int) (Math.Floor(rect.Y - bound.Y)/ypixelSize);
                startX = (int) (totalWidth - startX - copiedX);
                FillBuffer(data.DataBuffer, startX, startY, (int) totalWidth, copiedX, copiedY, tileWidth - copiedX,
                    tileHeight - copiedY, filename, _imageType);
            }
            return data;
        }


        private ImageData ParallelMerge(ThorCyteImageInfo imageInfo)
        {
            ScanRegion scanRegion = imageInfo.SRegion;
            uint totalWidth = imageInfo.XSize;
            uint totalHeight = imageInfo.YSize;
            List<Scanfield> scanFields = scanRegion.ScanFieldList;
            var data = new ImageData(totalWidth, totalHeight);
            int n = scanFields.Count;
            var tasks = new Task[n];
            for (int i = 0; i < n; i++)
            {
                var mergeInfo = new MergeInfo(data.DataBuffer) {ScanField = scanFields[i], Info = imageInfo};
                tasks[i] = Task.Factory.StartNew(ParallelMergeCallback, mergeInfo);
            }
            Task.WaitAll(tasks);
            return data;
        }

        private void ParallelMergeCallback(object state)
        {
            ParallelMergeCallback((MergeInfo) state);
        }

        private void ParallelMergeCallback(MergeInfo state)
        {
            ThorCyteImageInfo imageInfo = state.Info;
            Scanfield scanField = state.ScanField;
            ushort[] dataBuffer = state.DataBuffer;
            int scanFieldId = scanField.ScanFieldId;
            ScanRegion scanRegion = imageInfo.SRegion;
            Rect bound = scanRegion.Bound;
            uint totalWidth = imageInfo.XSize;
            int tileHeight = imageInfo.TileHeight;
            int tileWidth = imageInfo.TileWidth;
            int channelId = imageInfo.ChannelId;
            double xpixelSize = imageInfo.XPixselSize;
            double ypixelSize = imageInfo.YPixselSize;
            string filename = GetImageFileName(scanRegion.RegionId, channelId, scanFieldId);
            Rect rect = scanField.SFRect;
            int copiedX = rect.Right > bound.Right
                ? CalcPixelsToBeCopied(rect.Width + (bound.Right - rect.Right), xpixelSize)
                : tileWidth;
            int copiedY = rect.Bottom > bound.Bottom
                ? CalcPixelsToBeCopied(rect.Height + (bound.Bottom - rect.Bottom), ypixelSize)
                : tileHeight;
            var startX = (int) (Math.Floor(rect.X - bound.X)/xpixelSize);
            var startY = (int) (Math.Floor(rect.Y - bound.Y)/ypixelSize);
            startX = (int) (totalWidth - startX - copiedX);
            FillBuffer(dataBuffer, startX, startY, (int) totalWidth, copiedX, copiedY, tileWidth - copiedX,
                0, filename, _imageType);
        }


        private void FillBuffer(ushort[] total, int startX, int startY, int totalWidth, int copiedX,
            int copiedY, string filename, ImageType type)
        {
            FillBuffer(total, startX, startY, totalWidth, copiedX, copiedY, 0, 0, filename, type);
        }

        private void FillBuffer(ushort[] total, int startX, int startY, int totalWidth, int copiedX,
            int copiedY, int tilePosX, int tilePosY, string filename, ImageType type)
        {
            switch (type)
            {
                case ImageType.Jpeg:
                    FillJpegBuffer(total, startX, startY, totalWidth, copiedX, copiedY, tilePosX, tilePosY, filename);
                    break;
                case ImageType.Tiff:
                    FillTiffBuffer(total, startX, startY, totalWidth, copiedX, copiedY, tilePosX, tilePosY, filename);
                    break;
                default:
                    throw new ArgumentException("image type is none");
            }

        }

        private static void FillJpegBuffer(ushort[] total, int startX, int startY, int totalWidth, int copiedX,
            int copiedY, int tilePosX, int tilePosY, string filename)
        {
            FIBITMAP dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_JPEG, filename, 0);
            FIBITMAP gray = FreeImage.ConvertToGreyscale(dib);
            uint width = FreeImage.GetWidth(gray);
            var height = (int) FreeImage.GetHeight(gray);
            var lineData = new byte[width];
            int offset = startY*totalWidth + startX;
            for (int i = height - tilePosY - 1; i >= height - copiedY - tilePosY; i--)
            {
                IntPtr line = FreeImage.GetScanLine(gray, i);
                Marshal.Copy(line, lineData, 0, (int) width);
                ushort[] ushortLineData = Array.ConvertAll(lineData, b => (ushort) (b << 8));
                Array.Copy(ushortLineData, tilePosX, total, offset, copiedX);
                startY++;
                offset = startY*totalWidth + startX;
            }
        }

        private static void FillTiffBuffer(ushort[] total, int startX, int startY, int totalWidth, int copiedX,
            int copiedY, int tilePosX, int tilePosY, string filename)
        {
            FIBITMAP gray = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_TIFF, filename, 0);
            uint width = FreeImage.GetWidth(gray);
            var height = (int)FreeImage.GetHeight(gray);
            uint bitsPerPixel = FreeImage.GetBPP(gray);
            uint stride = (width * bitsPerPixel + 7) / 8;          
            const int size = sizeof(ushort);
            var lineData = new byte[stride];
            int offset = startY * totalWidth * size + startX * size;
            for (int i = height - 1 - tilePosY; i >= height - copiedY; i--)
            {
                IntPtr line = FreeImage.GetScanLine(gray, i);
                Marshal.Copy(line, lineData, 0, (int)stride);
                Buffer.BlockCopy(lineData, tilePosX*size, total, offset, copiedX*size);
                startY++;
                offset = startY*totalWidth*size + startX*size;
            }
        }

        private int CalcPixelsToBeCopied(double len, double step)
        {
            return (int) Math.Floor(len/step);
        }

        /// <summary>
        /// init root path of data to IData interface
        /// </summary>
        /// <param name="path"></param>
        private void SetExperimentPath(string path)
        {
            _imageBasePath = Path.Combine(path, "carrier1", "scan1", "FLD");
            if (!Directory.Exists(_imageBasePath))
                throw new DirectoryNotFoundException(string.Format("{0} folder not found!", _imageBasePath));
        }

        /// <summary>
        /// get vaild bound rect of a scan field, vaild means
        /// the rectangle part in the scan region bound
        /// </summary>
        /// <returns></returns>
        private Rect GetScanFieldValidBound(Rect scanFieldBound, ThorCyteImageInfo imageInfo)
        {
            Rect scanRegionBound = imageInfo.SRegion.Bound;
            int tileWidth = imageInfo.TileWidth;
            int tileHeight = imageInfo.TileHeight;
            double xPixelSize = imageInfo.XPixselSize;
            double yPixelSize = imageInfo.YPixselSize;
            int copiedX = scanFieldBound.Right > scanRegionBound.Right
                ? CalcPixelsToBeCopied(scanFieldBound.Width + (scanRegionBound.Right - scanFieldBound.Right), xPixelSize)
                : tileWidth;
            int copiedY = scanFieldBound.Bottom > scanRegionBound.Bottom
                ? CalcPixelsToBeCopied(scanFieldBound.Height + (scanRegionBound.Bottom - scanFieldBound.Bottom), yPixelSize)
                : tileHeight;
            var totalWidth = (int) imageInfo.XSize;
            var startX = (int) ((scanFieldBound.X - scanRegionBound.X)/xPixelSize);
            var startY = (int)((scanFieldBound.Y - scanRegionBound.Y) / yPixelSize);
                
            startX = totalWidth - startX - copiedX;
            return new Rect(startX, startY, copiedX, copiedY);
        }
        
        private void Clear()
        {
            _extension = string.Empty;
            _imageBasePath = string.Empty;
            _imageInfoDict.Clear();
            _imageType = ImageType.None;
        }

        #endregion

        #region Methods in IData interface



        /// <summary>
        /// Get image data by scanId, scanRegionId and channelId.All ids are 1-based exposed to outside.
        /// It would be better to keep all id in same rule.althorgh in inside the implement code
        /// some of the ids are not 1-base, such as channel id
        /// </summary>
        /// <param name="scanId"></param>
        /// <param name="scanRegionId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public ImageData GetData(int scanId, int scanRegionId, int channelId)
        {
            if (scanId != ScanCount)
                throw new ArgumentOutOfRangeException("scanId");
            else
            {
                try
                {
                    return GetImageData(scanId, scanRegionId, channelId);
                }
                catch (ArgumentNullException)
                {

                }
                catch (ArgumentException)
                {

                }
                catch (NullReferenceException)
                {

                }
                catch (DirectoryNotFoundException)
                {

                }
                return null;
            }
        }


        public void SetExperimentInfo(IExperiment experiment)
        {
            Clear();
            _experiment = (ThorCyteExperiment) experiment;
            SetExperimentPath(_experiment.BasePath);

        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int streamFrameId, int planeId,
            int timingFrameId)
        {
            return streamFrameId == 0 && planeId == 0 && timingFrameId == 0
                ? GetData(scanId, scanRegionId, channelId)
                : null;
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId, double scale,
            Int32Rect regionRect)
        {
            if (scale <= 0.0 || !regionRect.HasArea || regionRect.X < 0 || regionRect.Y < 0)
                return null;
            var myRegionRect = new Rect(regionRect.X, regionRect.Y, regionRect.Width, regionRect.Height);
            var image = new ImageData((uint)regionRect.Width, (uint)regionRect.Height);
            ThorCyteImageInfo imageInfo = GetImageInfo(scanId, scanRegionId, channelId);         
            int tileCount = imageInfo.SRegion.ScanFieldList.Count;

            var tasks = new List<Task>(tileCount);
            for (int i = 0; i < tileCount; i++)
            {
                Scanfield scanField = imageInfo.SRegion.ScanFieldList[i];
                Rect tileRect = GetScanFieldValidBound(scanField.SFRect, imageInfo);

                Rect rect = Rect.Intersect(myRegionRect, tileRect);
                if (rect != Rect.Empty)
                {
                    
                    string filePath = GetImageFileName(scanRegionId, channelId, scanField.ScanFieldId);
                    if (File.Exists(filePath))
                    {
                        var tilePosX = (int)(rect.X - tileRect.X);
                        var tilePosY = (int)(rect.Y - tileRect.Y);
                        tasks.Add(
                            Task.Factory.StartNew(
                                () =>
                                    FillBuffer(image.DataBuffer, (int) (rect.X - myRegionRect.X),
                                        (int) (rect.Y - myRegionRect.Y),
                                        (int) myRegionRect.Width, (int) rect.Width, (int) rect.Height, tilePosX,
                                        tilePosY,
                                        filePath, _imageType)));
                    }
                }
            }
            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());
            return image.Resize((int)(regionRect.Width * scale), (int)(regionRect.Height * scale));
        }

        public ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int tileId,
            int timingFrameId)
        {
            return GetTileData(scanId, scanRegionId, channelId, streamFrameId, 0, tileId, timingFrameId);
        }

        public ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int planeId,
            int tileId,
            int timingFrameId)
        {
            if (streamFrameId == 0 && planeId == 0 && tileId == 0 || timingFrameId == 0)
            {
                ScanInfo info = _experiment.GetScanInfo(scanId);
                int width = info.TileWidth;
                int height = info.TiledHeight;
                string name = GetImageFileName(scanRegionId, channelId, tileId);
                var data = new ImageData((uint) (width), (uint) (height));
                FillBuffer(data.DataBuffer, 0, 0, width, width, height, name, _imageType);
                return data;

            }
            else
            {
                return null;
            }
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId)
        {
            return GetData(scanId, scanRegionId, channelId, 0, 0, timingFrameId);
        }

        #endregion

        #endregion




    }
}
