using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ComponentDataService;
using ComponentDataService.Types;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.GraphicModule.ViewModels
{
    public class HistogramVm : GraphicVmBase
    {
        #region Fields

        private const int DefaultYMaxValue = 1;
        private double _maxCount = 1;

        #endregion

        #region Properties

        private Dictionary<int, List<int>> _colorDataList = new Dictionary<int, List<int>>();

        public Dictionary<int, List<int>> ColorDataList
        {
            get { return _colorDataList; }
            set
            {
                if (_colorDataList == value)
                {
                    return;
                }
                SetProperty(ref _colorDataList, value);
            }
        }

        private bool _isEnabledOverlay;

        public bool IsEnabledOverlay
        {
            get { return _isEnabledOverlay; }
            set
            {
                if (_isEnabledOverlay == value)
                {
                    return;
                }
                SetProperty(ref _isEnabledOverlay, value);
            }
        }

        private bool _isCheckedOverlay;

        public bool IsCheckedOverlay
        {
            get { return _isCheckedOverlay; }
            set
            {
                if (_isCheckedOverlay == value)
                {
                    return;
                }
                GraphType = value ? GraphStyle.Outline : GraphStyle.BarChart;
                IsSwitchStyleEnabled = !value;
                SetProperty(ref _isCheckedOverlay, value);
                UpdateGraphData();
            }
        }

        private bool _isAutoYScale;

        public bool IsAutoYScale
        {
            get { return _isAutoYScale; }
            set
            {
                if (_isAutoYScale == value)
                {
                    return;
                }
                SetProperty(ref _isAutoYScale, value);
                UpdateEvents();
            }
        }

        private int _yScaleValue;

        public int YScaleValue
        {
            get { return _yScaleValue; }
            set
            {
                if (_yScaleValue == value)
                {
                    return;
                }
                SetProperty(ref _yScaleValue, value);
                UpdateEvents();
            }
        }

        private double _smooth;

        public double Smooth
        {
            get { return _smooth; }
            set
            {
                if (Math.Abs(_smooth - value) < double.Epsilon)
                {
                    return;
                }
                SetProperty(ref _smooth, value);
            }
        }

        private bool _isDeleteEnabled;

        public bool IsDeleteEnabled
        {
            get { return _isDeleteEnabled; }
            set
            {
                if (_isDeleteEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isDeleteEnabled, value);
            }
        }

        private bool _isEditEnabled;

        public bool IsEditEnabled
        {
            get { return _isEditEnabled; }
            set
            {
                if (_isEditEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isEditEnabled, value);
            }
        }

        private bool _isNewOverlayEnabled;

        public bool IsNewOverlayEnabled
        {
            get { return _isNewOverlayEnabled; }
            set
            {
                if (_isNewOverlayEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isNewOverlayEnabled, value);
            }
        }

        private bool _isSwitchStyleEnabled = true;

        public bool IsSwitchStyleEnabled
        {
            get { return _isSwitchStyleEnabled; }
            set
            {
                if (_isSwitchStyleEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isSwitchStyleEnabled, value);
            }
        }

        private OverLayModel _selectedOverlay;

        public OverLayModel SelectedOverlay
        {
            get { return _selectedOverlay; }
            set
            {
                if (_selectedOverlay == value)
                {
                    return;
                }
                IsDeleteEnabled = value != null;
                IsEditEnabled = value != null;
                SetProperty(ref _selectedOverlay, value);
            }
        }

        private DelegateCommand _deleteOverlayCmd;

        public DelegateCommand DeleteOverlayCmd
        {
            get { return _deleteOverlayCmd ?? (_deleteOverlayCmd = new DelegateCommand(DeleteOverlay)); }
        }

        private readonly ImpObservableCollection<OverLayModel> _overlayList =
            new ImpObservableCollection<OverLayModel>();

        public ImpObservableCollection<OverLayModel> OverlayList
        {
            get { return _overlayList; }
        }

        private ImpObservableCollection<int> _historyDataList = new ImpObservableCollection<int>();

        public ImpObservableCollection<int> HistoryDataList
        {
            get { return _historyDataList; }
            set { _historyDataList = value; }
        }

        public List<Dictionary<int, int>> _colorDataDictionary = new List<Dictionary<int, int>>(); 

        #endregion

        #region Constructor

        public HistogramVm()
        {
            GraphType = GraphStyle.BarChart;
            IsDrawEllipseEnabled = false;
            IsDrawPolygonEnabled = false;
            IsYAxisEnabled = false;
            YAxis.MaxRange = DefaultYMaxValue;
        }

        #endregion

        #region Methods

        public override void Init()
        {
            base.Init();
            YAxis.Title = ConstantHelper.HistogramYTitle;
            ClearPlot();
        }

        public override bool IsVisible(BioEvent ev, out Point point)
        {
            if (XAxis.SelectedNumeratorFeature == null)
            {
                point = new Point(-1, -1);
                return false;
            }

            var xValue = XAxis.GetFeatureValue(ev);
            point = new Point(xValue, 0);

            if (point.X < XAxis.MinValue || point.X >= XAxis.MaxValue)	// outside the plot area
                return false;
            return true;
        }

        public override void UpdateEvents(object args)
        {
            XScale = (XAxis.MaxValue - XAxis.MinValue) / (Width - 1);
            ClearPlot();
            base.UpdateEvents(args);
            _maxCount = _historyDataList.Max() > 1 ? _historyDataList.Max() : 1;
            if (IsEnabledOverlay)
            {
                foreach (var overlay in _overlayList)
                {
                    overlay.RefreshData();
                    for (var i = 0; i < Width; i++)
                    {
                        var f = overlay.Datas[i];
                        var j = (int)f;
                        if (j > _maxCount)
                            _maxCount = j;
                    }
                }
            }
            YAxis.IsMaxCountChanged = true;
            Update(_isAutoYScale ? _maxCount : _yScaleValue);
            YAxis.IsMaxCountChanged = false;
        }

        private void Update(double max)
        {
            ViewDispatcher.Invoke(
                () =>
                {
                    YAxis.MaxRange = max;
                    ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<GraphUpdateEvent>().Publish(new GraphUpdateArgs(Id));
                });
        }

        protected override void UpdateGraphData()
        {
            ViewDispatcher.Invoke(() => ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<GraphUpdateEvent>().Publish(new GraphUpdateArgs(Id)));
        }

        protected override void ProcessEvent(BioEvent bioEvent)
        {
            Point pt;
            if (!IsVisible(bioEvent, out pt))
            {
                return;
            }

            var x = XAxis.IsLogScale ? XAxis.GetValueBaseOnLog(pt.X) : (int)((pt.X - XAxis.MinRange) / XScale + 0.5);
            _historyDataList[x]++;

            if (bioEvent.ColorIndex != RegionColorIndex.Black)
            {
                _colorDataList[(int)bioEvent.ColorIndex][x]++;
            }
        }

        protected override void ClearPlot()
        {
            if (_colorDataList.Count == 0)
            {
                for (var index = 0; index < ConstantHelper.ColorCount; index++)
                {
                    var list = new List<int>();
                    list.AddRange(new int[Width]);
                    _colorDataList.Add(index, list);
                }
            }
            else
            {
                foreach (var key in _colorDataList.Keys)
                {
                    _colorDataList[key].Clear();
                    _colorDataList[key].AddRange(new int[Width]);
                }
            }

            _historyDataList.Clear();
            _historyDataList.AddRange(new int[Width]);
        }

        public override void InitGraphParams(string id)
        {
            Id = id;
            IsNormalizexy = true;
            IsInitialized = true;
            Init();
            XAxis.IsInitialized = false;
            YAxis.IsInitialized = false;
            XAxis.SelectedNumeratorFeature =
                XAxis.NumeratorFeatureList.FirstOrDefault(feature => feature.FeatureType == FeatureType.Area);
            XAxis.SetDefaultRange(XAxis.IsLogScale);
            XAxis.IsDefaultLabel = true;
            XAxis.UpdateTitle();
            _isAutoYScale = true;
            _yScaleValue = ConstantHelper.DefaultHistogramYScale;
            OnPropertyChanged("IsAutoYScale");
            OnPropertyChanged("YScaleValue");
            XAxis.IsInitialized = true;
            YAxis.IsInitialized = true;
        }

        public void CreateOverlay(string name, ColorInfo colorInfo)
        {
            var wellNos = new List<int>();
            if (GraphicModule.GraphicManagerVmInstance.ActiveWellNos != null)
            {
                foreach (var no in GraphicModule.GraphicManagerVmInstance.ActiveWellNos)
                {
                    wellNos.Add(no);
                }
                wellNos = wellNos.OrderBy(well=>well).ToList();
            }
            var overlay = new OverLayModel(name, colorInfo, wellNos)
            {
                ParentGraph = this
            };
            foreach (var data in _historyDataList)
            {
                overlay.Datas.Add(data);
            }
            _overlayList.Add(overlay);
            IsCheckedOverlay = true;
            IsEnabledOverlay = true;
            UpdateGraphData();
        }

        public void EditOverlay(string originalName, string newName, ColorInfo colorInfo)
        {
            foreach (var overlay in _overlayList)
            {
                if (overlay.Name == originalName)
                {
                    overlay.Name = newName;
                    overlay.OverlayColorInfo = colorInfo;
                    break;
                }
            }
            UpdateGraphData();
        }

        private void DeleteOverlay()
        {
            if (_selectedOverlay == null)
            {
                return;
            }

            _overlayList.Remove(_selectedOverlay);
            if (_overlayList.Count  == 0)
            {
                IsCheckedOverlay = false;
                IsEnabledOverlay = false;
            }
           
            UpdateGraphData();
        }

        public void SetIsNewEnabld()
        {
            IsNewOverlayEnabled = GraphicModule.GraphicManagerVmInstance.ActiveWellNos != null &&
                                  GraphicModule.GraphicManagerVmInstance.ActiveWellNos.Count > 0;
        }
        #endregion
    }
}
