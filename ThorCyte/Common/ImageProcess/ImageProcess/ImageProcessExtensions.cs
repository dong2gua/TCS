using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcess
{
    public partial class ImageData
    {
        private const double DpiX = 96;
        private const double DpiY = 96;

        public BitmapSource ToBitmapSource(int sourceDepth = 14)
        {
            return ToBitmapSource(1.0, sourceDepth);
        }


        public BitmapSource ToBitmapSource(double scale, int sourceDepth = 14)
        {
            var width = (int)XSize;
            var height = (int)YSize;
            int stride = width * Channels;
            const int destDepth = 8;
            int rightShift = sourceDepth - destDepth;
            IntPtr buffer = DoRightShift(this, rightShift);
            BitmapSource bmp;
            double dpiY = DpiY/scale;
            try
            {
                if (IsGray)
                {
                    bmp = BitmapSource.Create(width, height, DpiX, dpiY, PixelFormats.Gray8,
                        BitmapPalettes.Gray256, buffer, stride*height, stride);
                }
                else
                {
                    bmp = BitmapSource.Create(width, height, DpiX, dpiY, PixelFormats.Bgr24, null,
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
