using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools;
using ThorCyte.ImageViewerModule.Events;
using ThorCyte.ImageViewerModule.Model;
using ThorCyte.ImageViewerModule.Selector;
using ThorCyte.ImageViewerModule.View;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.IO;
namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ImageViewerViewModel : BindableBase
    {
        private ViewportDisplayType _viewportType;
        public ViewportDisplayType ViewportType
        {
            get { return _viewportType; }
            set
            {
                SetProperty<ViewportDisplayType>(ref _viewportType, value, "ViewportType");
            }
        }
        private bool _imageViewEnable;
        public bool ImageViewEnable
        {
            get { return _imageViewEnable; }
            set
            {
                SetProperty<bool>(ref _imageViewEnable, value, "ImageViewEnable");
            }
        }
        private bool _isAspectRatio;
        public bool IsAspectRatio
        {
            get { return _isAspectRatio; }
            set
            {
                SetProperty<bool>(ref _isAspectRatio, value, "IsAspectRatio");
            }
        }
        private bool _isRuler;
        public bool IsRuler
        {
            get { return _isRuler; }
            set
            {
                SetProperty<bool>(ref _isRuler, value, "IsRuler");
            }
        }
        private bool _isDragger;
        public bool IsDragger
        {
            get { return _isDragger; }
            set { SetProperty<bool>(ref _isDragger, value, "IsDragger"); }
        }
        private bool _isProfile;
        public bool IsProfile
        {
            get { return _isProfile; }
            set
            {
                SetProperty<bool>(ref _isProfile, value, "IsProfile");
                if(!_isProfile)
                {
                    if (profileWindow != null)
                        profileWindow.Close();
                }
            }
        }


        private List<ViewportView> _viewports;
        public List<ViewportView> Viewports
        {
            get { return _viewports; }
            set { SetProperty<List<ViewportView>>(ref _viewports, value, "Viewports"); }
        }
        private ViewportView _currentViewport;
        public ViewportView CurrentViewport
        {
            get { return _currentViewport; }
            set { SetProperty<ViewportView>(ref _currentViewport, value, "CurrentViewport"); }
        }
        private int _maxBrightness;
        public int MaxBrightness
        {
            get { return _maxBrightness; }
            set { SetProperty<int>(ref _maxBrightness, value, "MaxBrightness"); }
        }
        private int _minBrightness;
        public int MinBrightness
        {
            get { return _minBrightness; }
            set { SetProperty<int>(ref _minBrightness, value, "MinBrightness"); }
        }
        private int _brightness=0;
        public int Brightness
        {
            get { return _brightness; }
            set { SetProperty<int>(ref _brightness, value, "Brightness"); updateBrightnessContrast(); }
        }
        private double _contrast=1;
        public double Contrast
        {
            get { return _contrast; }
            set
            {
                SetProperty<double>(ref _contrast, value, "Contrast");
                OnPropertyChanged("SliderContrast");        
                updateBrightnessContrast();
            }
        }
        public double SliderContrast
        {
            get { return getSliderContrast(_contrast); }
            set
            {
                double c = value / 100d; // jcl-6471
                if (c >= 0)     // (0 : 30) to (1 : 31)
                    c = (c + 1d);
                else    // (-1 : -10) to (0.9 to 0.0) decrementing 0.1
                {
                    c = Math.Abs(c);
                    c = (10d - c) / 10d;
                }
                Contrast = c;
            }
        }
        private string _mousePointStatus;
        public string MousePointStatus
        {
            get { return _mousePointStatus; }
            set { SetProperty<string>(ref _mousePointStatus, value, "MousePointStatus");  }
        }

        private Dictionary<ViewportView, ViewportViewModel> _viewportDic = new Dictionary<ViewportView, ViewportViewModel>();
        private ProfileViewModel profileViewModel;
        private ProfileWindow profileWindow;
        private BrightnessContrastWindow brightnessContrastWindow;
        private IList<Channel> _channels;
        private IList<VirtualChannel> _virtualChannels;
        private IList<ComputeColor> _computeColors;
        private ScanInfo _scanInfo;
        private ExperimentInfo _experimentInfo;
        private IExperiment _experiment;
        private IData _data;
        private int _scanId;
        private int _regionId;
        private int _tileId;
        public ICommand DraggerCommand { get; private set; }
        public ICommand DrawRegionCommand { get; private set; }
        public ICommand ZoomCommand { get; private set; }
        public ICommand ComputeColorCommand { get; private set; }
        public ICommand VirtualChannelCommand { get; private set; }
        public ICommand ProfileCommand { get; private set; }
        public ICommand RulerCommand { get; private set; }
        public ICommand BrightnessContrastCommand { get; private set; }
        public ICommand AutoGreyCommand { get; set; }
        public ICommand AspectRatioCommand { get; set; }
        public ICommand ResetBrightnessCommand { get; set; }
        public ICommand MaximizeViewportCommand { get; set; }

        
        public ImageViewerViewModel()
        {

            DrawRegionCommand = new DelegateCommand<string>(OnDrawRegion);
            DraggerCommand = new DelegateCommand(OnDraggerChanged);
            ZoomCommand = new DelegateCommand<string>(OnZoom);
            ComputeColorCommand = new DelegateCommand(OnComputeColor);
            VirtualChannelCommand = new DelegateCommand(OnVirtualChannel);
            ProfileCommand = new DelegateCommand(OnProfile);
            RulerCommand = new DelegateCommand(OnRuler);
            BrightnessContrastCommand = new DelegateCommand(OnBrightnessContrast);
            AutoGreyCommand = new DelegateCommand(OnAutoGrey);
            AspectRatioCommand = new DelegateCommand(OnAspectRatio);
            ResetBrightnessCommand = new DelegateCommand(OnResetBrightness);
            MaximizeViewportCommand = new DelegateCommand(OnMaximizeViewport);
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<UpdateMousePointEvent>().Subscribe(OnUpdateMousePoint);
            eventAggregator.GetEvent<OperateChannelEvent>().Subscribe(OnOperateChannel);
            eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(OnLoadExperiment);
            eventAggregator.GetEvent<SelectRegions>().Subscribe(OnSelectRegions);
            eventAggregator.GetEvent<SelectRegionTileEvent>().Subscribe(OnSelectRegionTile);
            eventAggregator.GetEvent<FrameChangedEvent>().Subscribe(OnFrameChanged);
            eventAggregator.GetEvent<UpdateCurrentChannelEvent>().Subscribe(OnUpdateChannel);
            eventAggregator.GetEvent<SaveAnalysisResultEvent>().Subscribe(SaveChannels);
            eventAggregator.GetEvent<UpdateProfilePointsEvent>().Subscribe(OnUpdateProfilePoints);

            _viewports = new List<ViewportView>();
            AddVP();

            profileViewModel = new ProfileViewModel();
            InitializeExperiment();
        }
        public void OnViewportTypeChange(int type)
        {
            if (ViewportType.Type == type) return;
            switch (type)
            {
                case 1:
                    _viewportDic[_viewports[0]].IsShown = true;
                    _viewportDic[_viewports[1]].IsShown = false;
                    _viewportDic[_viewports[2]].IsShown = false;
                    _viewportDic[_viewports[3]].IsShown = false;
                    break;
                case 2:
                    _viewportDic[_viewports[0]].IsShown = true;
                    _viewportDic[_viewports[1]].IsShown = true;
                    _viewportDic[_viewports[2]].IsShown = false;
                    _viewportDic[_viewports[3]].IsShown = false;
                    break;
                case 3:
                    _viewportDic[_viewports[0]].IsShown = true;
                    _viewportDic[_viewports[1]].IsShown = true;
                    _viewportDic[_viewports[2]].IsShown = false;
                    _viewportDic[_viewports[3]].IsShown = false;
                    break;
                case 4:
                    _viewportDic[_viewports[0]].IsShown = true;
                    _viewportDic[_viewports[1]].IsShown = true;
                    _viewportDic[_viewports[2]].IsShown = true;
                    _viewportDic[_viewports[3]].IsShown = true;
                    break;
                default:
                    return;
            }
            ViewportType = new ViewportDisplayType { Type = type, Viewports = _viewports,CurrentViewport= _viewports[0] }; 
        }
        public void OnMaximizeViewport()
        {
            if (ViewportType.Type == 1)
            {
                _viewportDic[_viewports[0]].IsShown = true;
                _viewportDic[_viewports[1]].IsShown = true;
                _viewportDic[_viewports[2]].IsShown = true;
                _viewportDic[_viewports[3]].IsShown = true;
                ViewportType = new ViewportDisplayType { Type = 4, Viewports = _viewports, CurrentViewport = _currentViewport };
            }
            else
            {
                for(int i=0;i<4;i++)
                {
                    if (_viewports[i] != _currentViewport)
                        _viewportDic[_viewports[i]].IsShown = false;
                }
                ViewportType = new ViewportDisplayType { Type = 1, Viewports = _viewports, CurrentViewport = _currentViewport };
            }
        }
        
        private void OnSelectRegionTile(RegionTile regionTile)
        {
            _regionId = regionTile.RegionId;
            _tileId = regionTile.TileId;
            OnDisplay();
        }
        private void OnSelectRegions(List<int> regions)
        {
            if (regions.Count != 1) return;
            _regionId = regions[0];
            _tileId = -1;
            OnDisplay();
        }
        private void OnFrameChanged(FrameIndex frameIndex)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            _viewportDic[CurrentViewport].FrameDisplay(frameIndex);
        }
        private void OnLoadExperiment(int scanId)
        {
            InitializeExperiment();
        }
        private void InitializeExperiment()
        {
            _experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            _data = ServiceLocator.Current.GetInstance<IData>();
            _scanId= _experiment.GetCurrentScanId();
            if (_scanId < 0) return;
            _scanInfo = _experiment.GetScanInfo(_scanId);
            _experimentInfo = _experiment.GetExperimentInfo();
            _channels = _scanInfo.ChannelList;
            _virtualChannels = _scanInfo.VirtualChannelList;
            _computeColors = _scanInfo.ComputeColorList;
            MaxBrightness = (0x01 << _experimentInfo.IntensityBits) - 1;
            MinBrightness = -1 * MaxBrightness;
            IsDragger = false;
            IsRuler = false;
            IsProfile = false;
            IsAspectRatio = true;
            ImageViewEnable = false;
            foreach (var o in _viewports)
            {
                o.viewportBorder.BorderBrush = Brushes.Black;
                o.listView.Visibility = Visibility.Visible;
                o.drawCanvas.Tool = ToolType.Pointer;
                _viewportDic[o].Clear();
            }
            LoadChannels();
            ViewportType = new ViewportDisplayType { Type = 1, Viewports = _viewports, CurrentViewport = _viewports[0] };
            CurrentViewport = Viewports.FirstOrDefault();
            CurrentViewport.viewportBorder.BorderBrush = Brushes.White;
            _viewportDic[CurrentViewport].IsShown = true;
        }
        private void OnDisplay()
        {
            CurrentViewport.listView.Visibility = Visibility.Visible;
            _viewportDic[CurrentViewport].Initialization(_data, _scanInfo,_experimentInfo, _scanId, _regionId, _tileId);
            IsDragger = true;
            IsProfile = false;
            IsRuler = false;
            updateDrawTool();
            CurrentViewport.drawCanvas.SetPixelSize(_scanInfo.XPixcelSize, _scanInfo.YPixcelSize);
            ImageViewEnable = true;
        }
        private void OnDrawRegion(string arg)
        {
            ToolType type;
            if (!Enum.TryParse<ToolType>(arg, out type)) return;
            foreach (var o in Viewports)
            {
                o.drawCanvas.Tool =ToolType.Pointer;
            }
            IsDragger = false;
            IsProfile = false;
            IsRuler = false;
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            CurrentViewport.drawCanvas.Tool = type;
        }
        private void OnZoom(string arg)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            bool isIn;
            if (!bool.TryParse(arg, out isIn)) return;
            if (isIn) _viewportDic[CurrentViewport].Zoomin();
            else _viewportDic[CurrentViewport].Zoomout();
        }
        private void OnComputeColor()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            IsRuler = false;
            IsProfile = false;
            updateDrawTool();
            var window = new SetComputeColorWindow();
            var vm = new SetComputeColorViewModel(_channels, _virtualChannels, _computeColors);
            vm.IsNew = true;
            vm.ChannelName = "color " + (_computeColors.Count + 1).ToString();
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                var c = new ComputeColor() { Name = vm.ChannelName };
                c.ComputeColorDictionary = computeColorList2Dic(vm.ChannelList);
                _computeColors.Add(c);
                foreach (var o in _viewports)
                {
                    if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                        _viewportDic[o].AddComputeColor(c);
                }
            }
        }
        private void OnVirtualChannel()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            IsRuler = false;
            IsProfile = false;
            updateDrawTool();
            var window = new SetVirtualChannelWindow();
            var vm = new SetVirtualChannelViewModel(_channels, _virtualChannels, _computeColors);
            vm.IsNew = true;
            vm.ChannelName = "virtual " + (_virtualChannels.Count + 1).ToString();
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                var virtualChannel = new VirtualChannel() { ChannelId = 0, ChannelName = vm.ChannelName, FirstChannel = vm.Channel1, SecondChannel = vm.Channel2, Operator = vm.Operator, Operand = vm.Operand };
                _virtualChannels.Add(virtualChannel);
                foreach (var o in _viewports)
                {
                    if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                        _viewportDic[o].AddVirtualChannel(virtualChannel);
                }
            }

        }
        private void OnDraggerChanged()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsDragger = false;
                return;
            }
            IsRuler = false;
            IsProfile = false;
            updateDrawTool();
        }
        private void OnProfile()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsProfile = false;
                return;
            }
            IsRuler = false;
            if (IsProfile)
            {
                if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
                profileWindow = new ProfileWindow();
                profileWindow.DataContext = profileViewModel;
                profileWindow.Topmost = true;
                profileWindow.Owner = Application.Current.MainWindow;
                profileViewModel.Initialization(_viewportDic[CurrentViewport].CurrentChannelImage);
                profileWindow.Show();
                profileWindow.Closed += ProfileWindow_Closed;
            }
            updateDrawTool();
        }
        private void ProfileWindow_Closed(object sender, EventArgs e)
        {
            IsProfile = false;
            profileViewModel.OnCloseWindow();
            updateDrawTool();
        }

        private void OnRuler()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsRuler = false;
                return;
            }
            IsProfile = false;
            updateDrawTool();

        }
        private void OnBrightnessContrast()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            IsRuler = false;
            IsProfile = false;
            updateDrawTool();
            brightnessContrastWindow = new BrightnessContrastWindow();
            brightnessContrastWindow.DataContext = this;
            brightnessContrastWindow.Topmost = true;
            brightnessContrastWindow.Owner = Application.Current.MainWindow;
            brightnessContrastWindow.HistogramPanel.SetData(_viewportDic[CurrentViewport].CurrentChannelImage.ThumbnailImageData);
            _brightness = _viewportDic[CurrentViewport].CurrentChannelImage.Brightness;
            _contrast = _viewportDic[CurrentViewport].CurrentChannelImage.Contrast;
            OnPropertyChanged("Brightness");
            OnPropertyChanged("Contrast");
            OnPropertyChanged("SliderContrast");
            brightnessContrastWindow.ShowDialog();
        }
        private void OnUpdateChannel(ChannelImage channel)
        {
            profileViewModel.UpdateChannel(channel);

            if (channel ==null|| brightnessContrastWindow==null) return;
            brightnessContrastWindow.HistogramPanel.SetData(channel.ThumbnailImageData);
            _brightness = channel.Brightness;
            _contrast = channel.Contrast;
            OnPropertyChanged("Brightness");
            OnPropertyChanged("Contrast");
            OnPropertyChanged("SliderContrast");
        }
        private void OnUpdateProfilePoints(ProfilePoints e)
        {
            var rect = _viewportDic[CurrentViewport].VisualRect;
            var scale = _viewportDic[CurrentViewport].Scale.Item3;
            var start = new Point((e.StartPoint.X - rect.X) * scale, (e.StartPoint.Y - rect.Y) * scale);
            var end = new Point((e.EndPoint.X - rect.X) * scale, (e.EndPoint.Y - rect.Y) * scale);
            profileViewModel.UpdateProfilePoints(start, end);            
        }
        private void OnAspectRatio()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsAspectRatio = true;
                return;
            }
            _viewportDic[CurrentViewport].SetAspectRatio(IsAspectRatio);
        }
        private void OnAutoGrey()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            var channel = _viewportDic[CurrentViewport].CurrentChannelImage;
            if (channel == null) return;
            if (channel.IsComputeColor) return;
            var max = channel.ThumbnailImageData.Max();
            var min = channel.ThumbnailImageData.Min();
            _contrast = (0x01 << _experimentInfo.IntensityBits) / (max - min);
            _brightness = 0 - (int)(min * _contrast);
            OnPropertyChanged("Brightness");
            OnPropertyChanged("Contrast");
            OnPropertyChanged("SliderContrast");
            updateBrightnessContrast();
        }
        private void OnResetBrightness()
        {
            _contrast = 1;
            _brightness = 0 ;
            OnPropertyChanged("Brightness");
            OnPropertyChanged("Contrast");
            OnPropertyChanged("SliderContrast");
            updateBrightnessContrast();
        }
        
        private void OnUpdateMousePoint(MousePointStatus status)
        {
            if (status.IsComputeColor)
                MousePointStatus = string.Format("Zoom:{0}% X:{1} Y:{2} Compute Color",(status.Scale*100).ToString("0").PadLeft(10), status.Point.X.ToString("0").PadLeft(10), status.Point.Y.ToString("0").PadLeft(10));
            else
                MousePointStatus = string.Format("Zoom:{0}% X:{1} Y:{2} GL:{3}", (status.Scale * 100).ToString("0").PadLeft(10), status.Point.X.ToString("0").PadLeft(10), status.Point.Y.ToString("0").PadLeft(10), status.GrayValue.ToString().PadLeft(10));

        }
        private void OnOperateChannel(OperateChannelArgs args)
        {
            if(args.IsComputeColor)
            {
                var computeColor = _computeColors.Where(x => x.Name == args.ChannelName).FirstOrDefault();
                if (computeColor == null) return;
                if (args.Operator == 0) EditComputeColor(computeColor);
                else if (args.Operator == 1) DeleteComputeColor(computeColor);

            }
            else
            {
                var channel = _virtualChannels.Where(x => x.ChannelName == args.ChannelName).FirstOrDefault();
                if (channel == null) return;
                if (args.Operator == 0) EditVirtualChannel(channel);
                else if (args.Operator == 1) DeleteVirtualChannel(channel);

            }

        }
        private void Vierport_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vp = sender as ViewportView;
            if (vp == CurrentViewport) return;
            e.Handled = true;
        }
        private void Vierport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vp = sender as ViewportView;
            if (vp == CurrentViewport) return;
            CurrentViewport.drawCanvas.Tool = ToolType.Pointer;
            CurrentViewport = vp;
            foreach (var o in _viewports)
            {
                o.viewportBorder.BorderBrush = Brushes.Black;
            }
            CurrentViewport.viewportBorder.BorderBrush = Brushes.White;
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsProfile = false;
                IsDragger = false;
                IsRuler = false;
                CurrentViewport.drawCanvas.Tool = ToolType.Pointer;
                return;
            }
            IsAspectRatio = _viewportDic[CurrentViewport].IsAspectRatio;
            if (IsProfile)
            {
                profileViewModel.Initialization(_viewportDic[CurrentViewport].CurrentChannelImage);
            }
            updateDrawTool();
            if (brightnessContrastWindow != null)
            {
                brightnessContrastWindow.HistogramPanel.SetData(_viewportDic[CurrentViewport].CurrentChannelImage.ThumbnailImageData);
                _brightness = _viewportDic[CurrentViewport].CurrentChannelImage.Brightness;
                _contrast = _viewportDic[CurrentViewport].CurrentChannelImage.Contrast;
                OnPropertyChanged("Brightness");
                OnPropertyChanged("Contrast");
                OnPropertyChanged("SliderContrast");
            }
            e.Handled = true;
        }
        private void AddVP()
        {
            for (int i = 0; i < 4; i++)
            {
                var vpv = new ViewportView();
                var vpvm = new ViewportViewModel();
                vpv.DataContext = vpvm;
                vpv.PreviewMouseLeftButtonUp += Vierport_PreviewMouseLeftButtonUp;
                vpv.PreviewMouseLeftButtonDown += Vierport_PreviewMouseLeftButtonDown;
                vpv.drawCanvas.MousePoint += vpvm.OnMousePoint;
                vpv.drawCanvas.SizeChanged += vpvm.OnCanvasSizeChanged;
                vpv.drawCanvas.UpdateDisplayImage += vpvm.UpdateDisplayImage;
                vpvm.DrawingCanvas = vpv.drawCanvas;
                _viewports.Add(vpv);
                _viewportDic.Add(vpv, vpvm);
            }
        }
        private double getSliderContrast(double contrast)
        {
            double c = _contrast;
            if (c >= 1)     // (0 : 30) to (1 : 31)
                c = (c - 1d);
            else    // (-1 : -10) to (0.9 to 0.0) decrementing 0.1
            {
                c = 10d - c * 10d;
                c = -c;
            }
            return c * 100d;
        }
        private void updateBrightnessContrast()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            _viewportDic[CurrentViewport].UpdateBrightnessContrast(_brightness, _contrast);
        }
        private void EditVirtualChannel(VirtualChannel virtualChannel)
        {
            IsRuler = false;
            IsProfile = false;
            updateDrawTool();
            var window = new SetVirtualChannelWindow();
            var vm = new SetVirtualChannelViewModel(_channels, _virtualChannels, _computeColors);
            vm.IsNew = false;
            vm.ChannelName = virtualChannel.ChannelName;
            vm.Channel1= virtualChannel.FirstChannel  ;
            vm.Channel2= virtualChannel.SecondChannel ;
            vm.Operator= virtualChannel.Operator ;
            vm.Operand= virtualChannel.Operand ;
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                virtualChannel.FirstChannel = vm.Channel1;
                virtualChannel.SecondChannel = vm.Channel2;
                virtualChannel.Operator = vm.Operator;
                virtualChannel.Operand = vm.Operand;
                foreach (var o in _viewports)
                {
                    if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                        _viewportDic[o].EditVirtualChannel(virtualChannel);
                }
            }
        }
        private void DeleteVirtualChannel(VirtualChannel virtualChannel)
        {
            _virtualChannels.Remove(virtualChannel);
            foreach (var o in _viewports)
            {
                if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                    _viewportDic[o].DeleteVirtualChannel(virtualChannel);
            }

        }
        private void EditComputeColor(ComputeColor computeColor)
        {
            IsRuler = false;
            IsProfile = false;
            updateDrawTool();
            var window = new SetComputeColorWindow();
            var vm = new SetComputeColorViewModel(_channels, _virtualChannels, _computeColors);
            vm.IsNew = false;
            vm.ChannelName = computeColor.Name;
            vm.ChannelList = computeColorDic2List(computeColor.ComputeColorDictionary);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                computeColor.ComputeColorDictionary.Clear();
                foreach (var o in vm.ChannelList.Where(x => x.IsSelected))
                {
                    computeColor.ComputeColorDictionary.Add(o.Channel, o.Color);
                }
                foreach (var o in _viewports)
                {
                    if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                        _viewportDic[o].EditComputeColor(computeColor);
                }
            }

        }
        private void DeleteComputeColor(ComputeColor computeColor)
        {
            _computeColors.Remove(computeColor);
            foreach (var o in _viewports)
            {
                if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                    _viewportDic[o].DeleteComputeColor(computeColor);
            }

        }
        private void SaveChannels(int i)
        {
            XElement xe = new XElement("ImageViewerModule");
            IEnumerable<XElement> elements = from ele in xe.Elements("Scan")
                                             where(string)ele.Attribute("ScanId") == _scanId.ToString()
                                             select ele;
            if (elements.Count() > 0)
                elements.First().Remove();
            var scanxe = new XElement("Scan", new XAttribute("ScanId", _scanId.ToString()));
            var vxe = new XElement("VirtualChannels");
            foreach(var o in _virtualChannels)
            {
                var vsxe = new XElement("VirtualChannel",
                    new XAttribute("ChannelName",o.ChannelName)                    
                    );
                vsxe.Add(new XElement("ChannelId", o.ChannelId));
                vsxe.Add(new XElement("FirstChannel", o.FirstChannel.ChannelName));
                if (o.SecondChannel != null)
                    vsxe.Add(new XElement("SecondChannel", o.SecondChannel.ChannelName));
                vsxe.Add(new XElement("Operator", o.Operator));
                vsxe.Add(new XElement("Operand", o.Operand));
                vxe.Add(vsxe);
            }
            scanxe.Add(vxe);
            var cxe = new XElement("ComputeColors");
            foreach (var o in _computeColors)
            {
                var ccxe = new XElement("ComputeColor",
                    new XAttribute("Name", o.Name)
                    );
                foreach (var d in o.ComputeColorDictionary)
                {
                    var dxe = new XElement("Item",new XAttribute("Channel",d.Key.ChannelName),new XAttribute("Color",d.Value.ToString()));
                    ccxe.Add(dxe);
                }
                cxe.Add(ccxe);
            }
            scanxe.Add(cxe);
            xe.Add(scanxe);
            if (!Directory.Exists(_experiment.GetExperimentInfo().AnalysisPath + "\\ImageViewerModule"))
                Directory.CreateDirectory(_experiment.GetExperimentInfo().AnalysisPath + "\\ImageViewerModule");
            xe.Save(_experiment.GetExperimentInfo().AnalysisPath+ "\\ImageViewerModule\\channels.xml");
        }
        private void LoadChannels()
        {
            if (!Directory.Exists(_experiment.GetExperimentInfo().AnalysisPath + "\\ImageViewerModule"))
                Directory.CreateDirectory(_experiment.GetExperimentInfo().AnalysisPath + "\\ImageViewerModule");
            if (!File.Exists(_experiment.GetExperimentInfo().AnalysisPath + "\\ImageViewerModule\\channels.xml"))
                return;
            XElement xe = XElement.Load(_experiment.GetExperimentInfo().AnalysisPath + "\\ImageViewerModule\\channels.xml");
            IEnumerable<XElement> elements = from ele in xe.Elements("Scan")
                                             where (string)ele.Attribute("ScanId") == _scanId.ToString()
                                             select ele;
            if (elements.Count() > 0)
            {
                var scanxe = elements.First();
                foreach(var ele in scanxe.Element("VirtualChannels").Elements())
                {
                    var virtualChannel = new VirtualChannel() ;
                    virtualChannel.ChannelName = ele.Attribute("ChannelName").Value;
                    int id;
                    if (int.TryParse(ele.Element("ChannelId").Value, out id))
                        virtualChannel.ChannelId = id;
                    ImageOperator op;
                    if(Enum.TryParse(ele.Element("Operator").Value, out op))
                        virtualChannel.Operator = op;
                    double operand;
                    if (double.TryParse(ele.Element("Operand").Value, out operand))
                        virtualChannel.Operand = operand;
                    _virtualChannels.Add(virtualChannel);
                }
                foreach (var ele in scanxe.Element("VirtualChannels").Elements())
                {
                    var virtualChannel = _virtualChannels.Where(x => x.ChannelName == ele.Attribute("ChannelName").Value).FirstOrDefault();
                    virtualChannel.FirstChannel = gothroughChannels(ele.Element("FirstChannel").Value);
                    if(ele.Element("SecondChannel")!=null)
                    virtualChannel.SecondChannel = gothroughChannels(ele.Element("SecondChannel").Value);
                }
                foreach (var ele in scanxe.Element("ComputeColors").Elements())
                {
                    var computeColor = new ComputeColor();
                    computeColor.Name= ele.Attribute("Name").Value;
                    computeColor.ComputeColorDictionary = new Dictionary<Channel, Color>();
                    foreach(var e in ele.Elements())
                    {
                        computeColor.ComputeColorDictionary.Add(gothroughChannels(e.Attribute("Channel").Value),(Color)ColorConverter.ConvertFromString( e.Attribute("Color").Value));
                    }
                    _computeColors.Add(computeColor);
                }
            }
        }
        private Channel gothroughChannels(string name)
        {
            var sicf = _channels.Where(x => x.ChannelName == name);
            if (sicf.Count() > 0)
                return sicf.FirstOrDefault();
            else
            {
                var sivcf = _virtualChannels.Where(x => x.ChannelName == name);
                if (sivcf.Count() > 0)
                    return sivcf.FirstOrDefault();
                else
                    return null;
            }
        }
        private Dictionary<Channel, Color> computeColorList2Dic(IList<ComputeColorItem> list)
        {
            var dic = new Dictionary<Channel, Color>();
            foreach (var o in list.Where(x => x.IsSelected))
            {
                dic.Add(o.Channel, o.Color);
            }
            return dic;
        }
        private IList<ComputeColorItem> computeColorDic2List(Dictionary<Channel, Color> dic)
        {
            var list = new List<ComputeColorItem>();
            foreach (var o in _channels)
            {
                if (dic.ContainsKey(o))
                    list.Add(new ComputeColorItem() { Channel = o, IsSelected = true, Color = dic[o] });
                else
                    list.Add(new ComputeColorItem() { Channel = o, IsSelected = false, Color =Colors.Red });

            }
            foreach (var o in _virtualChannels)
            {
                if (dic.ContainsKey(o))
                    list.Add(new ComputeColorItem() { Channel = o, IsSelected = true, Color = dic[o] });
                else
                    list.Add(new ComputeColorItem() { Channel = o, IsSelected = false, Color = Colors.Red });

            }
            
            return list;
        }
        private void updateDrawTool()
        {
            if (IsProfile)
                _currentViewport.drawCanvas.Tool = ToolType.Profile;
            else if(IsRuler)
                _currentViewport.drawCanvas.Tool = ToolType.Ruler;
            else if (IsDragger)
                _currentViewport.drawCanvas.Tool = ToolType.Dragger;
            else
                _currentViewport.drawCanvas.Tool = ToolType.Pointer;
        }
    }
}
