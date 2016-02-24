using FreeImageAPI;
using ImageProcess;
using System;
using System.Collections.Generic;
using System.IO;
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
        public IntPtr DataBuffer { get; private set; }

        public MergeInfo(IntPtr dataBuffer)
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

        private int _maxIntensity;
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
            int key = GetKey(scanId, scanRegionId, channelId);
            if (_imageInfoDict.ContainsKey(key) == false)
            {
                _imageInfoDict[key] = GetImageInfo(scanId, scanRegionId, channelId);
            }
            ThorCyteImageInfo imageInfo = _imageInfoDict[key];
            ImageData data = ParallelMerge(imageInfo);          
            return data;
        }

        private ThorCyteImageInfo GetImageInfo(int scanId, int scanRegionId, int channelId)
        {
            ScanInfo info = _experiment.GetScanInfo(scanId);          
            ScanRegion scanRegion = info.ScanRegionList[scanRegionId];
            double xpixelSize = info.XPixcelSize;
            double ypixelSize = info.YPixcelSize;
            Rect bound = scanRegion.Bound;
            var totalWidth = (uint) Math.Round((decimal)bound.Width/(decimal)info.XPixcelSize);
            var totalHeight = (uint) Math.Round((decimal)bound.Height/(decimal)info.YPixcelSize);
            TryInitImageType();
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
                //ParallelMergeCallback(mergeInfo);// for test and debug
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
            IntPtr dataBuffer = state.DataBuffer;
            int scanFieldId = scanField.ScanFieldId;
            ScanRegion scanRegion = imageInfo.SRegion;
            uint totalWidth = imageInfo.XSize;
            uint totalHeight = imageInfo.YSize;
            int tileWidth = imageInfo.TileWidth;
            int channelId = imageInfo.ChannelId; 
            string filename = GetImageFileName(scanRegion.RegionId, channelId, scanFieldId);
            Int32Rect vaild = GetScanFieldValidBound(scanField.SFRect, imageInfo);          
            int startX = vaild.X;
            int startY = vaild.Y;
            var copiedX = vaild.Width;
            int copiedY = vaild.Height;
            FillBuffer(dataBuffer, startX, startY, (int) totalWidth, (int)totalHeight,copiedX, copiedY, tileWidth - copiedX,
                0, filename, _imageType);
        }

        private void FillBuffer(IntPtr total, int startX, int startY, int totalWidth, int totalHeight,
            int copiedX, int copiedY, string filename, ImageType type)
        {
            FillBuffer(total, startX, startY, totalWidth, totalHeight, copiedX, copiedY, 0, 0, filename, type);
        }

        private void FillBuffer(IntPtr total, int startX, int startY, int totalWidth, int totalHeight,
            int copiedX, int copiedY, int tilePosX, int tilePosY, string filename, ImageType type)
        {
            switch (type)
            {
                case ImageType.Jpeg:
                    FillJpegBuffer(total, startX, startY, totalWidth, totalHeight, copiedX, copiedY, tilePosX, tilePosY,
                        filename);
                    break;

                case ImageType.Tiff:
                    FillTiffBuffer(total, startX, startY, totalWidth, totalHeight, copiedX, copiedY, tilePosX, tilePosY,
                        filename);
                    break;
                default:
                    throw new ArgumentException("image type is none");
            }
        }

        private void FillJpegBuffer(IntPtr total, int startX, int startY, int totalWidth, int totalHeight,
            int copiedX, int copiedY, int tilePosX, int tilePosY, string filename)
        {
            FIBITMAP dib = FIBITMAP.Zero;         
            try
            {
                dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_JPEG, filename, FREE_IMAGE_LOAD_FLAGS.JPEG_ACCURATE);                
                FillJpegBuffer(dib, total, startX, startY, totalWidth,totalHeight, copiedX, copiedY, tilePosX, tilePosY);
            }
            finally
            {
                FreeImage.Unload(dib);               
            }
        }

        private  void FillJpegBuffer(FIBITMAP source, IntPtr dest, int startX, int startY, int totalWidth,
            int totalHeight, int copiedX, int copiedY, int tilePosX, int tilePosY)
        {
            var width = (int) FreeImage.GetWidth(source);
            var height = (int) FreeImage.GetHeight(source);
            var bitsPerPixel = (int) FreeImage.GetBPP(source);
            int channels = bitsPerPixel/8;
            // avoid out of range exception during the copy
            // check and change the copy length
            copiedX = CalcPixelsToBeCopied(tilePosX, copiedX, width);
            copiedY = CalcPixelsToBeCopied(tilePosY, copiedY, height);
            copiedX = CalcPixelsToBeCopied(startX, copiedX, totalWidth);
            copiedY = CalcPixelsToBeCopied(startY, copiedY, totalHeight);
            int offset = startY*totalWidth + startX;
            int y = startY;
            bool isLittleEndian = FreeImage.IsLittleEndian();
            unsafe
            {
                var pDest = (byte*) dest.ToPointer();
                if (isLittleEndian)
                {
                    for (int i = height - tilePosY - 1; i >= height - copiedY - tilePosY; i--)
                    {
                        
                        IntPtr line = FreeImage.GetScanLine(source, i);
                        var pLine = (byte*) line.ToPointer();
                        for (int j = 0; j < copiedX; j++)
                        {
                            var value = (ushort) (pLine[(tilePosX + j)*channels]*_maxIntensity/byte.MaxValue);
                            var hi = (byte)(value >> 8);
                            var low = (byte) (value);
                            pDest[2*(offset + j)] = low;
                            pDest[2*(offset + j) + 1] = hi;
                        }
                        y++;
                       
                        offset = y*totalWidth + startX;
                    }
                }
                else
                {
                    for (int i = height - tilePosY - 1; i >= height - copiedY - tilePosY; i--)
                    {
                        IntPtr line = FreeImage.GetScanLine(source, i);
                        var pLine = (byte*) line.ToPointer();
                        for (int j = 0; j < copiedX; j++)
                        {
                            var value = (ushort) (pLine[(tilePosX + j)*channels]*_maxIntensity/byte.MaxValue);
                            var hi = (byte)((value >> 8) & (0xFF));
                            var low = (byte)(value & 0xFF);
                            pDest[2*(offset + j)] = hi;
                            pDest[2*(offset + j) + 1] = low;
                        }
                        y++;
                        offset = y*totalWidth + startX;
                    }
                }
            }
        }

        private static void FillTiffBuffer(IntPtr total, int startX, int startY, int totalWidth,int totalHeight, int copiedX,
            int copiedY, int tilePosX, int tilePosY, string filename)
        {

            FIBITMAP gray = FIBITMAP.Zero;
            try
            {
                gray = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_TIFF, filename, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                FillTiffBuffer(gray, total, startX, startY, totalWidth, totalHeight, copiedX, copiedY, tilePosX,
                    tilePosY);
            }
            finally
            {
                FreeImage.Unload(gray);
            }
        }


        private static void FillTiffBuffer(FIBITMAP source, IntPtr dest, int startX, int startY, int totalWidth,
            int totalHeight, int copiedX, int copiedY, int tilePosX, int tilePosY)
        {
            var width = (int) FreeImage.GetWidth(source);
            var height = (int) FreeImage.GetHeight(source);
            // avoid out of range exception during the copy
            // check and change the copy length
            copiedX = CalcPixelsToBeCopied(tilePosX, copiedX, width);
            copiedY = CalcPixelsToBeCopied(tilePosY, copiedY, height);
            copiedX = CalcPixelsToBeCopied(startX, copiedX, totalWidth);
            copiedY = CalcPixelsToBeCopied(startY, copiedY, totalHeight);
            int y = startY;
            int offset = y*totalWidth + startX;
            unsafe
            {
                var pDest = (ushort*) dest.ToPointer();

                for (int i = height - tilePosY - 1; i >= height - copiedY - tilePosY; i--)
                {
                    IntPtr line = FreeImage.GetScanLine(source, i);
                    var pLine = (ushort*) line.ToPointer();
                    for (int j = 0; j < copiedX; j++)
                    {
                        pDest[offset + j] = pLine[tilePosX + j];
                    }
                    y++;
                    offset = y*totalWidth + startX;
                }
            }
        }



        private int CalcPixelsToBeCopied(double len, double step)
        {
            return (int) Math.Round((decimal) len/(decimal) step);
        }


        private static int CalcPixelsToBeCopied(int start, int len, int limit)
        {          
            return start + len <= limit ? len : limit - start;
        }

        private Int32Rect Intersect(Int32Rect rect1, Int32Rect rect2)
        {
            Int32Rect rect = Int32Rect.Empty;
            Int32Rect left = rect1.X <= rect2.X ? rect1 : rect2;
            Int32Rect right = rect1.X > rect2.X ? rect1 : rect2;
            if ((right.X > left.X + left.Width) || (right.Y + right.Height < left.Y) || (right.Y > left.Y + left.Height))
            {
                return rect;
            }
            rect = new Int32Rect { X = right.X, Y = left.Y > right.Y ? left.Y : right.Y };
            if (right.X + right.Width <= left.X + left.Width)
                rect.Width = right.Width;
            else
                rect.Width = left.Width + left.X - right.X;
            if (right.Height <= left.Height)
            {
                if ((left.Y > right.Y) && (right.Y < left.Y + left.Height))
                    rect.Height = right.Y + right.Height - left.Y;
                else if ((left.Y <= right.Y) && (left.Y + left.Height >= right.Y + right.Height))
                    rect.Height = right.Height;
                else
                    rect.Height = left.Y + left.Height - right.Y;
            }
            else
            {
                if (right.Y + right.Height <= left.Y + left.Height)
                    rect.Height = right.Y + right.Height - left.Y;
                else if ((left.Y > right.Y) && (right.Y + right.Height > left.Y + left.Height))
                    rect.Height = left.Height;
                else
                    rect.Height = left.Y + left.Height - right.Y;
            }
            if (rect.Width > 0 && rect.Height > 0)
                return rect;
            return Int32Rect.Empty;
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
        private Int32Rect GetScanFieldValidBound(Rect scanFieldBound, ThorCyteImageInfo imageInfo)
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
                ? CalcPixelsToBeCopied(scanFieldBound.Height + (scanRegionBound.Bottom - scanFieldBound.Bottom),
                    yPixelSize)
                : tileHeight;
            var totalWidth = (int) (imageInfo.XSize);
            var startX = (int) Math.Round((decimal) (scanFieldBound.X - scanRegionBound.X)/(decimal) xPixelSize);
            var startY = (int) Math.Round((decimal) (scanFieldBound.Y - scanRegionBound.Y)/(decimal) yPixelSize);
            startX = totalWidth - startX - copiedX;
            return new Int32Rect(startX, startY, copiedX, copiedY);
        }


        private ImageData GetResizedData(ImageData original, int width, int height, double scale)
        {
            var scaleWidth = (int)(width * scale);
            var scaleHeight = (int)(height * scale);
            var image = new ImageData((uint)scaleWidth, (uint)scaleHeight);        
            ImageProcessLib.Resize16U(original.DataBuffer, width, height, image.DataBuffer, scaleWidth, scaleHeight, 1);
            return image;
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
                return null;
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
                catch (NotSupportedException)
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
            int intensityBits = experiment.GetExperimentInfo().IntensityBits;
            _maxIntensity = (0x01 << intensityBits) - 1;
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int planeId, int timingFrameId)
        {
            return GetData(scanId, scanRegionId, channelId);
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int planeId, int timingFrameId, double scale,
            Int32Rect regionRect)
        {
            return GetData(scanId, scanRegionId, channelId, timingFrameId, scale, regionRect);
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int streamFrameId, int planeId,
            int timingFrameId)
        {         
            return GetData(scanId, scanRegionId, channelId);
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId, double scale,
            Int32Rect regionRect)
        {
            try
            {
                if (scale <= 0.0 || !regionRect.HasArea || regionRect.X < 0 || regionRect.Y < 0)
                    return null;
                var original = new ImageData((uint)regionRect.Width, (uint)regionRect.Height);
                ThorCyteImageInfo imageInfo = GetImageInfo(scanId, scanRegionId, channelId);
                int tileCount = imageInfo.SRegion.ScanFieldList.Count;
                var tasks = new List<Task>(tileCount);
                for (int i = 0; i < tileCount; i++)
                {
                    Scanfield scanField = imageInfo.SRegion.ScanFieldList[i];
                    Int32Rect tileRect = GetScanFieldValidBound(scanField.SFRect, imageInfo);
                   
                    Int32Rect rect = Intersect(regionRect, tileRect);
                    if (rect != Int32Rect.Empty)
                    {
                        string filePath = GetImageFileName(scanRegionId, channelId, scanField.ScanFieldId);
                        if (File.Exists(filePath))
                        {
                            int tilePosX = rect.X - tileRect.X;
                            int tilePosY = rect.Y - tileRect.Y;
                            tasks.Add(
                                Task.Factory.StartNew(
                                    () =>
                                        FillBuffer(original.DataBuffer, rect.X - regionRect.X,
                                            rect.Y - regionRect.Y,
                                            regionRect.Width, regionRect.Height, rect.Width, rect.Height, tilePosX,
                                            tilePosY, filePath, _imageType)));

                            // for test and debug
                            //FillBuffer(original.DataBuffer, rect.X - regionRect.X,
                            //    rect.Y - regionRect.Y,
                            //    regionRect.Width, regionRect.Height, rect.Width, rect.Height, tilePosX,
                            //    tilePosY, filePath, _imageType);
                        }
                    }
                }

                if (tasks.Count > 0)
                {
                    Task.WaitAll(tasks.ToArray());
                }

                ImageData image = GetResizedData(original, regionRect.Width, regionRect.Height, scale);
                original.Dispose();
                return image;
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
            catch (NotSupportedException)
            { }
            return null;
           
           
        }

        public ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int tileId,
            int timingFrameId)
        {
            return GetTileData(scanId, scanRegionId, channelId, 0, 0, tileId, 0);
        }

        public ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int planeId,
            int tileId, int timingFrameId)
        {
            try
            {
                if (tileId > 0)
                {
                    ScanInfo info = _experiment.GetScanInfo(scanId);
                    TryInitImageType();
                    int width = info.TileWidth;
                    int height = info.TiledHeight;
                    string name = GetImageFileName(scanRegionId, channelId, tileId);
                    var data = new ImageData((uint) (width), (uint) (height));
                    FillBuffer(data.DataBuffer, 0, 0, width, height, width, height, name, _imageType);
                    return data;
                }
                else
                {
                    return null;
                }
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
            catch (NotSupportedException)
            {
            }
            return null;
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId)
        {
            return GetData(scanId, scanRegionId, channelId);
        }

        #endregion

        #endregion

    }
}
