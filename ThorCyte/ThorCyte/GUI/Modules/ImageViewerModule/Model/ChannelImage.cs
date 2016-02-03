using ImageProcess;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Model
{
    public class ChannelImage : BindableBase
    {
        public ChannelImage()
        {
            Contrast = 1;
            Brightness = 0;
            IsComputeColor = false;
        }
        public Channel ChannelInfo { get; set; }
        public ComputeColor ComputeColorInfo { get; set; }
        public string ChannelName { get; set; }
        private Tuple<ImageSource, Int32Rect> _image;
        public Tuple<ImageSource, Int32Rect> Image
        {
            get { return _image; }
            set { SetProperty<Tuple<ImageSource, Int32Rect>>(ref _image, value, "Image"); }
        }
        private BitmapSource _thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set { SetProperty<BitmapSource>(ref _thumbnail, value, "Thumbnail"); }
        }
        public ImageData ImageData { get; set; }
        public Int32Rect DataRect { get; set; }
        public double DataScale { get; set; }
        public ImageData ThumbnailImageData { get; set; }
        public int Brightness { get; set; }
        public double Contrast { get; set; }
        public bool IsComputeColor { get; set; }
        public Dictionary<Channel,Color> ComputeColorDic { get; set; }
        public Dictionary<Channel, Tuple<ImageData, Color>> ComputeColorDicWithData { get; set; }
    }
}
