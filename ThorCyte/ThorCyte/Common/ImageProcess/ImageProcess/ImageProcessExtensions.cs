using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcess
{
    public static class ImageProcessExtensions
    {
        private const double DpiX = 96;
        private const double DpiY = 96;

        public static BitmapSource ToBitmapSource(this ImageData data, int sourceDepth = 14)
        {
            var width = (int) data.XSize;
            var height = (int) data.YSize;
            int stride = width*data.Channels;
            const int destDepth = 8;
            int rightShift = sourceDepth - destDepth;
            IntPtr buffer = DoRightShift(data, rightShift);
            BitmapSource bmp;
            try
            {
                if (data.IsGray)
                {
                    bmp = BitmapSource.Create(width, height, DpiX, DpiY, PixelFormats.Gray8,
                        BitmapPalettes.Gray256, buffer, stride*height, stride);
                }
                else
                {
                    bmp = BitmapSource.Create(width, height, DpiX, DpiY, PixelFormats.Bgr24, null,
                        buffer, stride*height, stride);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return bmp;
        }

        public static UInt64 GetCupClocks()
        {
            return ImageProcessLib.GetCpuClocks();
        }

        private static IntPtr DoRightShift(ImageData data, int shift)
        {
            var width = (int) data.XSize;
            var height = (int) data.YSize;
            int n = width*height*data.Channels;
            IntPtr buffer = Marshal.AllocHGlobal(n);
            for (int i = 0; i < n; i++)
            {               
                var value = (byte) (data[i] >> shift);
                Marshal.WriteByte(buffer, i, value);
            }
            return buffer;
        }
    }
}
