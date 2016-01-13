using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.Events;
using ThorCyte.ImageViewerModule.Model;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ViewportViewModel : BindableBase
    {
        private double _aspectRatio = 1;
        public double AspectRatio
        {
            get { return _aspectRatio; }
            set {
                SetProperty<double>(ref _aspectRatio, value, "AspectRatio");
                Scale = new Tuple<double, double, double>(VisualScale, VisualScale * _aspectRatio, DataScale);
            }
        }
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
                _currentChannelImage = value;
                if (_currentChannelImage != null)
                {
                    if (CurrentChannelImage.ImageData != null) CurrentChannelImage.ImageData.Dispose();
                    CurrentChannelImage.ImageData = getData(CurrentChannelImage, VisualRect, DataScale);
                    UpdateBitmap(CurrentChannelImage);
                    _updateCurrentChannelEvent.Publish(CurrentChannelImage);
                }
                OnPropertyChanged("CurrentChannelImage");
            }
        }
        private Tuple<double, double, double> _scale = new Tuple<double, double, double>(1, 1, 1);
        public Tuple<double, double, double> Scale
        {
            get { return _scale; }
            set { SetProperty<Tuple<double, double, double>>(ref _scale, value, "Scale"); }
        }
        private Size _imageSize;
        public Size ImageSize
        {
            get { return _imageSize; }
            set { SetProperty<Size>(ref _imageSize, value, "ImageSize"); }

        }
        private bool _isShown;
        public bool IsShown
        {
            get { return _isShown; }
            set
            {
                if (value == _isShown) return;
                _isShown = value;
                if(_isShown&&IsActive)
                RefreshChannel();
            }
        }
        public bool IsActive { get;private set; }
        public DrawTools.DrawingCanvas DrawingCanvas { get; set; }
        private double CanvasActualWidth;
        private double CanvasActualHeight;
        UpdateCurrentChannelEvent _updateCurrentChannelEvent;
        UpdateMousePointEvent _updateMousePointEvent;
        OperateChannelEvent _operateChannelEvent;
        public ICommand ListViewDeleteCommand { get; private set; }
        public ICommand ListViewEditCommand { get; private set; }
        private Int32Rect VisualRect;
        private double VisualScale;
        private double DataScale;
        private double ActualScale;
        private const int ThumbnailSize = 80;
        private IData _iData;
        private int _scanId;
        private int _regionId;
        private int _tileId;
        private int _maxBits;
        private ScanInfo _scanInfo;
        private bool _isTile { get { return _tileId > 0; } }
        private FrameIndex _frameIndex=new FrameIndex() { StreamId=1, TimeId=1 , ThirdStepId=1 };
        public void Zoomin()
        {
            ActualScale *= 1.5;
            CalcZoom();
        }
        public void Zoomout()
        {
            ActualScale /= 1.5;
            CalcZoom();
        }
        private void CalcZoom()
        {
            var s = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / _aspectRatio));
            if (ActualScale > 4) ActualScale = 4;
            else if (ActualScale < s) ActualScale = s;
            if (_isTile)
            {
                VisualScale = ActualScale;
                DataScale = 1;
            }
            else
            {
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
            }
            Scale = new Tuple<double, double, double>(VisualScale, VisualScale * _aspectRatio, DataScale);
            var status = new MousePointStatus() { Scale = ActualScale};
            _updateMousePointEvent.Publish(status);
        }
        public void UpdateDisplayImage(Rect canvasRect)
        {
            if (_isTile) return;
            if (CurrentChannelImage == null) return;
            var width = (int)Math.Min((canvasRect.Width + 500 ) , ImageSize.Width);
            var height = (int)Math.Min((canvasRect.Height + 500 ), ImageSize.Height);
            int x, y;
            x = (int)Math.Max(canvasRect.X - 250, 0);
            y = (int)Math.Max(canvasRect.Y - 250, 0);
            VisualRect = new Int32Rect(x, y, width, height);
            if (CurrentChannelImage.ImageData != null) CurrentChannelImage.ImageData.Dispose();
            CurrentChannelImage.ImageData = getData(_currentChannelImage, VisualRect, DataScale);
            UpdateBitmap(CurrentChannelImage);
        }
        public ViewportViewModel()
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _updateCurrentChannelEvent = eventAggregator.GetEvent<UpdateCurrentChannelEvent>();
            _updateMousePointEvent = eventAggregator.GetEvent<UpdateMousePointEvent>();
            _operateChannelEvent = eventAggregator.GetEvent<OperateChannelEvent>();
            ListViewDeleteCommand = new DelegateCommand<object>(OnListViewDelete);
            ListViewEditCommand = new DelegateCommand<object>(OnListViewEdit);
            _channelImages = new ObservableCollection<ChannelImage>();
        }
        public void OnMousePoint(Point point)
        {
            int x = (int)(point.X );
            int y = (int)(point.Y );
            if (x >= ImageSize.Width || x < 0 || y >= ImageSize.Height || y < 0) return;
            var index = (int)(x * DataScale) - (int)(VisualRect.X * DataScale) + ((int)(y * DataScale) - (int)(VisualRect.Y * DataScale)) * (int)(VisualRect.Width * DataScale);
            if (index >= CurrentChannelImage.ImageData.Length||index<0) return;
            var gv = CurrentChannelImage.ImageData[index];
            var status = new MousePointStatus() {Scale=ActualScale, Point = new Point(x*_scanInfo.XPixcelSize, y * _scanInfo.YPixcelSize), GrayValue = gv, IsComputeColor = CurrentChannelImage.IsComputeColor };
            _updateMousePointEvent.Publish(status);

        }
        public void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CanvasActualWidth = (int)e.NewSize.Width;
            CanvasActualHeight = (int)e.NewSize.Height;
        }
        private ImageData getData(ChannelImage channel, Int32Rect rect, double scale)
        {
            ImageData data;
            if (channel.IsComputeColor)
            {
                channel.ComputeColorDicWithData = getComputeDictionary(channel.ComputeColorDic, rect, scale);
                data = getComputeData(channel.ComputeColorDicWithData, rect, scale);

            }
            else if (channel.ChannelInfo.IsvirtualChannel)
            {
                var dic = new Dictionary<Channel, ImageData>();
                TraverseVirtualChannel(dic, channel.ChannelInfo as VirtualChannel, rect, scale);
                data = getVirtualData(channel.ChannelInfo as VirtualChannel, dic);
            }
            else
            {
                data = getActiveData(channel.ChannelInfo, rect, scale);
            }
            return data;
        }
        private ImageData getActiveData(Channel channelInfo, Int32Rect rect, double scale)
        {
            if (_isTile)
                return _iData.GetTileData(_scanId, _regionId, channelInfo.ChannelId, _frameIndex.StreamId, _tileId, _frameIndex.TimeId).Resize((int)(rect.Width * scale), (int)(rect.Height * scale));
            else
                return _iData.GetData(_scanId, _regionId, channelInfo.ChannelId, _frameIndex.TimeId, scale, rect);
        }

        private void TraverseVirtualChannel(Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, getActiveData(channel.FirstChannel, rect, scale));
            }
            if (channel.SecondChannel != null)
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
        private ImageData getVirtualData(VirtualChannel channel, Dictionary<Channel, ImageData> dic )
        {

            ImageData data1 = null, data2 = null;
            if (!channel.FirstChannel.IsvirtualChannel)
            {
                data1 = dic[channel.FirstChannel];
            }
            else
            {
                data1 = getVirtualData(channel.FirstChannel as VirtualChannel, dic);
            }

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert)
            {
                if (!channel.SecondChannel.IsvirtualChannel)
                {
                    data2 = dic[channel.SecondChannel];
                }
                else
                {
                    data2 = getVirtualData(channel.SecondChannel as VirtualChannel, dic);
                }
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert))) return null;
            ImageData result = new ImageData(data1.XSize, data1.YSize);
            switch (channel.Operator)
            {
                case ImageOperator.Add:
                    result = data1.Add(data2,_maxBits);
                    break;
                case ImageOperator.Subtract:
                    result = data1.Sub(data2);
                    break;
                case ImageOperator.Invert:
                    result = data1.Invert(_maxBits);
                    break;
                case ImageOperator.Max:
                    result = data1.Max(data2);
                    break;
                case ImageOperator.Min:
                    result = data1.Min(data2);
                    break;
                case ImageOperator.Multiply:
                    result = data1.MulConstant(channel.Operand,_maxBits);
                    break;
                default:
                    break;
            }
            return result;
        }
        private ImageData getComputeData(Dictionary<Channel, Tuple<ImageData, Color>> dic, Int32Rect rect, double scale)
        {
            var data = new ImageData((uint)(rect.Width * scale), (uint)(rect.Height * scale), false);
            foreach (var o in dic)
            {
                var d = ToComputeColor(o.Value.Item1, o.Value.Item2);
                data = data.Add(d, _maxBits);
                d.Dispose();
            }
            return data;
        }
        private ImageData ToComputeColor(ImageData data, Color color)
        {
            var n = data.Length;
            var computeData = new ImageData(data.XSize, data.YSize, false);
            for (int i = 0; i < n; i++)
            {
                computeData[i * 3] = (ushort)((double)color.B * (double)(data[i]) / 255.0);
                computeData[i * 3 + +1] = (ushort)((double)color.G * (double)(data[i]) / 255.0);
                computeData[i * 3 + 2] = (ushort)((double)color.R * (double)(data[i]) / 255.0);
                //computeData[i * 3 + 3] = 255;
            }
            return computeData;
        }
        private Dictionary<Channel, Tuple<ImageData, Color>> getComputeDictionary(Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var datadic = new Dictionary<Channel, ImageData>();
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    TraverseVirtualChannel(datadic, o.Key as VirtualChannel, rect, scale);
                else
                if (!datadic.ContainsKey(o.Key))
                    datadic.Add(o.Key, getActiveData(o.Key, rect, scale));
            }

            var dic = new Dictionary<Channel, Tuple<ImageData, Color>>();
            foreach (var o in ComputeColorDictionary)
            {
                ImageData data;
                if (!o.Key.IsvirtualChannel)
                {
                    data = datadic[o.Key];
                }
                else
                {
                    data = getVirtualData(o.Key as VirtualChannel, datadic);
                }
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
                result = channelImage.ImageData.SetBrightnessAndContrast(channelImage.Contrast, channelImage.Brightness, _maxBits);
            channelImage.Image =new Tuple<ImageSource, Int32Rect>( result.ToBitmapSource(_maxBits),VisualRect);
        }
        private void UpdateThumbnail(ChannelImage channelImage)
        {
            ImageData result = null;
            if (channelImage.Contrast == 1 && channelImage.Brightness == 0)
                result = channelImage.ThumbnailImageData;
            else
                result = channelImage.ThumbnailImageData.SetBrightnessAndContrast(channelImage.Contrast,channelImage.Brightness,_maxBits);
            channelImage.Thumbnail = result.ToBitmapSource(_maxBits);
        }
        public void Initialization(IData iData, ScanInfo scanInfo,ExperimentInfo experimentInfo, int scanId, int regionId, int tileId)
        {
            if (tileId * _tileId <= 0) DrawingCanvas.DeleteAll();
            _iData = iData;
            _scanId = scanId;
            _regionId = regionId;
            _tileId = tileId;
            _maxBits = experimentInfo.IntensityBits;
            _scanInfo = scanInfo;
            if (_isTile)
            {
                var width = (int)(_scanInfo.TileWidth);
                var height = (int)(_scanInfo.TileWidth);
                ImageSize = new Size(width, height);
                DataScale = 1; 
                VisualScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height));
                Scale = new Tuple<double, double, double>(VisualScale, VisualScale * _aspectRatio, DataScale);
                ActualScale = DataScale* VisualScale;
            }
            else
            {
                var width = (int)(_scanInfo.ScanRegionList[regionId].Bound.Width / _scanInfo.XPixcelSize);
                var height = (int)(_scanInfo.ScanRegionList[regionId].Bound.Height / _scanInfo.YPixcelSize);
                ImageSize = new Size(width, height);
                DataScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height));
                VisualScale = 1;
                Scale = new Tuple<double, double, double>(VisualScale, VisualScale * _aspectRatio, DataScale);
                ActualScale = DataScale * VisualScale;
            }
            RefreshChannel();
            IsActive = true;
        }
        public void Clear()
        {
            IsActive = false;
            _channelImages.Clear();
            DrawingCanvas.DeleteAll();
        }
        public void RefreshChannel()
        {
            var channels = _scanInfo.ChannelList;
            var virtualChannels = _scanInfo.VirtualChannelList;
            var computeColors = _scanInfo.ComputeColorList;
            _channelImages.Clear();
            VisualRect = new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height);
            foreach (var o in channels)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                if (channelImage.ThumbnailImageData != null) channelImage.ThumbnailImageData.Dispose();
                channelImage.ThumbnailImageData = getData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            foreach (var o in virtualChannels)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                if (channelImage.ThumbnailImageData != null) channelImage.ThumbnailImageData.Dispose();
                channelImage.ThumbnailImageData = getData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            foreach (var o in computeColors)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelName = o.Name;
                channelImage.IsComputeColor = true;
                channelImage.ComputeColorDic = o.ComputeColorDictionary;
                if (channelImage.ThumbnailImageData != null) channelImage.ThumbnailImageData.Dispose();
                channelImage.ThumbnailImageData = getData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            OnPropertyChanged("ChannelImages");
            CurrentChannelImage = _channelImages.FirstOrDefault();
        }
        public void FrameDisplay(FrameIndex frameIndex)
        {
            _frameIndex = frameIndex;
            var rect = new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height);
            foreach (var o in _channelImages)
            {
                if (o.ThumbnailImageData != null) o.ThumbnailImageData.Dispose();
                o.ThumbnailImageData= getData(o, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(o);
            }
            OnPropertyChanged("ChannelImages");
            CurrentChannelImage = _channelImages.FirstOrDefault();
        }

        public void AddVirtualChannel(VirtualChannel virtrualChannel)
        {
            var channelImage = new ChannelImage();
            channelImage.ChannelInfo = virtrualChannel;
            channelImage.ChannelName = virtrualChannel.ChannelName;
            if (channelImage.ThumbnailImageData != null) channelImage.ThumbnailImageData.Dispose();
            channelImage.ThumbnailImageData = getData(channelImage, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
            UpdateThumbnail(channelImage);
            int index = 0;
            for (int i = _channelImages.Count - 1; i >= 0; i--)
            {
                if (!_channelImages[i].IsComputeColor)
                { 
                    index = i+1;
                    break;
                }
            }
            _channelImages.Insert(index,channelImage);
            CurrentChannelImage = channelImage;
        }
        public void EditVirtualChannel(VirtualChannel virtrualChannel)
        {
            foreach (var o in _channelImages.Where(x => !x.IsComputeColor))
            {
                if (o.ChannelInfo == virtrualChannel)
                {
                    if (o.ThumbnailImageData != null) o.ThumbnailImageData.Dispose();
                    o.ThumbnailImageData = getData(o, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                    UpdateThumbnail(o);
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
            if (channelImage.ThumbnailImageData != null) channelImage.ThumbnailImageData.Dispose();
            channelImage.ThumbnailImageData = getData(channelImage, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
            UpdateThumbnail(channelImage);
            _channelImages.Add(channelImage);
            CurrentChannelImage = channelImage;
        }
        public void EditComputeColor(ComputeColor computeColor)
        {
            foreach (var o in _channelImages.Where(x => x.IsComputeColor))
            {
                if (o.ChannelName == computeColor.Name)
                {
                    o.ComputeColorDic = computeColor.ComputeColorDictionary;
                    if (o.ThumbnailImageData != null) o.ThumbnailImageData.Dispose();
                    o.ThumbnailImageData = getData(o, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                    UpdateThumbnail(o);
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
        public void UpdateBrightnessContrast(int brightness, double contrast)
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
