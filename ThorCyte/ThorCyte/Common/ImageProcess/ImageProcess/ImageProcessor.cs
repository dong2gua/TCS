
using System;
using System.Collections.Generic;
using System.Windows;

namespace ImageProcess
{
   
    public static class ImageProcessor
    {

        #region Public

        public static ImageData Resize(this ImageData srcData, int dstWidth, int dstHeight)
        {
            if (dstHeight <= 0) throw new ArgumentOutOfRangeException("dstWidth");
            if (dstWidth <= 0) throw new ArgumentOutOfRangeException("dstWidth");
            var dstData = new ImageData((uint) dstWidth, (uint) dstHeight);
            ImageProcessLib.Resize16U(srcData.DataBuffer, (int) srcData.XSize, (int) srcData.YSize,
                dstData.DataBuffer, dstWidth, dstHeight, srcData.Channels);
            return dstData;
        }

        public static ImageData AddConstant(this ImageData srcData, ushort value, int depth = 14)
        {
            var width = (int) srcData.XSize;
            var height = (int) srcData.YSize;
            var dstData = new ImageData(srcData.XSize, srcData.YSize);
            var maxValue = (ushort) ((0x01 << depth) - 1);
            ImageProcessLib.AddConstant16U(value, srcData.DataBuffer, width, height,
                dstData.DataBuffer, srcData.Channels, maxValue);

            return dstData;
        }

        public static ImageData Add(this ImageData srcData, ImageData other, int depth = 14)
        {
            if (srcData.XSize != other.XSize) throw new ArgumentException("Image data XSize not equal");
            if (srcData.YSize != other.YSize) throw new ArgumentException("Image data YSize not equal");
            if (srcData.Channels != other.Channels) throw new ArgumentException("Image data Channels not equal");
            var dstData = new ImageData(srcData.XSize, srcData.YSize, srcData.IsGray);
            var height = (int) srcData.XSize;
            var width = (int) srcData.YSize;
            int channels = srcData.Channels;
            var maxValue = (ushort) ((0x01 << depth) - 1);
            ImageProcessLib.Add16U(srcData.DataBuffer, other.DataBuffer, width, height, dstData.DataBuffer, channels,
                maxValue);
            return dstData;
        }



        public static ImageData SubConstant(this ImageData srcData, ushort value)
        {
            var width = (int) srcData.XSize;
            var height = (int) srcData.YSize;
            var dstData = new ImageData(srcData.XSize, srcData.YSize);
            ImageProcessLib.SubConstant16U(value, srcData.DataBuffer, width, height, dstData.DataBuffer,
                srcData.Channels);      
            return dstData;
        }

        public static ImageData Sub(this ImageData minuend, ImageData subtracter)
        {
            if (minuend.XSize != subtracter.XSize) throw new ArgumentException("Image data XSize not equal");
            if (minuend.YSize != subtracter.YSize) throw new ArgumentException("Image data YSize not equal");
            if (minuend.Channels != subtracter.Channels) throw new ArgumentException("Image data Channels not equal");
            var dstData = new ImageData(minuend.XSize, minuend.YSize, minuend.IsGray);
            var height = (int)minuend.XSize;
            var width = (int)minuend.YSize;
            int channels = minuend.Channels;
            ImageProcessLib.Sub16U(minuend.DataBuffer, subtracter.DataBuffer, width, height, dstData.DataBuffer,
                channels);
            return dstData;
        }

        //public static ImageData MulConstant(this ImageData srcData, ushort value, int depth= 14)
        //{
        //    var width = (int) srcData.XSize;
        //    var height = (int) srcData.YSize;
        //    var dstData = new ImageData(srcData.XSize, srcData.YSize);
        //    var maxValue = (ushort)((0x01 << depth) - 1);
        //    ImageProcessLib.MulConstant16U(value, srcData.DataBuffer, width, height, dstData.DataBuffer,
        //        dstData.Channels, maxValue);
        //    return dstData;
        //}

        public static ImageData MulConstant(this ImageData srcData, double value, int depth = 14)
        {
            return srcData.SetBrightnessAndContrast(value, 0, depth);
        }

        public static ImageData SetBrightnessAndContrast(this ImageData srcData, double alpha, int beta, int depth = 14)
        {
            var dstData = new ImageData(srcData.XSize, srcData.YSize, srcData.IsGray);
            var maxValue = (ushort)((0x01 << depth) - 1);
            int n = srcData.Length;
            for (int i = 0; i < n; i++)
            {
                var value = (int)(alpha * srcData[i] + beta);
                if (value > maxValue) value = maxValue;
                else if (value < 0) value = 0;
                dstData[i] = (ushort) value;
            }
            return dstData;
        }

