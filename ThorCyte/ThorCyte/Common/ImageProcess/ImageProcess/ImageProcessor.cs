
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace ImageProcess
{
    public static class ImageProcessor
    {
        public static ImageData Resize(this ImageData srcData, int dstWidth, int dstHeight)
        {
            var dstData = new ImageData((uint) dstWidth, (uint) dstHeight);
            GCHandle srcHandle = GCHandle.Alloc(srcData.DataBuffer, GCHandleType.Pinned);
            GCHandle dstHandle = GCHandle.Alloc(dstData.DataBuffer, GCHandleType.Pinned);
            ImageProcessLib.Resize16UC1(srcHandle.AddrOfPinnedObject(), (int) srcData.XSize, (int) srcData.YSize,
                dstHandle.AddrOfPinnedObject(), dstWidth, dstHeight);
            srcHandle.Free();
            dstHandle.Free();
            return dstData;
        }

        public static ImageData AddConstant(this ImageData srcData, ushort value)
        {
            var width = (int) srcData.XSize;
            var height = (int) srcData.YSize;
            var dstData = new ImageData(srcData.XSize, srcData.YSize);
            GCHandle srcHandle = GCHandle.Alloc(srcData.DataBuffer, GCHandleType.Pinned);
            GCHandle dstHandle = GCHandle.Alloc(dstData.DataBuffer, GCHandleType.Pinned);
            ImageProcessLib.AddConstant16UC1(value, srcHandle.AddrOfPinnedObject(), width, height,
                dstHandle.AddrOfPinnedObject());
            srcHandle.Free();
            dstHandle.Free();
            return dstData;
        }

        public static ImageData SubConstant(this ImageData srcData, ushort value)
        {
            var width = (int) srcData.XSize;
            var height = (int) srcData.YSize;
            var dstData = new ImageData(srcData.XSize, srcData.YSize);
            GCHandle srcHandle = GCHandle.Alloc(srcData.DataBuffer, GCHandleType.Pinned);
            GCHandle dstHandle = GCHandle.Alloc(dstData.DataBuffer, GCHandleType.Pinned);
            ImageProcessLib.SubConstant16UC1(value, srcHandle.AddrOfPinnedObject(), width, height,
                dstHandle.AddrOfPinnedObject());
            srcHandle.Free();
            dstHandle.Free();
            return dstData;
        }

        public static ImageData MulConstant(this ImageData srcData, ushort value)
        {
            var width = (int) srcData.XSize;
            var height = (int) srcData.YSize;
            var dstData = new ImageData(srcData.XSize, srcData.YSize);
            GCHandle srcHandle = GCHandle.Alloc(srcData.DataBuffer, GCHandleType.Pinned);
            GCHandle dstHandle = GCHandle.Alloc(dstData.DataBuffer, GCHandleType.Pinned);
            ImageProcessLib.MulConstant16UC1(value, srcHandle.AddrOfPinnedObject(), width, height,
                dstHandle.AddrOfPinnedObject());
            srcHandle.Free();
            dstHandle.Free();
            return dstData;
        }

        public static ImageData SetBrightnessAndContrast(this ImageData srcData, double alpha, ushort beta)
        {
            var dstData = new ImageData(srcData.XSize, srcData.YSize);
            int n = srcData.DataBuffer.Length;
            for (int i = 0; i < n; i++)
            {
                dstData.DataBuffer[i] = (ushort) (alpha*srcData.DataBuffer[i] + beta);
            }

            return dstData;
        }

        public static IList<ushort> GetDataInProfileLine(this ImageData data, Point start, Point end)
        {
            return data.GetDataInProfileLineInternal((int) start.X, (int) start.Y, (int) end.X, (int) end.Y).ToList();
        }


        internal static IEnumerable<ushort> GetDataInProfileLineInternal(this ImageData data, int startX, int startY, int endX,
            int endY)
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
            int error = dx/2;
            int ystep = (startY < endY) ? 1 : -1;
            int y = startY;
            for (int x = startX; x <= endX; x++)
            {
                int col = (steep ? y : x);
                int row = (steep ? x : y);
                yield return data.DataBuffer[row*data.XSize + col];
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
    }
}
