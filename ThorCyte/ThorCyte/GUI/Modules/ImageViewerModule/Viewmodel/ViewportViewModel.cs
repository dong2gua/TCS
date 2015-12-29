using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThorCyte.ImageViewerModule.Model;
using Prism.Mvvm;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Collections.ObjectModel;
using ImageProcess;
using Xceed.Wpf.Toolkit;
using ThorCyte.ImageViewerModule.View;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.ImageViewerModule.Events;
using Prism.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using System.Diagnostics;
namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ViewportViewModel : BindableBase
    {
        private ObservableCollection<ChannelImage> _channelImages;
        public ObservableCollection<ChannelImage> ChannelImages
        {
            get { return _channelImages; }
            set { SetProperty<ObservableCollection<ChannelImage>>(ref _channelImages, value, "ChannelImages"); }
        }
        private ChannelImage _currentChannelImage;
        public ChannelImage CurrentChannelImage
        {
            get { return _currentChannelImage; }
            set
            {
                if (value == _currentChannelImage) return;
                _currentChannelImage = value;
                if (_currentChannelImage != null)
                {
                    CurrentChannelImage.ImageData = getData(CurrentChannelImage, VisualRect, DataScale);
                    UpdateBitmap(CurrentChannelImage);
                    _updateCurrentChannelEvent.Publish(CurrentChannelImage);
                }
                OnPropertyChanged("CurrentChannelImage");
            }
        }
        private BitmapSource _image;
        public BitmapSource Image
        {
            get { return _image; }
            set { SetProperty<BitmapSource>(ref _image, value, "Image"); }
        }

        private Tuple<double, double, double> _scale = new Tuple<double, double, double>(1, 1, 1);
        public Tuple<double, double, double> Scale
        {
            get { return _scale; }
            set { SetProperty<Tuple<double, double, double>>(ref _scale, value, "Scale"); }
        }
        UpdateCurrentChannelEvent _updateCurrentChannelEvent;
        UpdateMousePointEvent _updateMousePointEvent;
        OperateChannelEvent _operateChannelEvent;
        public ICommand ListViewDeleteCommand { get; private set; }
        public ICommand ListViewEditCommand { get; private set; }
        private Size ImageSize;
        private Int32Rect VisualRect;
        private double VisualScale;
        private double DataScale;
        private double ActualScale;
        private const int ThumbnailSize = 80;
        private Point ZoomCenterPoint;
        private int CanvasWidth = 512;
        private int CanvasHeight = 512;
        IData _iData;
        private double AspectRatio;
        private int _scanId;
        private int _regionId;
        private int _tileId;
        private bool _isTile { get { return _tileId > 0; } }

        public void Zoomin()
        {
            ActualScale *= 1.5;
            if (_isTile) CalcZoomForTile();
            else CalcZoom();
        }
        public void Zoomout()
        {

            ActualScale /= 1.5;
            if (_isTile) CalcZoomForTile(); 
            else CalcZoom();
        }
        private void CalcZoom()
        {
            if (CurrentChannelImage == null) return;
            var lastDataScale = DataScale;
            var s = Math.Min(((double)CanvasWidth  / (double)ImageSize.Width), ((double)CanvasHeight  / (double)ImageSize.Height));
            if (ActualScale > 4) ActualScale = 4;
            else if (ActualScale < s) ActualScale = s;
            if (ActualScale > 1)
            {
                DataScale = 1;
                VisualScale = ActualScale;
            }
            else
            {
                DataScale = ActualScale;
                VisualScale = 1;
            }
            var width =(int)Math.Min( ( CanvasWidth+100)/DataScale, ImageSize.Width);
            var height =(int) Math.Min(( CanvasHeight + 100) / DataScale, ImageSize.Height);
            int x, y;
            x = (int)(ZoomCenterPoint.X - (ZoomCenterPoint.X - VisualRect.X) / lastDataScale * DataScale);
            y = (int)(ZoomCenterPoint.Y - (ZoomCenterPoint.Y - VisualRect.Y) / lastDataScale * DataScale);
            if (x < 0) x = 0;
            else if (x + width > ImageSize.Width) x = (int)(ImageSize.Width - width);
            if (y < 0) y = 0;
            else if (y + height > ImageSize.Height) y = (int)(ImageSize.Height - height);

            VisualRect = new Int32Rect(x, y, width, height);
            CurrentChannelImage.ImageData = getData(_currentChannelImage, VisualRect, DataScale);
            UpdateBitmap(CurrentChannelImage);
            Scale = new Tuple<double, double, double>(VisualScale, VisualScale, DataScale);
        }
        private void CalcZoomForTile()
        {
            if (CurrentChannelImage == null) return;
            var s = Math.Min(((double)CanvasWidth / (double)ImageSize.Width), ((double)CanvasHeight / (double)ImageSize.Height));
            if (ActualScale > 4) ActualScale = 4;
            else if (ActualScale < s) ActualScale = s;
            VisualScale = ActualScale;
            DataScale = 1;
            Scale = new Tuple<double, double, double>(VisualScale, VisualScale, DataScale);
        }
        public ViewportViewModel()
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _updateCurrentChannelEvent = eventAggregator.GetEvent<UpdateCurrentChannelEvent>();
            _updateMousePointEvent = eventAggregator.GetEvent<UpdateMousePointEvent>();
            _operateChannelEvent = eventAggregator.GetEvent<OperateChannelEvent>();
            ListViewDeleteCommand = new DelegateCommand<object>(OnListViewDelete);
            ListViewEditCommand = new DelegateCommand<object>(OnListViewEdit);

            ColorPicker a;
        }
        public void OnMousePoint(Point point)
        {
            int x = (int)(point.X / DataScale);
            int y = (int)(point.Y / DataScale);
            if (x >= ImageSize.Width || x < 0 || y >= ImageSize.Height || y < 0) return;
            var gv =0;
            //var gv = CurrentChannelImage.ImageData[point.X + point.Y * ImageSize.Height*DataScale];
            var status = new MousePointStatus() { Point = new Point(x, y) , GrayValue=gv,IsComputeColor=CurrentChannelImage.IsComputeColor};
            _updateMousePointEvent.Publish(status);

        }
        public Vector OnMoveVisualRect(Vector vector)
        {
            if (CurrentChannelImage == null) return new Vector(0,0);
            if(_isTile) return new Vector(0, 0);
            var lastX = VisualRect.X;
            var lastY = VisualRect.Y;

            VisualRect.X += (int)(vector.X/DataScale);
            VisualRect.Y += (int)(vector.Y/DataScale);
            if (VisualRect.X < 0) VisualRect.X = 0;
            else if (VisualRect.X+VisualRect.Width > ImageSize.Width) VisualRect.X = (int)(ImageSize.Width - VisualRect.Width);
            if (VisualRect.Y < 0) VisualRect.Y = 0;
            else if (VisualRect.Y+VisualRect.Height > ImageSize.Height) VisualRect.Y = (int)(ImageSize.Height - VisualRect.Height);
            
            CurrentChannelImage.ImageData= getData(CurrentChannelImage, VisualRect, DataScale);
            UpdateBitmap(CurrentChannelImage);
            return new Vector( (VisualRect.X-lastX)*DataScale, (VisualRect.Y - lastY) * DataScale);
        }
        public void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CanvasWidth = (int)e.NewSize.Width;
            CanvasHeight = (int)e.NewSize.Height;
            CalcZoom();
        }

        private ImageData getData(ChannelImage channel, Int32Rect rect, double scale)
        {
            ImageData data;
            if(channel.IsComputeColor)
            {
                channel.ComputeColorDicWithData = getComputeDictionary(channel.ComputeColorDic, rect, scale);
                data = getComputeData(channel.ComputeColorDicWithData, rect, scale);

            }
           else if (channel.ChannelInfo.IsvirtualChannel)
                data = getVirtualData(channel.ChannelInfo as VirtualChannel, rect, scale);
            else
            {
                data = getActiveData(channel.ChannelInfo, rect, scale);
            }
            return data;
        }
        private ImageData getThumbnailData(ChannelImage channel, Int32Rect rect, double scale)
        {
            if (_isTile)
                return getData(channel, rect, scale).Resize((int)(rect.Width * scale), (int)(rect.Height * scale));
            else
                return getData(channel, rect, scale);
        }
        private ImageData getActiveData(Channel channelInfo, Int32Rect rect, double scale)
        {
            if (_isTile)
                return _iData.GetTileData(_scanId, _regionId, channelInfo.ChannelId, 1, _tileId, 1);
            else
                return _iData.GetData(_scanId, _regionId, channelInfo.ChannelId, 1, scale, rect);
        }
        
        private void  TraverseVirtualChannel(Dictionary<Channel, ImageData>  dic ,VirtualChannel channel,Int32Rect rect,double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, getActiveData(channel.FirstChannel, rect, scale));
            }
            if(channel.SecondChannel!=null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, getActiveData(channel.SecondChannel, rect, scale));
                }

            }
        }
        private ImageData getVirtualData(VirtualChannel channel, Int32Rect rect,double scale)
        {
            var dic = new Dictionary<Channel, ImageData>();
            TraverseVirtualChannel(dic, channel as VirtualChannel, rect, scale);

            ImageData data1 = null, data2 = null;
            if(dic.ContainsKey(channel.FirstChannel))
            data1 = dic[channel.FirstChannel];

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert)
            {
                if (dic.ContainsKey(channel.SecondChannel))
                    data2 = dic[channel.SecondChannel];
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert))) return null;
            ImageData result = new ImageData(data1.XSize, data1.YSize);
            switch (channel.Operator)
            {
                case ImageOperator.Add:
                    for (int i= 0; i < result.XSize * result.YSize; i++)
                    {
                        var elemt = data1.DataBuffer[i] + data2.DataBuffer[i];
                        result.DataBuffer[i] = elemt > ushort.MaxValue ? ushort.MaxValue : (ushort)elemt;
                    }
                    break;
                case ImageOperator.Subtract:
                    for (int i = 0; i < result.XSize * result.YSize; i++)
                    {
                        var elemt = data1.DataBuffer[i] - data2.DataBuffer[i];
                        result.DataBuffer[i] = elemt < ushort.MinValue ? ushort.MinValue : (ushort)elemt;
                    }
                    break;
                case ImageOperator.Invert:
                    for (int i = 0; i < result.XSize * result.YSize; i++)
                    {
                        var elemt = ushort.MaxValue- data1.DataBuffer[i];
                        result.DataBuffer[i] = (ushort)elemt;
                    }
                    break;
                case ImageOperator.Max:
                    for (int i = 0; i < result.XSize * result.YSize; i++)
                    {
                        result.DataBuffer[i] = Math.Max(data1.DataBuffer[i], data2.DataBuffer[i]);
                    }
                    break;
                case ImageOperator.Min:
                    for (int i = 0; i < result.XSize * result.YSize; i++)
                    {
                        result.DataBuffer[i] = Math.Min(data1.DataBuffer[i], data2.DataBuffer[i]);
                    }
                    break;
                case ImageOperator.Multiply:
                    for (int i = 0; i < result.XSize * result.YSize; i++)
                    {
                        var elemt = data1.DataBuffer[i] *channel.Operand;
                        result.DataBuffer[i] = elemt > ushort.MaxValue ? ushort.MaxValue : (ushort)elemt;
                    }
                    break;
                default:
                    break;
            }
            return result;

        }
        private ImageData getComputeData(Dictionary<Channel, Tuple<ImageData, Color>> dic, Int32Rect rect, double scale)
        {
            var data = new ImageData((uint)(rect.Width* scale), (uint)(rect.Height * scale),false);
            foreach (var o in dic)
            {
                var d = ToComputeColor(o.Value.Item1.DataBuffer, o.Value.Item2);
                for (int i = 0; i < data.DataBuffer.Length; i++)
                {
                    var elemt = d[i] + data.DataBuffer[i];
                    data.DataBuffer[i] = elemt > ushort.MaxValue ? ushort.MaxValue : (ushort)elemt;
                }
            }
            return data;
        }
        private ushort[] ToComputeColor(ushort[] data, Color color)
        {
            var n = data.Length;
            ushort[] computeData = new ushort[n * 3];
            for (int i = 0; i < n; i++)
            {
                computeData[i * 3] = (ushort)((double)color.B * (double)(data[i]) / 255.0);
                computeData[i * 3 + +1] = (ushort)((double)color.G * (double)(data[i]) / 255.0);
                computeData[i * 3 + 2] = (ushort)((double)color.R * (double)(data[i]) / 255.0);
                //computeData[i * YSize * 4 + j * 4 + 3] = 255;
            }
            return computeData;
        }
        private Dictionary<Channel, Tuple<ImageData, Color>> getComputeDictionary(Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect ,double scale)
        {
            var datadic = new Dictionary<Channel, ImageData>();
            foreach (var o in ComputeColorDictionary)
            {
                if(o.Key.IsvirtualChannel)
                    TraverseVirtualChannel(datadic, o.Key as VirtualChannel, rect, scale);
                else
                if (!datadic.ContainsKey(o.Key))
                    datadic.Add(o.Key, getActiveData(o.Key, rect, scale));
            }

            var dic = new Dictionary<Channel, Tuple<ImageData, Color>>();
            foreach (var o in ComputeColorDictionary)
            {
                var data = datadic[o.Key];
                dic.Add(o.Key, new Tuple<ImageData, Color>(data, o.Value));
            }
            return dic;
        }
        private void UpdateBitmap(ChannelImage channelImage)
        {
            ImageData result = null;
            if (channelImage.Contrast == 1 && channelImage.Brightness == 0)
                result = channelImage.ImageData;
            else
                result = channelImage.ImageData.SetBrightnessAndContrast(channelImage.Contrast,(ushort) channelImage.Brightness);
            channelImage.Image = result.ToBitmapSource();
        }

    private void UpdateThumbnail(ChannelImage channelImage)
        {
            ImageData result = null;
            if (channelImage.Contrast == 1 && channelImage.Brightness == 0)
                result = channelImage.ThumbnailImageData;
            else
                result = channelImage.ThumbnailImageData.MulConstant((ushort)channelImage.Contrast).AddConstant((ushort)channelImage.Brightness);
            channelImage.Thumbnail = result.ToBitmapSource();
        }
        public void Initialization(IData iData, ScanInfo scanInfo, int scanId, int regionId, int tileId)
        {
            _iData = iData;
            _scanId = scanId;
            _regionId = regionId;
            _tileId = tileId;
            var channels = scanInfo.ChannelList;
            var virtualChannels = scanInfo.VirtualChannelList;
            var computeColors = scanInfo.ComputeColorList;
            if(_isTile)
            {
                var width = (int)(scanInfo.TileWidth );
                var height = (int)(scanInfo.TileWidth);
                ImageSize = new Size(width, height);
                DataScale = 1;
                VisualScale = Math.Min(((double)CanvasWidth / (double)ImageSize.Width), ((double)CanvasHeight / (double)ImageSize.Height));
                Scale = new Tuple<double, double, double>(VisualScale, VisualScale, DataScale);
                ActualScale = DataScale;
            }
            else
            {
                var width = (int)(scanInfo.ScanRegionList[regionId].Bound.Width / scanInfo.XPixcelSize);
                var height = (int)(scanInfo.ScanRegionList[regionId].Bound.Height / scanInfo.YPixcelSize);
                ImageSize = new Size(width, height);
                DataScale = Math.Min(((double)CanvasWidth / (double)ImageSize.Width), ((double)CanvasHeight / (double)ImageSize.Height));
                VisualScale = 1;
                Scale = new Tuple<double, double, double>(VisualScale, VisualScale, DataScale);
                ActualScale = DataScale;
            }

            _channelImages = new ObservableCollection<ChannelImage>();
            VisualRect = new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height);
            foreach (var o in channels)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                channelImage.ThumbnailImageData = getThumbnailData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            foreach (var o in virtualChannels)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                channelImage.ThumbnailImageData = getThumbnailData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            foreach (var o in computeColors)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelName = o.Name;
                channelImage.IsComputeColor = true;
                channelImage.ComputeColorDic = o.ComputeColorDictionary;
                channelImage.ThumbnailImageData = getThumbnailData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            OnPropertyChanged("ChannelImages");
            CurrentChannelImage = _channelImages.FirstOrDefault();
        }


        public void AddVirtualChannel(VirtualChannel virtrualChannel)
        {
            var channelImage = new ChannelImage();
            channelImage.ChannelInfo = virtrualChannel;
            channelImage.ChannelName = virtrualChannel.ChannelName;
            channelImage.ThumbnailImageData = getThumbnailData(channelImage, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
            UpdateThumbnail(channelImage);
            _channelImages.Add(channelImage);
            CurrentChannelImage = channelImage;
        }
        public void EditVirtualChannel(VirtualChannel virtrualChannel)
        {
            foreach (var o in _channelImages.Where(x => !x.IsComputeColor))
            {
                if (o.ChannelInfo == virtrualChannel)
                {
                    //o.ImageData = getVirtualData(virtrualChannel, VisualRect, DataScale);
                    CurrentChannelImage = o;
                }
            }
        }
        public void DeleteVirtualChannel(VirtualChannel virtrualChannel)
        {
            foreach (var o in _channelImages.Where(x => !x.IsComputeColor))
            {
                if (o.ChannelInfo == virtrualChannel)
                {
                    _channelImages.Remove(o);
                    CurrentChannelImage = ChannelImages.FirstOrDefault();
                    break;
                }
            }
        }
        public void AddComputeColor(ComputeColor computeColor)
        {
            var channelImage = new ChannelImage();
            channelImage.ChannelName = computeColor.Name;
            channelImage.IsComputeColor = true;
            channelImage.ComputeColorDic = computeColor.ComputeColorDictionary;
            channelImage.ThumbnailImageData = getThumbnailData(channelImage, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
            UpdateThumbnail(channelImage);
            _channelImages.Add(channelImage);
            CurrentChannelImage = channelImage;
        }
        public void EditComputeColor(ComputeColor computeColor)
        {
            foreach (var o in _channelImages.Where(x=>x.IsComputeColor))
            {
                if (o.ChannelName == computeColor.Name)
                {
                    o.ComputeColorDic = computeColor.ComputeColorDictionary;
                    CurrentChannelImage = o;
                }
            }
        }
        public void DeleteComputeColor(ComputeColor computeColor)
        {
            foreach (var o in _channelImages.Where(x => x.IsComputeColor))
            {
                if (o.ChannelName == computeColor.Name)
                {
                    _channelImages.Remove(o);
                    CurrentChannelImage = ChannelImages.FirstOrDefault();
                    break;
                }
            }
        }


        public void UpdateBrightnessContrast(double brightness, double contrast)
        {
            CurrentChannelImage.Brightness = brightness;
            CurrentChannelImage.Contrast = contrast;
            UpdateBitmap(CurrentChannelImage);
            UpdateThumbnail(CurrentChannelImage);
        }

        private void OnListViewEdit(object selectItem)
        {
            var channelImage = selectItem as ChannelImage;
            if (channelImage == null) return;
            var args = new OperateChannelArgs() { ChannelName = channelImage.ChannelName, IsComputeColor = channelImage.IsComputeColor, Operator = 0 };
            _operateChannelEvent.Publish(args);
        }
        private void OnListViewDelete(object selectItem)
        {
            var channelImage = selectItem as ChannelImage;
            if (channelImage == null) return;
            var args = new OperateChannelArgs() { ChannelName = channelImage.ChannelName, IsComputeColor = channelImage.IsComputeColor, Operator = 1 };
            _operateChannelEvent.Publish(args);
        }


    }
}
