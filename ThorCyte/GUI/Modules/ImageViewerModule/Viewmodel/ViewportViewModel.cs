using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _isListViewCollapsed = true;
        public bool IsListViewCollapsed
        {
            get { return _isListViewCollapsed; }
            set
            {
                SetProperty<bool>(ref _isListViewCollapsed, value, "IsListViewCollapsed");
            }
        }
        private bool _isAspectRatio;
        public bool IsAspectRatio
        {
            get { return _isAspectRatio; }
        }
        private double _aspectRatio;
        public double AspectRatio
        {
            get
            {
                if (_isAspectRatio)
                    return _aspectRatio;
                else
                    return 1;
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
                if (IsLoading)
                {
                    OnPropertyChanged("CurrentChannelImage");
                    return;
                }
                _currentChannelImage = value;
                if (_currentChannelImage != null)
                {
                    if (CurrentChannelImage.ImageData != null && CurrentChannelImage.DataRect.Equals(VisualRect) && CurrentChannelImage.DataScale > DataScale)
                    {
                        CurrentChannelImage.ImageData = CurrentChannelImage.ImageData.Resize((int)(VisualRect.Width * DataScale), (int)(VisualRect.Height * DataScale));
                        UpdateBitmap(CurrentChannelImage);
                        _updateCurrentChannelEvent.Publish(CurrentChannelImage);
                        OnPropertyChanged("CurrentChannelImage");
                    }
                    else if (CurrentChannelImage.ImageData == null)
                    {
                        IsLoading = true;
                        var task = getData(CurrentChannelImage, VisualRect, DataScale);
                        task.ContinueWith(t =>
                        {
                            UpdateBitmap(CurrentChannelImage);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsLoading = false;
                                _updateCurrentChannelEvent.Publish(CurrentChannelImage);
                                OnPropertyChanged("CurrentChannelImage");
                            });
                        });
                    }
                    else
                    {
                        _updateCurrentChannelEvent.Publish(CurrentChannelImage);
                        OnPropertyChanged("CurrentChannelImage");
                    }
                }
            }
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
                if (_isShown && IsActive)
                    RefreshChannel();
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty<bool>(ref _isLoading, value, "IsLoading"); }
        }
        public bool IsActive { get; private set; }
        public DrawTools.DrawingCanvas DrawingCanvas { get; set; }
        public Dictionary<int, Dictionary<Channel, ImageData>> ThumbnailDic { get; set; }
        private Dictionary<Channel, ImageData> tileThumbnailDic { get; set; }
        private double CanvasActualWidth;
        private double CanvasActualHeight;
        private double lastCanvasActualWidth;
        private double lastCanvasActualHeight;
        UpdateCurrentChannelEvent _updateCurrentChannelEvent;
        UpdateMousePointEvent _updateMousePointEvent;
        OperateChannelEvent _operateChannelEvent;
        public ICommand ListViewDeleteCommand { get; private set; }
        public ICommand ListViewEditCommand { get; private set; }
        public ICommand ExpandListCommand { get; private set; }
        public Int32Rect VisualRect { get; set; }
        public delegate Task FixToLastScaleHandler();
        public event FixToLastScaleHandler FixToLastScaleEvent;
        private double VisualScale;
        public double DataScale;
        private double ActualScale;
        private const int ThumbnailSize = 80;
        private IData _iData;
        private int _scanId;
        public int RegionId { get; set; }
        public int TileId { get; set; }
        private int _maxBits;
        private ScanInfo _scanInfo;
        private bool _isTile { get { return TileId > 0; } }
        private FrameIndex _frameIndex = new FrameIndex() { StreamId = 1, TimeId = 1, ThirdStepId = 1 };
        public async Task SetAspectRatio(bool isAspectRatio)
        {
            _isAspectRatio = isAspectRatio;
            DrawingCanvas.SetZoomPoint(0.5, 0.5);
            await CalcZoom();
        }
        private async Task CalcZoom()
        {
            var s = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
            if (s > 4) ActualScale = s;
            else if (ActualScale > 4) ActualScale = 4;
            else if (ActualScale < s) ActualScale = s;
            if (_isTile)
            {
                if (ActualScale == VisualScale) return;
                VisualScale = ActualScale;
                DataScale = 1;
            }
            else
            {
                if (ActualScale > 1)
                {
                    if (ActualScale == VisualScale) return;
                    DataScale = 1;
                    VisualScale = ActualScale;
                }
                else
                {
                    if (ActualScale == DataScale) return;
                    DataScale = ActualScale;
                    VisualScale = 1;
                }
            }
            await DrawingCanvas.SetActualScale(VisualScale, VisualScale * AspectRatio, DataScale);
            var status = new MousePointStatus() { Scale = ActualScale };
            _updateMousePointEvent.Publish(status);
        }
        public async Task OnZoom(int delta)
        {
            if (!IsActive) return;
            IsLoading = true;
            if (delta > 0)
            {
                ActualScale *= 1.5;
                await CalcZoom();
            }
            else
            {
                ActualScale /= 1.5;
                await CalcZoom();
            }
            IsLoading = false;
        }
        public async Task FixToViewport()
        {
            IsLoading = true;
            ActualScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
            await CalcZoom();
            IsLoading = false;
        }
        public async Task UpdateDisplayImage(Rect canvasRect)
        {
            if (_isTile) return;
            if (CurrentChannelImage == null) return;
            var width = (int)Math.Min((canvasRect.Width + 500), ImageSize.Width);
            var height = (int)Math.Min((canvasRect.Height + 500), ImageSize.Height);
            int x, y;
            x = (int)Math.Max(canvasRect.X - 250, 0);
            y = (int)Math.Max(canvasRect.Y - 250, 0);
            width = Math.Min((int)ImageSize.Width - x, width);
            height = Math.Min((int)ImageSize.Height - y, height);
            VisualRect = new Int32Rect(x, y, width, height);
            if (CurrentChannelImage.ImageData != null && CurrentChannelImage.DataRect.Equals(VisualRect) && CurrentChannelImage.DataScale > DataScale)
            {
                CurrentChannelImage.ImageData = CurrentChannelImage.ImageData.Resize((int)(VisualRect.Width * DataScale), (int)(VisualRect.Height * DataScale));
                UpdateBitmap(CurrentChannelImage);
            }
            else
            {
                foreach (var o in _channelImages)
                {
                    if (o.ImageData != null)
                    {
                        o.ImageData.Dispose();
                        o.ImageData = null;
                    }
                }
                await getData(CurrentChannelImage, VisualRect, DataScale);
                UpdateBitmap(CurrentChannelImage);
            }
        }
        public ViewportViewModel()
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _updateCurrentChannelEvent = eventAggregator.GetEvent<UpdateCurrentChannelEvent>();
            _updateMousePointEvent = eventAggregator.GetEvent<UpdateMousePointEvent>();
            _operateChannelEvent = eventAggregator.GetEvent<OperateChannelEvent>();
            ListViewDeleteCommand = new DelegateCommand<object>(OnListViewDelete);
            ListViewEditCommand = new DelegateCommand<object>(OnListViewEdit);
            ExpandListCommand = new DelegateCommand(OnExpandList);
            _channelImages = new ObservableCollection<ChannelImage>();
        }
        public void OnMousePoint(Point point)
        {
            int x = (int)(point.X);
            int y = (int)(point.Y);
            if (x >= ImageSize.Width || x < 0 || y >= ImageSize.Height || y < 0) return;
            var index = (int)(x * DataScale) - (int)(VisualRect.X * DataScale) + ((int)(y * DataScale) - (int)(VisualRect.Y * DataScale)) * (int)(VisualRect.Width * DataScale);
            if (CurrentChannelImage == null) return;
            if (CurrentChannelImage.ImageData == null) return;
            if (index >= CurrentChannelImage.ImageData.Length || index < 0) return;
            var gv = CurrentChannelImage.IsComputeColor ? 0 : CurrentChannelImage.ImageData[index];
            var status = new MousePointStatus() { Scale = ActualScale, Point = new Point(x * _scanInfo.XPixcelSize, y * _scanInfo.YPixcelSize), GrayValue = gv, IsComputeColor = CurrentChannelImage.IsComputeColor };
            _updateMousePointEvent.Publish(status);
        }
        public async void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            IsLoading = true;
            lastCanvasActualWidth = CanvasActualWidth;
            lastCanvasActualHeight = CanvasActualHeight;
            CanvasActualWidth = e.NewSize.Width;
            CanvasActualHeight = e.NewSize.Height;
            if (FixToLastScaleEvent != null)
                await FixToLastScaleEvent();
            IsLoading = false;
        }
        public async Task FixToLastScale()
        {
            var s = Math.Min(CanvasActualWidth / lastCanvasActualWidth, CanvasActualHeight / lastCanvasActualHeight);
            ActualScale = s * ActualScale;
            await CalcZoom();
            FixToLastScaleEvent -= FixToLastScale;
        }
        private async Task getData(ChannelImage channel, Int32Rect rect, double scale)
        {
            if (channel.ImageData != null) channel.ImageData.Dispose();
            if (channel.IsComputeColor)
            {
                channel.ComputeColorDicWithData = await getComputeDictionary(channel.ComputeColorDic, rect, scale);
                channel.ImageData = await getComputeData(channel.ComputeColorDicWithData, rect, scale);

            }
            else if (channel.ChannelInfo.IsvirtualChannel)
            {
                var dic = new Dictionary<Channel, ImageData>();
                await TraverseVirtualChannel(dic, channel.ChannelInfo as VirtualChannel, rect, scale);
                channel.ImageData = await getVirtualData(channel.ChannelInfo as VirtualChannel, dic);
            }
            else
            {
                channel.ImageData = await getActiveData(channel.ChannelInfo, rect, scale);
            }
            channel.DataScale = scale;
            channel.DataRect = rect;
        }
        public Task<ImageData> getActiveData(Channel channelInfo, Int32Rect rect, double scale)
        {
            return Task.Run<ImageData>(() =>
            {
                if (_isTile)
                    return _iData.GetTileData(_scanId, RegionId, channelInfo.ChannelId, _frameIndex.StreamId, TileId, _frameIndex.TimeId).Resize((int)(rect.Width * scale), (int)(rect.Height * scale));
                else
                    return _iData.GetData(_scanId, RegionId, channelInfo.ChannelId, _frameIndex.TimeId, scale, rect);
            });
        }
        private async Task TraverseVirtualChannel(Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                await TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, await getActiveData(channel.FirstChannel, rect, scale));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    await TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, await getActiveData(channel.SecondChannel, rect, scale));
                }
            }
        }
        private async Task<ImageData> getVirtualData(VirtualChannel channel, Dictionary<Channel, ImageData> dic)
        {
            ImageData data1 = null, data2 = null;
            if (!channel.FirstChannel.IsvirtualChannel)
                data1 = dic[channel.FirstChannel];
            else
                data1 = await getVirtualData(channel.FirstChannel as VirtualChannel, dic);

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert && channel.Operator != ImageOperator.ShiftPeak)
            {
                if (!channel.SecondChannel.IsvirtualChannel)
                    data2 = dic[channel.SecondChannel];
                else
                    data2 = await getVirtualData(channel.SecondChannel as VirtualChannel, dic);
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert && channel.Operator != ImageOperator.ShiftPeak))) return null;
            ImageData result = new ImageData(data1.XSize, data1.YSize);
            switch (channel.Operator)
            {
                case ImageOperator.Add:
                    result = data1.Add(data2, _maxBits);
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
                    result = data1.MulConstant(channel.Operand, _maxBits);
                    break;
                case ImageOperator.ShiftPeak:
                    result = data1.ShiftPeak((ushort)channel.Operand, _maxBits);
                    break;
                default:
                    break;
            }
            return result;
        }
        private async Task<ImageData> getComputeData(Dictionary<Channel, Tuple<ImageData, Color>> dic, Int32Rect rect, double scale)
        {
            var data = new ImageData((uint)(rect.Width * scale), (uint)(rect.Height * scale), false);
            foreach (var o in dic)
            {
                var d = await ToComputeColor(o.Value.Item1, o.Value.Item2);
                data = await AddComputeColor(data, d, _maxBits);
                d.Dispose();
            }
            return data;
        }
        private Task<ImageData> ToComputeColor(ImageData data, Color color)
        {
            return Task.Run<ImageData>(() =>
            {
                var n = data.Length;
                var computeData = new ImageData(data.XSize, data.YSize, false);
                for (int i = 0; i < n; i++)
                {
                    computeData[i * 3] = (ushort)((double)color.B * (double)(data[i]) / 255.0);
                    computeData[i * 3 + +1] = (ushort)((double)color.G * (double)(data[i]) / 255.0);
                    computeData[i * 3 + 2] = (ushort)((double)color.R * (double)(data[i]) / 255.0);
                }
                return computeData;
            });
        }
        private Task<ImageData> AddComputeColor(ImageData data, ImageData d, int maxBits)
        {
            return Task.Run<ImageData>(() =>
            {
                return data.Add(d, _maxBits);
            });
        }
        private async Task<Dictionary<Channel, Tuple<ImageData, Color>>> getComputeDictionary(Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var datadic = new Dictionary<Channel, ImageData>();
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    await TraverseVirtualChannel(datadic, o.Key as VirtualChannel, rect, scale);
                else
                if (!datadic.ContainsKey(o.Key))
                    datadic.Add(o.Key, await getActiveData(o.Key, rect, scale));
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
                    data = await getVirtualData(o.Key as VirtualChannel, datadic);
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
            if (result != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var image = result.ToBitmapSource(_maxBits);
                    channelImage.Image = new Tuple<ImageSource, Int32Rect>(image, VisualRect);
                });
            }
        }
        private void UpdateThumbnail(ChannelImage channelImage)
        {
            ImageData result = null;
            if (channelImage.Contrast == 1 && channelImage.Brightness == 0)
                result = channelImage.ThumbnailImageData;
            else
                result = channelImage.ThumbnailImageData.SetBrightnessAndContrast(channelImage.Contrast, channelImage.Brightness, _maxBits);
            if (result != null)
                channelImage.Thumbnail = result.ToBitmapSource(_aspectRatio, _maxBits);
        }
        public async Task Initialization(IData iData, ScanInfo scanInfo, ExperimentInfo experimentInfo, int scanId, int regionId, int tileId)
        {
            if (tileId * TileId <= 0) Application.Current.Dispatcher.Invoke(() => { DrawingCanvas.DeleteAll(); });
            _iData = iData;
            _scanId = scanId;
            RegionId = regionId;
            TileId = tileId;
            _maxBits = experimentInfo.IntensityBits;
            _scanInfo = scanInfo;
            _aspectRatio = _scanInfo.YPixcelSize / _scanInfo.XPixcelSize;
            _isAspectRatio = true;
            if (_isTile)
            {
                var width = _scanInfo.TileWidth;
                var height = _scanInfo.TiledHeight;
                ImageSize = new Size(width, height);
                DataScale = 1;
                VisualScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
                await DrawingCanvas.SetActualScale(VisualScale, VisualScale * AspectRatio, DataScale);
                ActualScale = DataScale * VisualScale;
                tileThumbnailDic = new Dictionary<Channel, ImageData>();
            }
            else
            {
                var width = (int)Math.Round(_scanInfo.ScanRegionList[regionId].Bound.Width / _scanInfo.XPixcelSize);
                var height = (int)Math.Round(_scanInfo.ScanRegionList[regionId].Bound.Height / _scanInfo.YPixcelSize);
                ImageSize = new Size(width, height);
                DataScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
                VisualScale = 1;
                await DrawingCanvas.SetActualScale(VisualScale, VisualScale * AspectRatio, DataScale);
                ActualScale = DataScale * VisualScale;
            }
            await RefreshChannel();
            IsActive = true;
        }
        public void Clear()
        {
            IsActive = false;
            _channelImages.Clear();
            DrawingCanvas.DeleteAll();
        }
        public async Task RefreshChannel()
        {
            IsLoading = true;
            int currentChannelIndex = 0;
            if (_currentChannelImage != null && _channelImages.Count > 0)
                currentChannelIndex = _channelImages.IndexOf(CurrentChannelImage);
            var channels = _scanInfo.ChannelList;
            var virtualChannels = _scanInfo.VirtualChannelList;
            var computeColors = _scanInfo.ComputeColorList;
            Application.Current.Dispatcher.Invoke(() => { _channelImages.Clear(); });
            VisualRect = new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height);
            var scale = Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height);
            foreach (var o in channels)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                channelImage.Brightness = o.Brightness;
                channelImage.Contrast = o.Contrast;
                channelImage.ThumbnailImageData = await getThumbnailData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            foreach (var o in virtualChannels)
            {
                var channelImage = new ChannelImage();
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                channelImage.Brightness = o.Brightness;
                channelImage.Contrast = o.Contrast;
                channelImage.ThumbnailImageData = await getThumbnailData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            foreach (var o in computeColors)
            {
                var channelImage = new ChannelImage();
                channelImage.ComputeColorInfo = o;
                channelImage.ChannelName = o.Name;
                channelImage.IsComputeColor = true;
                channelImage.Brightness = o.Brightness;
                channelImage.Contrast = o.Contrast;
                channelImage.ComputeColorDic = o.ComputeColorDictionary;
                channelImage.ThumbnailImageData = await getThumbnailData(channelImage, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(channelImage);
                _channelImages.Add(channelImage);
            }
            OnPropertyChanged("ChannelImages");
            IsLoading = false;
            CurrentChannelImage = _channelImages[currentChannelIndex];
        }
        public async Task FrameDisplay(FrameIndex frameIndex)
        {
            _frameIndex = frameIndex;
            foreach (var o in _channelImages)
            {
                o.ThumbnailImageData = await getThumbnailData(o, VisualRect, Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                UpdateThumbnail(o);
            }
            CurrentChannelImage = _currentChannelImage;
        }
        public async Task AddVirtualChannel(VirtualChannel virtrualChannel)
        {
            var channelImage = new ChannelImage();
            channelImage.ChannelInfo = virtrualChannel;
            channelImage.ChannelName = virtrualChannel.ChannelName;
            channelImage.ThumbnailImageData = await getThumbnailData(channelImage, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
            UpdateThumbnail(channelImage);
            int index = 0;
            for (int i = _channelImages.Count - 1; i >= 0; i--)
            {
                if (!_channelImages[i].IsComputeColor)
                {
                    index = i + 1;
                    break;
                }
            }
            _channelImages.Insert(index, channelImage);
            CurrentChannelImage = channelImage;
        }
        public async Task EditVirtualChannel(VirtualChannel virtrualChannel)
        {
            foreach (var o in _channelImages.Where(x => !x.IsComputeColor))
            {
                if (o.ChannelInfo == virtrualChannel)
                {
                    foreach (var x in findRelativeChannel(o.ChannelInfo as VirtualChannel))
                    {
                        x.ThumbnailImageData = await getThumbnailData(x, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                        UpdateThumbnail(x);
                        x.ImageData.Dispose();
                        x.ImageData = null;
                    }
                    CurrentChannelImage = o;
                }
            }
        }
        private List<ChannelImage> findRelativeChannel(VirtualChannel changeChannel)
        {
            List<ChannelImage> relativeChannel = new List<ChannelImage>();
            foreach (var o in _channelImages)
            {
                if (o.IsComputeColor)
                {
                    foreach (var cc in o.ComputeColorDic.Keys.ToList())
                    {
                        if (!cc.IsvirtualChannel) break;
                        if (channelcontains(changeChannel, cc as VirtualChannel))
                            relativeChannel.Add(o);
                    }
                }
                else if (o.ChannelInfo.IsvirtualChannel)
                {
                    if (channelcontains(changeChannel, o.ChannelInfo as VirtualChannel))
                        relativeChannel.Add(o);
                }
            }
            return relativeChannel;
        }
        private bool channelcontains(VirtualChannel a, VirtualChannel b)
        {
            if (a == b) return true;
            if (b.FirstChannel == a || b.SecondChannel == a) return true;
            if (b.SecondChannel == null)
            {
                if (b.FirstChannel.IsvirtualChannel) return channelcontains(a, b.FirstChannel as VirtualChannel);
                else return false;
            }
            else
            {
                if (b.FirstChannel.IsvirtualChannel && b.SecondChannel.IsvirtualChannel)
                    return channelcontains(a, b.FirstChannel as VirtualChannel) || channelcontains(a, b.SecondChannel as VirtualChannel);
                else if (b.FirstChannel.IsvirtualChannel)
                    return channelcontains(a, b.FirstChannel as VirtualChannel);
                else if (b.SecondChannel.IsvirtualChannel)
                    return channelcontains(a, b.SecondChannel as VirtualChannel);
                else
                    return false;
            }
        }
        public List<ChannelImage> DeleteVirtualChannel(VirtualChannel virtrualChannel)
        {
            List<ChannelImage> list = new List<ChannelImage>();
            foreach (var o in _channelImages.Where(x => !x.IsComputeColor))
            {
                if (o.ChannelInfo == virtrualChannel)
                {
                    foreach (var x in findRelativeChannel(o.ChannelInfo as VirtualChannel))
                    {
                        _channelImages.Remove(x);
                        list.Add(x);
                    }
                    CurrentChannelImage = ChannelImages.FirstOrDefault();
                    break;
                }
            }
            return list;
        }
        public async Task AddComputeColor(ComputeColor computeColor)
        {
            var channelImage = new ChannelImage();
            channelImage.ComputeColorInfo = computeColor;
            channelImage.ChannelName = computeColor.Name;
            channelImage.IsComputeColor = true;
            channelImage.ComputeColorDic = computeColor.ComputeColorDictionary;
            channelImage.ThumbnailImageData = await getThumbnailData(channelImage, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
            UpdateThumbnail(channelImage);
            _channelImages.Add(channelImage);
            CurrentChannelImage = channelImage;
        }
        public async Task EditComputeColor(ComputeColor computeColor)
        {
            foreach (var o in _channelImages.Where(x => x.IsComputeColor))
            {
                if (o.ChannelName == computeColor.Name)
                {
                    o.ComputeColorDic = computeColor.ComputeColorDictionary;
                    o.ThumbnailImageData = await getThumbnailData(o, new Int32Rect(0, 0, (int)ImageSize.Width, (int)ImageSize.Height), Math.Min(ThumbnailSize / ImageSize.Width, ThumbnailSize / ImageSize.Height));
                    UpdateThumbnail(o);
                    o.ImageData.Dispose();
                    o.ImageData = null;
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
        public void UpdateChannelBrightnessContrast(Channel channel, int brightness, double contrast)
        {
            var c = _channelImages.Where(x => x.ChannelInfo == channel).FirstOrDefault();
            c.Brightness = brightness;
            c.Contrast = contrast;
            if (c != _currentChannelImage) return;
            UpdateBitmap(c);
            UpdateThumbnail(c);
        }
        public void UpdateChannelBrightnessContrast(ComputeColor computeColor, int brightness, double contrast)
        {
            var c = _channelImages.Where(x => x.ComputeColorInfo == computeColor).FirstOrDefault();
            c.Brightness = brightness;
            c.Contrast = contrast;
            if (c != _currentChannelImage) return;
            UpdateBitmap(c);
            UpdateThumbnail(c);
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
        private void OnExpandList()
        {
            IsListViewCollapsed = !_isListViewCollapsed;
        }

        private async Task<ImageData> getThumbnailData(ChannelImage channel, Int32Rect rect, double scale)
        {
            ImageData tData;
            if (channel.IsComputeColor)
            {
                if (!ThumbnailDic.ContainsKey(RegionId) && !_isTile) return null;
                tData = await getThumbnailComputeData(_isTile ? tileThumbnailDic : ThumbnailDic[RegionId], channel.ComputeColorDic, rect, scale);
            }
            else if (channel.ChannelInfo.IsvirtualChannel)
            {
                if (!ThumbnailDic.ContainsKey(RegionId) && !_isTile) return null;
                await TraverseThumbnailVirtualChannel(_isTile ? tileThumbnailDic : ThumbnailDic[RegionId], channel.ChannelInfo as VirtualChannel, rect, scale);
                tData = await getVirtualData(channel.ChannelInfo as VirtualChannel, _isTile ? tileThumbnailDic : ThumbnailDic[RegionId]);
            }
            else
            {
                tData = await getThumbnailActiveData(channel.ChannelInfo, rect, scale);
            }
            return tData;
        }
        private async Task TraverseThumbnailVirtualChannel(Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                await TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, await getThumbnailActiveData(channel.FirstChannel, rect, scale));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    await TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, await getThumbnailActiveData(channel.SecondChannel, rect, scale));
                }
            }
        }
        private async Task<ImageData> getThumbnailComputeData(Dictionary<Channel, ImageData> dic, Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var tData = new ImageData((uint)(rect.Width * scale), (uint)(rect.Height * scale), false);
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    await TraverseThumbnailVirtualChannel(dic, o.Key as VirtualChannel, rect, scale);
                else
                if (!dic.ContainsKey(o.Key))
                    dic.Add(o.Key, await getActiveData(o.Key, rect, scale));
            }
            foreach (var o in ComputeColorDictionary)
            {
                ImageData data;
                if (!o.Key.IsvirtualChannel)
                    data = dic[o.Key];
                else
                    data = await getVirtualData(o.Key as VirtualChannel, dic);
                var d = await ToComputeColor(data, o.Value);
                tData = await AddComputeColor(tData, d, _maxBits);
                d.Dispose();
            }
            return tData;
        }
        public async Task<ImageData> getThumbnailActiveData(Channel channelInfo, Int32Rect rect, double scale)
        {
            ImageData tData;
            if (_isTile)
                return await getActiveData(channelInfo, VisualRect, scale);
            if (ThumbnailDic.ContainsKey(RegionId))
            {
                if (ThumbnailDic[RegionId].ContainsKey(channelInfo))
                {
                    tData = ThumbnailDic[RegionId][channelInfo];
                }
                else
                {
                    tData = await getActiveData(channelInfo, VisualRect, scale);
                    addDicValue(ThumbnailDic[RegionId], channelInfo, tData);
                }
            }
            else
            {
                tData = await getActiveData(channelInfo, VisualRect, scale);
                var dic = new Dictionary<Channel, ImageData>();
                addDicValue(dic, channelInfo, tData);
                addDicValue(ThumbnailDic, RegionId, dic);
            }
            return tData;
        } 
        private void addDicValue<TKey,TValue>(Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            lock(dic)
            {
                if (!dic.ContainsKey(key))
                    dic.Add(key, value);
            }
        }
    }
}