        public static ushort Max(this ImageData data)
        {
            int channels = data.Channels;
            if (channels != 1) throw new ArgumentOutOfRangeException("data", "must be 1 channel image data");
            ushort maxValue;
            int status = ImageProcessLib.Max16U(data.DataBuffer, (int) data.XSize, (int) data.YSize, channels,
                out maxValue);
            return maxValue;
        }

        public static ushort Min(this ImageData data)
        {
            int channels = data.Channels;
            if (channels != 1) throw new ArgumentOutOfRangeException("data", "must be 1 channel image data");
            ushort minValue;
            int status = ImageProcessLib.Min16U(data.DataBuffer, (int)data.XSize, (int)data.YSize, channels,
                out minValue);
            return minValue;
        }

        public static ImageData Max(this ImageData data, ImageData other)
        {
            if (data.XSize != other.XSize) throw new ArgumentException("Image data XSize not equal");
            if (data.YSize != other.YSize) throw new ArgumentException("Image data YSize not equal");
            if (data.Channels != other.Channels) throw new ArgumentException("Image data Channels not equal");
            var dstData = new ImageData(data.XSize, data.YSize, data.IsGray);
            int status = ImageProcessLib.MaxEvery16U(data.DataBuffer, other.DataBuffer, (int) data.XSize,
                (int) data.YSize, data.Channels, dstData.DataBuffer);
            return dstData;
        }

        public static ImageData Min(this ImageData data, ImageData other)
        {
            if (data.XSize != other.XSize) throw new ArgumentException("Image data XSize not equal");
            if (data.YSize != other.YSize) throw new ArgumentException("Image data YSize not equal");
            if (data.Channels != other.Channels) throw new ArgumentException("Image data Channels not equal");
            var dstData = new ImageData(data.XSize, data.YSize, data.IsGray);
            int status = ImageProcessLib.MinEvery16U(data.DataBuffer, other.DataBuffer, (int)data.XSize,
                (int)data.YSize, data.Channels, dstData.DataBuffer);
            return dstData;
        }

        public static ImageData Invert(this ImageData data, int depth = 14)
        {
            var maxValue = (ushort) ((0x01 << depth) - 1);
            var dstData = new ImageData(data.XSize, data.YSize, data.IsGray);
            int status = ImageProcessLib.Invert16U(data.DataBuffer, (int) data.XSize, (int) data.YSize, data.Channels,
                maxValue, dstData.DataBuffer);
            return dstData;
        }

        public static IList<ushort> GetDataInProfileLine(this ImageData data, Point start, Point end)
        {
            List<Point> points = GetPointsInProfileLine((int) start.X, (int) start.Y, (int) end.X, (int) end.Y);
            return points.ConvertAll(pt =>
            {
                var x = (int) pt.X;
                var y = (int) pt.Y;
                return data[(int) (y*data.XSize + x)];
            });
        }

        public static IList<IList<ushort>> GetMultiChannelsDataInProfileLine(this ImageData data, Point start, Point end)
        {
            if (data.Channels != 3) throw new ArgumentOutOfRangeException("data", "Must be 3 channel image data");
            const int channels = 3;
            List<Point> points = GetPointsInProfileLine((int) start.X, (int) start.Y, (int) end.X, (int) end.Y);
            int n = points.Count;
            var first = new ushort[n];
            var second = new ushort[n];
            var third = new ushort[n];
            Int64 step = data.XSize*channels;
            for (int i = 0; i < n; i++)
            {
                Point pt = points[i];
                var x = (int) pt.X;
                var y = (int) pt.Y;
                first[i] = data[(int) (y*step + channels*x)];
                second[i] = data[(int) (y*step + channels*x + 1)];
                third[i] = data[(int) (y*step + channels*x + 2)];
            }
            return new IList<ushort>[] {first, second, third};
        }

        #endregion

        #region Private

        private static List<Point> GetPointsInProfileLine(int startX, int startY, int endX, int endY)
        {
            bool steep = Math.Abs(endY - startY) > Math.Abs(endX - startX);
            if (steep)
            {
                int t = startX;
                startX = startY;
                startY = t;
                t = endX; // swap endX and endY
                endX = endY;
                endY = t;
            }
            if (startX > endX)
            {
                int t = startX;
                startX = endX;
                endX = t;
                t = startY; // swap startY and endY
                startY = endY;
                endY = t;
            }
            int dx = endX - startX;
            int dy = Math.Abs(endY - startY);
            int error = dx / 2;
            int ystep = (startY < endY) ? 1 : -1;
            int y = startY;
            var n = (int)(Math.Sqrt(Math.Pow(startX - endX, 2) + Math.Pow(startY - endY, 2)));
            var points = new List<Point>(n);
            for (int x = startX; x <= endX; x++)
            {
                int col = (steep ? y : x);
                int row = (steep ? x : y);
                points.Add(new Point(col, row));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            return points;
        }

        #endregion
    }
}
