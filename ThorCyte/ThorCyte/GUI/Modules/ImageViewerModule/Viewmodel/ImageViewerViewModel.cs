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
namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ImageViewerViewModel : BindableBase
    {
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
                if (_isProfile) profileWindow.Show();
                else profileWindow.Hide();
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
        private double _brightness;
        public double Brightness
        {
            get { return _brightness; }
            set { SetProperty<double>(ref _brightness, value, "Brightness"); updateBrightnessContrast(); }
        }
        private double _contrast;
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
        
        private ProfileViewMoel ProfileViewMoel;
        private ProfileWindow profileWindow;

        public ICommand DraggerCommand { get; private set; }
        public ICommand DrawRegionCommand { get; private set; }
        public ICommand ZoomCommand { get; private set; }
        public ICommand ComputeColorCommand { get; private set; }
        public ICommand VirtualChannelCommand { get; private set; }
        public ICommand ProfileCommand { get; private set; }
        public ICommand RulerCommand { get; private set; }
        public ICommand BrightnessContrastCommand { get; private set; }
        public ICommand AutoGreyCommand { get; set; }


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

            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<UpdateMousePointEvent>().Subscribe(OnUpdateMousePoint);
            eventAggregator.GetEvent<OperateChannelEvent>().Subscribe(OnOperateChannel);
            eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(OnLoadExperiment);
            eventAggregator.GetEvent<SelectRegions>().Subscribe(OnSelectRegions);
            eventAggregator.GetEvent<SelectRegionTileEvent>().Subscribe(OnSelectRegionTile);

            _viewports = new List<ViewportView>();
            AddVP();
            CurrentViewport = Viewports.FirstOrDefault();

            ProfileViewMoel = new ProfileViewMoel();
            profileWindow = new ProfileWindow();
            profileWindow.DataContext = ProfileViewMoel;

        }
        private IList<Channel> _channels;
        private IList<VirtualChannel> _virtualChannels;
        private IList<ComputeColor> _computeColors;
        ScanInfo _scanInfo;
        private IExperiment _experiment;
        private IData _data;
        private int _scanId;
        private void OnSelectRegionTile(RegionTile regionTile)
        {
            OnDisplay(regionTile.RegionId,regionTile.TileId);
        }
        private void OnSelectRegions(List<int> regions)
        {
            if (regions.Count != 1) return;
            OnDisplay(regions[0], -1);

        }

        private void OnLoadExperiment(int scanId)
        {
            _experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            _data = ServiceLocator.Current.GetInstance<IData>();
            _scanId = scanId;
            _scanInfo = _experiment.GetScanInfo(scanId);
            _channels = _scanInfo.ChannelList;
            _virtualChannels = _scanInfo.VirtualChannelList;
            _computeColors = _scanInfo.ComputeColorList;

        }
        private void OnDisplay(int regionId,int tileId)
        {
            var vm = CurrentViewport.DataContext as ViewportViewModel;
            vm.Initialization(_data, _scanInfo, _scanId, regionId, tileId);
            ProfileViewMoel.Initialization(vm.CurrentChannelImage);


        }


        private void AddVP()
        {
            var vp = new ViewportView();
            vp.PreviewMouseLeftButtonDown += Vierport_PreviewMouseLeftButtonDown;
            _viewports.Add(vp);
        }
        private void Vierport_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vp = sender as ViewportView;
            if (vp == CurrentViewport) return;
            CurrentViewport.drawCanvas.Tool = ToolType.Pointer;
            CurrentViewport = vp;
            if (IsProfile)
            {
                CurrentViewport.drawCanvas.Tool = ToolType.Profile;
                var vvm = CurrentViewport.DataContext as ViewportViewModel;
                ProfileViewMoel.Initialization(vvm.CurrentChannelImage);
            }
            else if (IsDragger)
                CurrentViewport.drawCanvas.Tool = ToolType.Dragger;
            else if (IsRuler)
                CurrentViewport.drawCanvas.Tool = ToolType.Ruler;
            else
                CurrentViewport.drawCanvas.Tool = ToolType.Pointer;
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
            _currentViewport.drawCanvas.Tool = type;
            IsDragger = false;

        }
        private void OnDraggerChanged(bool? arg)
        {
            bool isDragger = arg == true;


            //foreach (var o in Viewports)
            //{
            //    o.drawCanvas.Tool = ToolType.Pointer;
            //}
            //_currentViewport.drawCanvas.Tool = isDragger ? ToolType.Dragger : ToolType.Pointer;

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
            if (CurrentViewport == null) return;
            bool isIn;
            if (!bool.TryParse(arg, out isIn)) return;
            var vm = CurrentViewport.DataContext as ViewportViewModel;
            if (isIn) vm.Zoomin();
            else vm.Zoomout();
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
            bool isProfile = arg == true;

            if (isProfile)
            {
                _currentViewport.drawCanvas.Tool = ToolType.Profile;
                IsDragger = false;
                IsRuler = false;
            }
            else
                _currentViewport.drawCanvas.Tool = ToolType.Pointer;

        }
        private void OnRuler(bool? arg)
        {
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
            if (CurrentViewport == null) return;
            var window = new BrightnessContrastWindow();
            window.DataContext = this;
            window.Show();
        }
        private void OnAutoGrey()
        {
            var vm = CurrentViewport.DataContext as ViewportViewModel;
            var channel = vm.CurrentChannelImage;
            if (channel == null) return;
            if (channel.IsComputeColor) return;
            var max = channel.ThumbnailImageData.DataBuffer[0];
            var min = channel.ThumbnailImageData.DataBuffer[0];
            foreach (var o in channel.ThumbnailImageData.DataBuffer)
            {
                max = Math.Max(max, o);
                min = Math.Min(min, o);
            }
            var c = 65535.0 / (max - min);
            if (c >= 1)     // (0 : 30) to (1 : 31)
                c = (c - 1d);
            else    // (-1 : -10) to (0.9 to 0.0) decrementing 0.1
            {
                c = 10d - c * 10d;
                c = -c;
            }

            _contrast = c * 100d;

            _brightness = ushort.MinValue - min * c;
            OnPropertyChanged("Brightness");
            OnPropertyChanged("Contrast");
            updateBrightnessContrast();
        }
        private void updateBrightnessContrast()
        {
            double c = _contrast / 100d; // jcl-6471

            if (c >= 0)     // (0 : 30) to (1 : 31)
                c = (c + 1d);
            else    // (-1 : -10) to (0.9 to 0.0) decrementing 0.1
            {
                c = Math.Abs(c);
                c = (10d - c) / 10d;
            }

            var vm = CurrentViewport.DataContext as ViewportViewModel;
            vm.UpdateBrightnessContrast(_brightness, c);
        }
        private void OnUpdateMousePoint(MousePointStatus status)
        {
            if (status.IsComputeColor)
                MousePointStatus = string.Format("X:{0} Y:{1} Compute Color", status.Point.X.ToString("0.0").PadLeft(10), status.Point.Y.ToString("0.0").PadLeft(10));
            else
                MousePointStatus = string.Format("X:{0} Y:{1} GL:{2}", status.Point.X.ToString("0.0").PadLeft(10), status.Point.Y.ToString("0.0").PadLeft(10), status.GrayValue.ToString().PadLeft(10));

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
                    var vvm = o.DataContext as ViewportViewModel;
                    vvm.AddVirtualChannel(virtualChannel);
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
                    var vvm = o.DataContext as ViewportViewModel;
                    vvm.EditVirtualChannel(virtualChannel);
                }
            }

        }
        private void DeleteVirtualChannel(VirtualChannel virtualChannel)
        {
            _channels.Remove(virtualChannel);
            foreach (var o in _viewports)
            {
                var vvm = o.DataContext as ViewportViewModel;
                vvm.DeleteVirtualChannel(virtualChannel);
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
                    var vvm = o.DataContext as ViewportViewModel;
                    vvm.AddComputeColor(c);
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
                    var vvm = o.DataContext as ViewportViewModel;
                    vvm.EditComputeColor(computeColor);
                }
            }

        }
        private void DeleteComputeColor(ComputeColor computeColor)
        {
            _computeColors.Remove(computeColor);
            foreach (var o in _viewports)
            {
                var vvm = o.DataContext as ViewportViewModel;
                vvm.DeleteComputeColor(computeColor);
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
