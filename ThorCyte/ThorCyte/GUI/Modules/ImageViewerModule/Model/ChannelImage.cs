using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.Infrastructure.Types;
using ImageProcess;
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
        public string ChannelName { get; set; }
        //public int ChannelId { get; set; }
        private BitmapSource _image;
        public BitmapSource Image
        {
            get { return _image; }
            set { SetProperty<BitmapSource>(ref _image, value, "Image"); }
        }
        private BitmapSource _thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set { SetProperty<BitmapSource>(ref _thumbnail, value, "Thumbnail"); }
        }
        public ImageData ImageData { get; set; }
        public ImageData ThumbnailImageData { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        public bool IsComputeColor { get; set; }
        public Dictionary<Channel,Color> ComputeColorDic { get; set; }
        public Dictionary<Channel, Tuple<ImageData, Color>> ComputeColorDicWithData { get; set; }

    }
}
