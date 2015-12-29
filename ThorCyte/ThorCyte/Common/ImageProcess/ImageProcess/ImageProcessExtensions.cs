using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcess
{
    public static class ImageProcessExtensions
    {
        private const double DpiX = 96;
        private const double DpiY = 96;

        public static BitmapSource ToBitmapSource(this ImageData data)
        {
            var width = (int)data.XSize;
            var height = (int) data.YSize;
            int stride = width*data.Channels;
            BitmapSource bmp;
            if (data.IsGray)
            {
                bmp = BitmapSource.Create(width, height, DpiX, DpiY, PixelFormats.Gray8,
                    BitmapPalettes.Gray256, Array.ConvertAll(data.DataBuffer, d => (byte) (d >> 8)), stride);
            }
            else
            {
                bmp = BitmapSource.Create(width, height, DpiX, DpiY, PixelFormats.Bgr24, null,
                    Array.ConvertAll(data.DataBuffer, d => (byte) (d >> 8)), stride);
            }
           
            return bmp;
        }
    }
}
