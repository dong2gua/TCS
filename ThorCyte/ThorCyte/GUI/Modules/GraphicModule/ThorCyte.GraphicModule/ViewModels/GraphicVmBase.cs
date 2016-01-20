using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ComponentDataService;
using ComponentDataService.Types;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using ROIService;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.Infrastructure.Types;
using ROIService.Region;

namespace ThorCyte.GraphicModule.ViewModels
{
    public abstract class GraphicVmBase : BindableBase
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #region Properties and Fields

        public static string DefaultGate;
        protected double XScale;
        protected double YScale;
        protected string LabelTitle = string.Empty;
        protected RegionColorIndex DefaultEventColorIndex = RegionColorIndex.Black;
        protected RegionColorIndex DefaultBackgroundIndex = RegionColorIndex.White;


        public int BinCount { get; set; }

        public bool IsInitialized { get; set; }

        public Dispatcher ViewDispatcher { get; set; }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                XAxis.GraphicId = value;
                YAxis.GraphicId = value;
            }
        }

        private int _width;

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                XAxis.BinCount = value;
            }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                YAxis.BinCount = value;
            }
        }

        private AxisModel _xAxis = new AxisModel(AxesEnum.XAxis) { IsSelected = true };

        public AxisModel XAxis
        {
            get
            {
                if (_xAxis.IsSelected)
                {
                    SelectedAxis = _xAxis;
                }
                return _xAxis;
            }
            set
            {
                if (_xAxis == value)
                {
                    return;
                }
                SetProperty(ref _xAxis, value);
            }
        }

        private AxisModel _yAxis = new AxisModel(AxesEnum.YAxis);

        public AxisModel YAxis
        {
            get
            {
                if (_yAxis.IsSelected)
                {
                    SelectedAxis = _yAxis;
                }
                return _yAxis;
            }
            set
            {
                if (_yAxis == value)
                {
                    return;
                }
                SetProperty(ref _yAxis, value);
            }
        }

        private AxisModel _selectedAxis;

        public AxisModel SelectedAxis
        {
            get { return _selectedAxis; }
            set
            {
                if (_selectedAxis == value)
                {
                    return;
                }
                SetProperty(ref _selectedAxis, value);
            }
        }

        private string _propertyWndTitle = string.Empty;

        public string PropertyWndTitle
        {
            get { return _propertyWndTitle; }
            set
            {
                if (_propertyWndTitle == value)
                {
                    return;
                }
                SetProperty(ref _propertyWndTitle, value);
            }
        }

        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                {
                    return;
                }
                SetProperty(ref _title, value);
            }
        }

        private bool _isOperatorEnable;

        public bool IsOperatorEnable
        {
            get { return _isOperatorEnable; }
            set
            {
                if (_isOperatorEnable == value)
                {
                    return;
                }
                SetProperty(ref _isOperatorEnable, value);
                if (!_isOperatorEnable)
                {
                    IsGate2Enable = false;
                }
                else
                {
                    IsGate2Enable = _selectedOperator != OperatorList[0];
                }
            }
        }

        private bool _isGate2Enable;

        public bool IsGate2Enable
        {
            get { return _isGate2Enable; }
            set
            {
                if (_isGate2Enable == value)
                {
                    return;
                }
                SetProperty(ref _isGate2Enable, value);
            }
        }

        private string _selectedGate1;

        public string SelectedGate1
        {
            get { return _selectedGate1; }
            set
            {
                if (_selectedGate1 == value)
                {
                    return;
                }
                SetProperty(ref _selectedGate1, value);
                IsOperatorEnable = _selectedGate1 != DefaultGate;
                UpdateRelationship();
            }
        }

        private string _selectedGate2;

        public string SelectedGate2
        {
            get { return _selectedGate2; }
            set
            {
                if (_selectedGate2 == value)
                {
                    return;
                }
                SetProperty(ref _selectedGate2, value);
                UpdateRelationship();
            }
        }

        private OperationType _selectedOperator;

        public OperationType SelectedOperator
        {
            get { return _selectedOperator; }
            set
            {
                if (_selectedOperator == value)
                {
                    return;
                }
                SetProperty(ref _selectedOperator, value);
                IsGate2Enable = _selectedOperator != OperationType.None;
                UpdateRelationship();
            }
        }

        private string _componentName;

        public string ComponentName
        {
            get { return _componentName; }
            set
            {
                Title = value;
                SetProperty(ref _componentName, value); 
            }
        }

        private static ToolType _regionToolType;

        public static ToolType RegionToolType
        {
            get { return _regionToolType; }
            set
            {
                if (_regionToolType == value)
                {
                    return;
                }
                _regionToolType = value;
                OnStaticPropertyChanged("RegionToolType");
            }
        }

        private ColorRegionModel _selectedRegionColor;

        public ColorRegionModel SelectedRegionColor
        {
            get { return _selectedRegionColor; }
            set
            {
                if (_selectedRegionColor == value)
                {
                    return;
                }
                var colorModel = value;
                if (colorModel.RegionColor.Equals(Colors.Black) || colorModel.RegionColor.Equals(Colors.White))
                {
                    colorModel.RegionColor = GraphicModule.GraphicManagerVmInstance.IsBlackBackground ? Colors.White : Colors.Black;
                }
                SetProperty(ref _selectedRegionColor, value); 
            }
        }

        private string _selectedComponent;

        public string SelectedComponent
        {
            get { return _selectedComponent; }
            set
            {
                if (_selectedComponent == value)
                {
                    return;
                }
                SetProperty(ref _selectedComponent, value);
            }
        }

        private bool _isNormalizexy;

        public bool IsNormalizexy
        {
            get { return _isNormalizexy; }
            set
            {
                if (_isNormalizexy == value)
                {
                    return;
                }
                SetProperty(ref _isNormalizexy, value);
            }
        }

        private bool _isColorListEnabled;

        public bool IsColorListEnabled
        {
            get { return _isColorListEnabled; }
            set
            {
                if (_isColorListEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isColorListEnabled, value);
            }
        }

        private GraphStyle _graphType;

        public GraphStyle GraphType
        {
            get { return _graphType; }
            set
            {
                if (_graphType == value)
                {
                    return;
                }
                SetProperty(ref _graphType, value);
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<GraphUpdateEvent>().Publish(new GraphUpdateArgs(Id));
            }
        }

        private bool _isYAxisEnabled;

        public bool IsYAxisEnabled
        {
            get { return _isYAxisEnabled; }
            set
            {
                if (_isYAxisEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isYAxisEnabled, value);
            }
        }

        private bool _isDrawEllipseEnabled;

        public bool IsDrawEllipseEnabled
        {
            get { return _isDrawEllipseEnabled; }
            set
            {
                if (_isDrawEllipseEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isDrawEllipseEnabled, value);
            }
        }

        private bool _isDrawPolygonEnabled;

        public bool IsDrawPolygonEnabled
        {
            get { return _isDrawPolygonEnabled; }
            set
            {
                if (_isDrawPolygonEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isDrawPolygonEnabled, value);
            }
        }

        private ImpObservableCollection<string> _gate1List = new ImpObservableCollection<string>();

        public ImpObservableCollection<string> Gate1List
        {
            get { return _gate1List; }
            set { _gate1List = value; }
        }

        private ImpObservableCollection<string> _gate2List = new ImpObservableCollection<string>();

        public ImpObservableCollection<string> Gate2List
        {
            get { return _gate2List; }
            set { _gate2List = value; }
        }

        private readonly ImpObservableCollection<OperationType> _operatorList = new ImpObservableCollection<OperationType>();

        public ImpObservableCollection<OperationType> OperatorList
        {
            get { return _operatorList; }
        }

        private static readonly ImpObservableCollection<ColorRegionModel> _colorRegionList = new ImpObservableCollection<ColorRegionModel>()
        {
            new ColorRegionModel
            {
                IsChecked = true,
                RegionColor = Colors.White,
                RegionColorString = "Default"
            },
            new ColorRegionModel
            {
                RegionColor = Colors.Red,
                RegionColorString = "Red"
            },
            new ColorRegionModel
            {
                RegionColor = Colors.Green,
                RegionColorString = "Green"
            },
            new ColorRegionModel
            {
                RegionColor = Colors.Blue,
                RegionColorString = "Blue"
            },
            new ColorRegionModel
            {
                RegionColor = Colors.Yellow,
                RegionColorString = "Yellow"
            },
            new ColorRegionModel
            {
                RegionColor = Colors.Magenta,
                RegionColorString = "Magenta"
            },
            new ColorRegionModel
            {
                RegionColor = Colors.Cyan,
                RegionColorString = "Cyan"
            }
        };

        public static ImpObservableCollection<ColorRegionModel> ColorRegionList
        {
            get { return _colorRegionList; }
        }

        private readonly ImpObservableCollection<string> _componentList = new ImpObservableCollection<string>();

        public ImpObservableCollection<string> ComponentList
        {
            get { return _componentList; }
        }

        #endregion

        #region Methods

        protected abstract void UpdateGraphData();

        protected virtual void ProcessEvent(BioEvent bioEvent) { }

        protected virtual void ClearPlot() { }

        protected virtual bool ValidateParameters() { return true; }

        public void SetSize(int width,int height)
        {
            Width = width;
            Height = height;
        }

        public virtual void UpdateEvents(object args)
        {
            if (!string.IsNullOrEmpty(_selectedGate1) && _selectedGate1 != DefaultGate)
            {
                var gate2 = string.Empty;
                if (_selectedOperator != OperationType.None && string.IsNullOrEmpty(_selectedGate2))
                {
                    return;
                }
                if (_isGate2Enable || _selectedOperator == OperationType.None)
                {
                    gate2 = _selectedGate2 ?? string.Empty;
                }
                var events = ROIManager.Instance.GetEvents(_selectedGate1, gate2, _selectedOperator);
                if (events == null || events.Count == 0)
                {
                    UpdateGraphData();
                    return;
                }
                foreach (var ev in events)
                {
                    ProcessEvent(ev);
                }
            }
            else
            {
                var activewells = args as IList<Well>;
                if (activewells == null || activewells.Count == 0)
                {
                    UpdateGraphData();
                    return;
                }
                foreach (var well in activewells)
                {
                    var events = ComponentDataManager.Instance.GetEvents(ComponentName, well.WellId);

                    if (events == null || events.Count == 0)
                    {
                        continue;
                    }
                    events.ToList().ForEach(ProcessEvent);
                }
            }
        }

        public virtual void SetTitle() { }

        public virtual void InitGraphParams(string id) { }

        public virtual void Init()
        {
            DefaultGate = OperationType.None.ToString();

            var components = ComponentDataManager.Instance.GetComponentNames();
            foreach (var component in components)
            {
                _componentList.Add(component);
            }
            _selectedComponent = _componentList.FirstOrDefault(component => component == ComponentName);

            var features = ComponentDataManager.Instance.GetFeatures(_selectedComponent).OrderBy(f=>f.Name);
            var channels = ComponentDataManager.Instance.GetChannels(_selectedComponent).OrderBy(channel => channel.ChannelName);

            _xAxis.AddFeatures(features);
            _yAxis.AddFeatures(features);

            _xAxis.AddChannles(channels);
            _yAxis.AddChannles(channels);
            
            _operatorList.AddRange(Enum.GetValues(typeof(OperationType)));
            _gate1List.Add(DefaultGate);
            _selectedGate1 = _gate1List[0];
            _selectedOperator = _operatorList[0];
        }


        public virtual bool IsVisible(BioEvent ev, out Point point)
        {
            point = new Point(0, 0);
            return false;
        }

        public void UpdateEvents()
        {
            if (!IsInitialized)
            {
                return;
            }
            if (_isNormalizexy)
            {
                Normalizexy();
            }
            UpdateEvents(GraphicModule.GraphicManagerVmInstance.ActiveWells);
        }

        public void Normalizexy()
        {
            //var activeWells = GraphicManagerVm.Instance.ActiveWells;

            //if (activeWells == null)
            //{
            //    return;
            //}
            //double min;
            //double max;

            //if (activeWells.Count == 0)
            //{
            //    return;
            //}

            //if (_xAxis.SelectedNumeratorFeature.FeatureType == FeatureType.XPos)
            //{
            //    min = activeWells[0].ScanRegion.Bounds.Left;
            //    max = activeWells[0].ScanRegion.Bounds.Right;
            //    foreach (var well in activeWells)
            //    {
            //        var rect = well.ScanRegion.Bounds;
            //        if (rect.Left < min)
            //        {
            //            min = rect.Left;
            //        }
            //        if (rect.Right > max)
            //        {
            //            max = rect.Right;
            //        }
            //    }
            //    _xAxis.SetRange(min, max);
            //}
            //if (_yAxis.SelectedNumeratorFeature != null && _yAxis.SelectedNumeratorFeature.FeatureType == FeatureType.YPos)
            //{
            //    min = activeWells[0].ScanRegion.Bounds.Top;
            //    max = activeWells[0].ScanRegion.Bounds.Bottom;
            //    foreach (var well in activeWells)
            //    {
            //        var rect = well.ScanRegion.Bounds;
            //        if (rect.Top < min)
            //        {
            //            min = rect.Top;
            //        }
            //        if (rect.Bottom > max)
            //        {
            //            max = rect.Bottom;
            //        }
            //    }
            //    _yAxis.SetRange(min, max);
            //}
        }

        private void UpdateRelationship()
        {
            if (!IsInitialized || !GraphicManagerVm.IsLoadGateEnd)
            {
                return;
            }
            GraphicModule.GraphicManagerVmInstance.UpdateRelationShip(Id);
            GraphicModule.GraphicManagerVmInstance.UpdateRegionList();
        }

        static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
