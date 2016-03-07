using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ThorCyte.ImageViewerModule.Events;
using ThorCyte.ImageViewerModule.Model;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ViewportViewModel : BindableBase
    {
        public bool IsListViewCollapsed
        {
            get { return _isListViewCollapsed; }
            set
            {
                SetProperty<bool>(ref _isListViewCollapsed, value, "IsListViewCollapsed");
            }
        }
        public bool IsAspectRatio
        {
            get { return _isAspectRatio; }
        }
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
        public ObservableCollection<ChannelImage> ChannelImages
        {
            get { return _channelImages; }
            set { SetProperty<ObservableCollection<ChannelImage>>(ref _channelImages, value, "ChannelImages"); }
        }
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
                        CurrentChannelImage.UpdateBitmap();
                        _updateCurrentChannelEvent.Publish(CurrentChannelImage);
                        OnPropertyChanged("CurrentChannelImage");
                    }
                    else if (CurrentChannelImage.ImageData == null)
                    {
                        IsLoading = true;
                        Task.Run(() =>
                        {
                            CurrentChannelImage.GetData(VisualRect, DataScale);
                            CurrentChannelImage.UpdateBitmap();
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
        public Size ImageSize
        {
            get { return _imageSize; }
            set { SetProperty<Size>(ref _imageSize, value, "ImageSize"); }
        }
        public bool IsShow { get; private set; }
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty<bool>(ref _isLoading, value, "IsLoading"); }
        }
        public bool IsActive { get; private set; }
        public DrawTools.DrawingCanvas DrawingCanvas { get; set; }
        public Dictionary<int, Dictionary<Channel, ImageData>> ThumbnailDic { get; set; }
        public int RegionId { get; set; }
        public int TileId { get; set; }
        public Int32Rect VisualRect { get; set; }
        public ICommand ListViewDeleteCommand { get; private set; }
        public ICommand ListViewEditCommand { get; private set; }
        public ICommand ExpandListCommand { get; private set; }
        public delegate Task FixToLastScaleHandler();
        public event FixToLastScaleHandler FixToLastScaleEvent;
        private UpdateCurrentChannelEvent _updateCurrentChannelEvent;
        private UpdateMousePointEvent _updateMousePointEvent;
        private OperateChannelEvent _operateChannelEvent;
        private double CanvasActualWidth;
        private double CanvasActualHeight;
        private double lastCanvasActualWidth;
        private double lastCanvasActualHeight;
        private double VisualScale;
        public double DataScale;
        private double ActualScale;
        private const int ThumbnailSize = 80;
        private IData _iData;
        private int _scanId;
        private int _maxBits;
        private ScanInfo _scanInfo;
        private bool _isListViewCollapsed = true;
        private bool _isAspectRatio;
        private double _aspectRatio;
        private ObservableCollection<ChannelImage> _channelImages;
        private ChannelImage _currentChannelImage;
        private Size _imageSize;
        private bool _isTile { get { return TileId > 0; } }
        private FrameIndex _frameIndex = new FrameIndex() { StreamId = 1, TimeId = 1, ThirdStepId = 1 };
        private ConcurrentQueue<Action> SerialQueue;
        private Thread _queueThread;
        private bool _isStopQueue = false;
        private AutoResetEvent _autoResetEve = new AutoResetEvent(false);
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
            SerialQueue = new ConcurrentQueue<Action>();
            _queueThread = new Thread(QueueExecution);
            _queueThread.Start();
            Application.Current.Exit += Current_Exit;
        }
        public async Task SetAspectRatio(bool isAspectRatio)
        {
            _isAspectRatio = isAspectRatio;
            DrawingCanvas.SetZoomPoint(0.5, 0.5);
            await DrawingCanvas.SetActualScale(VisualScale, VisualScale * AspectRatio, DataScale);
        }
        public async Task OnZoom(int delta)
        {
            if (!IsActive) return;
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
        }
        public async Task FixToViewport()
        {
            ActualScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
            await CalcZoom();
        }
        public async Task UpdateDisplayImage(Rect canvasRect)
        {
            if (_isTile) return;
            if (CurrentChannelImage == null) return;
            IsLoading = true;
            var width = (int)Math.Min((canvasRect.Width + 500), ImageSize.Width);
            var height = (int)Math.Min((canvasRect.Height + 500), ImageSize.Height);
            int x, y;
            x = (int)Math.Max(canvasRect.X - 250, 0);
            y = (int)Math.Max(canvasRect.Y - 250, 0);
            width = Math.Min((int)ImageSize.Width - x, width);
            height = Math.Min((int)ImageSize.Height - y, height);
            VisualRect = new Int32Rect(x, y, width, height);
            DrawingCanvas.SetImageRect(VisualRect);
            if (CurrentChannelImage.ImageData != null && CurrentChannelImage.DataRect.Equals(VisualRect) && CurrentChannelImage.DataScale > DataScale)
            {
                CurrentChannelImage.ImageData = CurrentChannelImage.ImageData.Resize((int)(VisualRect.Width * DataScale), (int)(VisualRect.Height * DataScale));
                CurrentChannelImage.UpdateBitmap();
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
                await CurrentChannelImage.GetDataAsync(VisualRect, DataScale);
                CurrentChannelImage.UpdateBitmap();
            }
            IsLoading = false;
        }
        public void UpdateShowStatus(bool isShow)
        {
            if (isShow && !IsShow && IsActive)
                refreshChannel(RegionId, TileId, _frameIndex);
            IsShow = isShow;
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
         public void LoadExperiment(IData iData, ScanInfo scanInfo, ExperimentInfo experimentInfo, int scanId)
        {
            _iData = iData;
            _scanId = scanId;
            _maxBits = experimentInfo.IntensityBits;
            _scanInfo = scanInfo;
            _aspectRatio = _scanInfo.YPixcelSize / _scanInfo.XPixcelSize;
            _isAspectRatio = true;
            var channels = _scanInfo.ChannelList;
            var virtualChannels = _scanInfo.VirtualChannelList;
            var computeColors = _scanInfo.ComputeColorList;
            _channelImages.Clear();
            foreach (var o in channels)
            {
                var channelImage = new ChannelImage(_iData, _scanId,_maxBits, _aspectRatio,this);
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                channelImage.Brightness = o.Brightness;
                channelImage.Contrast = o.Contrast;
                _channelImages.Add(channelImage);
            }
            foreach (var o in virtualChannels)
            {
                var channelImage = new ChannelImage(_iData, _scanId, _maxBits, _aspectRatio, this);
                channelImage.ChannelInfo = o;
                channelImage.ChannelName = o.ChannelName;
                channelImage.Brightness = o.Brightness;
                channelImage.Contrast = o.Contrast;
                _channelImages.Add(channelImage);
            }
            foreach (var o in computeColors)
            {
                var channelImage = new ChannelImage(_iData, _scanId, _maxBits, _aspectRatio, this);
                channelImage.ComputeColorInfo = o;
                channelImage.ChannelName = o.Name;
                channelImage.IsComputeColor = true;
                channelImage.Brightness = o.Brightness;
                channelImage.Contrast = o.Contrast;
                channelImage.ComputeColorDic = o.ComputeColorDictionary;
                _channelImages.Add(channelImage);
            }
            OnPropertyChanged("ChannelImages");
            CurrentChannelImage = _channelImages[0];

        }
        public void Initialization(int regionId, int tileId)
        {
            if (tileId * TileId <= 0)  DrawingCanvas.DeleteAll(); 
            RegionId = regionId;
            TileId = tileId;
            if (_isTile)
            {
                var width = _scanInfo.TileWidth;
                var height = _scanInfo.TiledHeight;
                ImageSize = new Size(width, height);
                VisualRect = new Int32Rect(0, 0, width, height);
                DrawingCanvas.SetImageRect(VisualRect);
                DataScale = 1;
                VisualScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
                DrawingCanvas.SetActualScaleOnly(VisualScale, VisualScale * AspectRatio, DataScale);
                ActualScale = DataScale * VisualScale;
            }
            else
            {
                var width = (int)Math.Round(_scanInfo.ScanRegionList[regionId].Bound.Width / _scanInfo.XPixcelSize);
                var height = (int)Math.Round(_scanInfo.ScanRegionList[regionId].Bound.Height / _scanInfo.YPixcelSize);
                ImageSize = new Size(width, height);
                VisualRect = new Int32Rect(0, 0, width, height);
                DrawingCanvas.SetImageRect(VisualRect);
                DataScale = Math.Min(((double)CanvasActualWidth / (double)ImageSize.Width), ((double)CanvasActualHeight / (double)ImageSize.Height / AspectRatio));
                VisualScale = 1;
                DrawingCanvas.SetActualScaleOnly(VisualScale, VisualScale * AspectRatio, DataScale);
                ActualScale = DataScale * VisualScale;
            }
            IsActive = true;
        }
        public void Clear()
        {
            IsActive = false;
            _channelImages.Clear();
            DrawingCanvas.DeleteAll();
        }
        public void DisplayImage(int regionId,int tileId)
        {
            RegionId = regionId;
            TileId = tileId;
            refreshChannel(RegionId, TileId, _frameIndex);
        }
        public void DisplayImage(FrameIndex frameIndex)
        {
            _frameIndex = frameIndex;
            refreshChannel(RegionId, TileId, _frameIndex);
        }
        public async Task AddVirtualChannel(VirtualChannel virtrualChannel)
        {
            var channelImage = new ChannelImage(_iData, _scanId, _maxBits, _aspectRatio, this);
            channelImage.ChannelInfo = virtrualChannel;
            channelImage.ChannelName = virtrualChannel.ChannelName;
            if (!_isListViewCollapsed)
            {
                await channelImage.GetThumbnailDataAsync();
                channelImage.UpdateThumbnail();
            }
            else
            {
                channelImage.ThumbnailImageData =null;
                channelImage.Thumbnail = null;
            }
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
            if(IsActive&&IsShow)
            CurrentChannelImage = channelImage;
        }
        public async Task EditVirtualChannel(VirtualChannel virtrualChannel)
        {
            foreach (var o in _channelImages.Where(x => !x.IsComputeColor))
            {
                if (o.ChannelInfo == virtrualChannel)
                {
                    if (!_isListViewCollapsed)
                    {
                        await o.GetThumbnailDataAsync();
                        o.UpdateThumbnail();
                    }
                    else
                    {
                        o.ThumbnailImageData = null;
                        o.Thumbnail = null;
                    }
                    if (o.ImageData != null)
                    {
                        o.ImageData.Dispose();
                        o.ImageData = null;
                    }
                }
                if (IsActive && IsShow)
                    CurrentChannelImage = o;
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
        public async Task AddComputeColor(ComputeColor computeColor)
        {
            var channelImage = new ChannelImage(_iData, _scanId, _maxBits, _aspectRatio, this);
            channelImage.ComputeColorInfo = computeColor;
            channelImage.ChannelName = computeColor.Name;
            channelImage.IsComputeColor = true;
            channelImage.ComputeColorDic = computeColor.ComputeColorDictionary;
            if (!_isListViewCollapsed)
            {
                await channelImage.GetThumbnailDataAsync();
                channelImage.UpdateThumbnail();
            }
            else
            {
                channelImage.ThumbnailImageData = null;
                channelImage.Thumbnail = null;
            }
            _channelImages.Add(channelImage);
            if(IsActive&&IsShow)
            CurrentChannelImage = channelImage;
        }
        public async Task EditComputeColor(ComputeColor computeColor)
        {
            foreach (var o in _channelImages.Where(x => x.IsComputeColor))
            {
                if (o.ChannelName == computeColor.Name)
                {
                    o.ComputeColorDic = computeColor.ComputeColorDictionary;
                    if (!_isListViewCollapsed)
                    {
                         await o.GetThumbnailDataAsync();
                        o.UpdateThumbnail();
                    }
                    else
                    {
                        o.ThumbnailImageData = null;
                        o.Thumbnail = null;
                    }
                    if (o.ImageData != null)
                    {
                        o.ImageData.Dispose();
                        o.ImageData = null;
                    }
                    if (IsActive && IsShow)
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
            CurrentChannelImage.UpdateBitmap();
            CurrentChannelImage.UpdateThumbnail();
        }
        public void UpdateChannelBrightnessContrast(Channel channel, int brightness, double contrast)
        {
            var c = _channelImages.Where(x => x.ChannelInfo == channel).FirstOrDefault();
            c.Brightness = brightness;
            c.Contrast = contrast;
            if (!IsShow || !IsActive) return;
            if (c != _currentChannelImage) return;
            c.UpdateBitmap();
            c.UpdateThumbnail();
        }
        public void UpdateChannelBrightnessContrast(ComputeColor computeColor, int brightness, double contrast)
        {
            var c = _channelImages.Where(x => x.ComputeColorInfo == computeColor).FirstOrDefault();
            c.Brightness = brightness;
            c.Contrast = contrast;
            if (!IsShow || !IsActive) return;
            if (c != _currentChannelImage) return;
            c.UpdateBitmap();
            c.UpdateThumbnail();
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
        private async void OnExpandList()
        {
            IsListViewCollapsed = !_isListViewCollapsed;
            if (!_isListViewCollapsed&&IsActive)
            {
                foreach (var o in _channelImages)
                {
                    if (o.ThumbnailImageData == null)
                    {
                        await o.GetThumbnailDataAsync();
                        o.UpdateThumbnail();
                    }
                }
            }
        }
        private void QueueExecution()
        {
            Action exc = null;
            while (!_isStopQueue)
            {
                IsLoading = false;
                _autoResetEve.WaitOne();
                IsLoading = true;
                while (!SerialQueue.IsEmpty)
                {
                    if (!SerialQueue.TryDequeue(out exc))
                        continue;
                    exc.Invoke();
                }
            }
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
        private void Current_Exit(object sender, ExitEventArgs e)
        {
            _isStopQueue = true;
            _autoResetEve.Set();
            _autoResetEve.Dispose();
        }
        private void refreshChannel(int regionId, int tileId, FrameIndex frameIndex)
        {
            var action = new Action(() =>
            {
                foreach (var o in _channelImages)
                {
                    o.SetRegionTileFrame(regionId, tileId, frameIndex);
                }
                if (!_isListViewCollapsed)
                {
                    foreach (var o in _channelImages)
                    {
                        o.GetThumbnailData();
                        o.UpdateThumbnail();
                    }
                }
                else
                {
                    foreach (var o in _channelImages)
                    {
                        o.ThumbnailImageData = null;
                        o.Thumbnail = null;
                    }
                }
                CurrentChannelImage.GetData(VisualRect, DataScale);
                CurrentChannelImage.UpdateBitmap();
                OnPropertyChanged("CurrentChannelImage");
            });
            Action e = null;
            while (!SerialQueue.IsEmpty)
                SerialQueue.TryDequeue(out e);
            SerialQueue.Enqueue(action);
            _autoResetEve.Set();
        }
    }

}
