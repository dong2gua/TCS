using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThorCyte.ImageViewerModule.Model;
using Prism.Mvvm;
using System.Windows.Media.Imaging;
using System.IO;
using ThorCyte.ImageViewerModule.View;
using Prism.Commands;
using System.Windows.Input;
using ThorCyte.ImageViewerModule.DrawTools;
using Prism.Events;
using Prism.Unity;
using ThorCyte.Infrastructure.Types;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.ImageViewerModule.Events;
using System.Windows.Media;
using ThorCyte.Infrastructure.Interfaces;
using System.Windows;
using ThorCyte.Infrastructure.Events;
using ThorCyte.ImageViewerModule.Selector;
using ImageProcess;
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
        private bool _imageViewEnable=true;
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
                if (_isProfile)
                {
                    if (!_viewportDic[CurrentViewport].IsActive||!_viewportDic[CurrentViewport].IsShown) return;
                    profileWindow = new ProfileWindow();
                    profileWindow.DataContext = profileViewModel;
                    profileWindow.Topmost = true;
                    profileViewModel.Initialization(_viewportDic[CurrentViewport].CurrentChannelImage);
                    profileWindow.Show();
                    profileWindow.Closed += ProfileWindow_Closed;
                }
                else
                {
                    if (profileWindow != null)
                        profileWindow.Close();
                }

            }
        }

        private void ProfileWindow_Closed(object sender, EventArgs e)
        {
            IsProfile = false;
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
            set { SetProperty<double>(ref _contrast, value, "Contrast"); updateBrightnessContrast(); }
        }
        private string _mousePointStatus;
        public string MousePointStatus
        {
            get { return _mousePointStatus; }
            set { SetProperty<string>(ref _mousePointStatus, value, "MousePointStatus");  }
        }
        
        private ProfileViewModel profileViewModel;
        private ProfileWindow profileWindow;
        private BrightnessContrastWindow brightnessContrastWindow;
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


        public ImageViewerViewModel()
        {

            DrawRegionCommand = new DelegateCommand<string>(OnDrawRegion);
            DraggerCommand = new DelegateCommand<bool?>(OnDraggerChanged);
            ZoomCommand = new DelegateCommand<string>(OnZoom);
            ComputeColorCommand = new DelegateCommand(OnComputeColor);
            VirtualChannelCommand = new DelegateCommand(OnVirtualChannel);
            ProfileCommand = new DelegateCommand<bool?>(OnProfile);
            RulerCommand = new DelegateCommand<bool?>(OnRuler);
            BrightnessContrastCommand = new DelegateCommand(OnBrightnessContrast);
            AutoGreyCommand = new DelegateCommand(OnAutoGrey);
            AspectRatioCommand = new DelegateCommand<bool?>(OnAspectRatio);

            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<UpdateMousePointEvent>().Subscribe(OnUpdateMousePoint);
            eventAggregator.GetEvent<OperateChannelEvent>().Subscribe(OnOperateChannel);
            eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(OnLoadExperiment);
            eventAggregator.GetEvent<SelectRegions>().Subscribe(OnSelectRegions);
            eventAggregator.GetEvent<SelectRegionTileEvent>().Subscribe(OnSelectRegionTile);
            eventAggregator.GetEvent<FrameChangedEvent>().Subscribe(OnFrameChanged);
            eventAggregator.GetEvent<UpdateCurrentChannelEvent>().Subscribe(OnUpdateChannel);

            _viewports = new List<ViewportView>();
            AddVP();
            CurrentViewport = Viewports.FirstOrDefault();
            CurrentViewport.viewportBorder.BorderBrush = Brushes.Red;

            profileViewModel = new ProfileViewModel();
            ViewportType = new ViewportDisplayType { Type = 1, Viewports = _viewports };
            _viewportDic[CurrentViewport].IsShown = true;
        }
        private IList<Channel> _channels;
        private IList<VirtualChannel> _virtualChannels;
        private IList<ComputeColor> _computeColors;
        ScanInfo _scanInfo;
        ExperimentInfo _experimentInfo;
        private IExperiment _experiment;
        private IData _data;
        private int _scanId;
        private int _regionId;
        private int _tileId;
        private double _aspectRatio;
        private Dictionary<ViewportView, ViewportViewModel> _viewportDic=new Dictionary<ViewportView, ViewportViewModel>();
        public void OnViewportTypeChange(int type)
        {            
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
            ViewportType = new ViewportDisplayType { Type = type, Viewports = _viewports }; 
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
            _experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            _data = ServiceLocator.Current.GetInstance<IData>();
            _scanId = scanId;
            _scanInfo = _experiment.GetScanInfo(scanId);
            _experimentInfo = _experiment.GetExperimentInfo();
            _aspectRatio =  _scanInfo.YPixcelSize/ _scanInfo.XPixcelSize;
            _channels = _scanInfo.ChannelList;
            _virtualChannels = _scanInfo.VirtualChannelList;
            _computeColors = _scanInfo.ComputeColorList;
            MaxBrightness = (int)Math.Pow(2, _experimentInfo.IntensityBits);
            MinBrightness = -1*MaxBrightness;
        }
        private void OnDisplay()
        {
            _viewportDic[CurrentViewport].Initialization(_data, _scanInfo,_experimentInfo, _scanId, _regionId, _tileId);
            CurrentViewport.drawCanvas.Tool = ToolType.Pointer;
            CurrentViewport.listView.Visibility = Visibility.Visible;
            CurrentViewport.drawCanvas.SetPixelSize(_scanInfo.XPixcelSize, _scanInfo.YPixcelSize);
            ImageViewEnable = true;
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
                o.viewportBorder.BorderBrush = Brushes.CadetBlue;
                CurrentViewport.viewportBorder.BorderBrush = Brushes.Red;
            }
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            if (IsProfile)
            {
                CurrentViewport.drawCanvas.Tool = ToolType.Profile;
                profileViewModel.Initialization(_viewportDic[CurrentViewport].CurrentChannelImage);
            }
            else if (IsDragger)
                CurrentViewport.drawCanvas.Tool = ToolType.Dragger;
            else if (IsRuler)
                CurrentViewport.drawCanvas.Tool = ToolType.Ruler;
            else
                CurrentViewport.drawCanvas.Tool = ToolType.Pointer;
            if(brightnessContrastWindow!=null)
            {
                Brightness = _viewportDic[CurrentViewport].CurrentChannelImage.Brightness;
                Contrast = _viewportDic[CurrentViewport].CurrentChannelImage.Contrast;
                brightnessContrastWindow.HistogramPanel.SetData(_viewportDic[CurrentViewport].CurrentChannelImage.ThumbnailImageData);
            }
            e.Handled = true;
        }
        private void OnDrawRegion(string arg)
        {
            ToolType type;
            if (!Enum.TryParse<ToolType>(arg, out type)) return;
            foreach (var o in Viewports)
            {
                o.drawCanvas.Tool = ToolType.Pointer;
            }
            IsDragger = false;
            IsProfile = false;
            IsRuler = false;
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            CurrentViewport.drawCanvas.Tool = type;
        }
        private void OnDraggerChanged(bool? arg)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsDragger = !IsDragger;
                return;
            }

            bool isDragger = arg == true;



            if (isDragger)
            {
                _currentViewport.drawCanvas.Tool = ToolType.Dragger;
                IsRuler = false;
                IsProfile = false;
            }
            else
                _currentViewport.drawCanvas.Tool = ToolType.Pointer;

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

            AddComputeColor();
        }
        private void OnVirtualChannel()
        {
            AddVirtualChannel();
        }
        private void OnProfile(bool? arg)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsProfile = !IsProfile;
                return;
            }
            bool isProfile = arg == true;

            if (isProfile)
            {
                _currentViewport.drawCanvas.Tool = ToolType.Profile;
                IsDragger = false;
                IsRuler = false;
            }
            else
            {
                _currentViewport.drawCanvas.Tool = ToolType.Pointer;
            }

        }
        private void OnRuler(bool? arg)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsRuler = !IsRuler;
                return;
            }
            bool isRuler = arg == true;

            if (isRuler)
            {
                _currentViewport.drawCanvas.Tool = ToolType.Ruler;
                IsDragger = false;
                IsProfile = false;
            }
            else
                _currentViewport.drawCanvas.Tool = ToolType.Pointer;

        }
        private void OnBrightnessContrast()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            brightnessContrastWindow = new BrightnessContrastWindow();
            brightnessContrastWindow.DataContext = this;
            brightnessContrastWindow.Topmost = true;
            brightnessContrastWindow.Show();
            Brightness = _viewportDic[CurrentViewport].CurrentChannelImage.Brightness;
            Contrast = _viewportDic[CurrentViewport].CurrentChannelImage.Contrast;
            brightnessContrastWindow.HistogramPanel.SetData(_viewportDic[CurrentViewport].CurrentChannelImage.ThumbnailImageData);
        }
        private void OnUpdateChannel(ChannelImage channel)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown|| brightnessContrastWindow==null) return;
            Brightness = _viewportDic[CurrentViewport].CurrentChannelImage.Brightness;
            Contrast = _viewportDic[CurrentViewport].CurrentChannelImage.Contrast;
            brightnessContrastWindow.HistogramPanel.SetData(_viewportDic[CurrentViewport].CurrentChannelImage.ThumbnailImageData);
        }

        private void OnAspectRatio(bool? arg)
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown)
            {
                IsAspectRatio = !IsAspectRatio;
                return;
            }
            bool isAspectRatio = arg == true;

            if (CurrentViewport == null) return;
            if (isAspectRatio)
            {
                _viewportDic[CurrentViewport].AspectRatio = _aspectRatio;
            }
            else
            {
                _viewportDic[CurrentViewport].AspectRatio = 1;
            }
        }
        private void OnAutoGrey()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            var channel = _viewportDic[CurrentViewport].CurrentChannelImage;
            if (channel == null) return;
            if (channel.IsComputeColor) return;
            var max = channel.ThumbnailImageData.Max();
            var min = channel.ThumbnailImageData.Min();
            var c = Math.Pow(2,_experimentInfo.IntensityBits) / (max - min);
            if (c >= 1)     // (0 : 30) to (1 : 31)
                c = (c - 1d);
            else    // (-1 : -10) to (0.9 to 0.0) decrementing 0.1
            {
                c = 10d - c * 10d;
                c = -c;
            }

            _contrast = c * 100d;

            _brightness = 0 - (int)(min * c);
            OnPropertyChanged("Brightness");
            OnPropertyChanged("Contrast");
            updateBrightnessContrast();
        }
        private void updateBrightnessContrast()
        {
            if (!_viewportDic[CurrentViewport].IsActive || !_viewportDic[CurrentViewport].IsShown) return;
            double c = _contrast / 100d; // jcl-6471

            if (c >= 0)     // (0 : 30) to (1 : 31)
                c = (c + 1d);
            else    // (-1 : -10) to (0.9 to 0.0) decrementing 0.1
            {
                c = Math.Abs(c);
                c = (10d - c) / 10d;
            }

            _viewportDic[CurrentViewport].UpdateBrightnessContrast(_brightness, c);
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
                var channel = _channels.Where(x => x.ChannelName == args.ChannelName).FirstOrDefault() as VirtualChannel;
                if (channel == null) return;
                if (args.Operator == 0) EditVirtualChannel(channel);
                else if (args.Operator == 1) DeleteVirtualChannel(channel);

            }

        }
        
        private void AddVirtualChannel()
        {
            var window = new SetVirtualChannelWindow();
            var vm = new SetVirtualChannelViewModel(_channels, _computeColors);
            vm.IsNew = true;
            window.DataContext = vm;
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                var virtualChannel = new VirtualChannel() { ChannelId = 0, ChannelName = vm.ChannelName, FirstChannel = vm.Channel1, SecondChannel = vm.Channel2, Operator = vm.Operator, Operand = vm.Operand };
                _channels.Add(virtualChannel);
                foreach (var o in _viewports)
                {
                    if(_viewportDic[o].IsShown&& _viewportDic[o].IsActive)
                        _viewportDic[o].AddVirtualChannel(virtualChannel);
                }
            }

        }
        private void EditVirtualChannel(VirtualChannel virtualChannel)
        {
            var window = new SetVirtualChannelWindow();
            var vm = new SetVirtualChannelViewModel(_channels, _computeColors);
            vm.IsNew = false;
            vm.ChannelName = virtualChannel.ChannelName;
            vm.Channel1= virtualChannel.FirstChannel  ;
            vm.Channel2= virtualChannel.SecondChannel ;
            vm.Operator= virtualChannel.Operator ;
            vm.Operand= virtualChannel.Operand ;
            window.DataContext = vm;
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
            _channels.Remove(virtualChannel);
            foreach (var o in _viewports)
            {
                if (_viewportDic[o].IsShown && _viewportDic[o].IsActive)
                    _viewportDic[o].DeleteVirtualChannel(virtualChannel);
            }

        }
        private void AddComputeColor()
        {
            var window = new SetComputeColorWindow();
            var vm = new SetComputeColorViewModel(_channels, _computeColors);
            vm.IsNew = true;
            window.DataContext = vm;
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
        private void EditComputeColor(ComputeColor computeColor)
        {
            var window = new SetComputeColorWindow();
            var vm = new SetComputeColorViewModel(_channels, _computeColors);
            vm.IsNew = false;
            vm.ChannelList = computeColorDic2List(computeColor.ComputeColorDictionary);
            window.DataContext = vm;
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
            return list;
        }

    }
}
