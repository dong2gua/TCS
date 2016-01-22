using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ComponentDataService;
using ComponentDataService.Types;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.GraphicModule.ViewModels
{
    public class ScattergramVm : GraphicVmBase
    {
        #region Fields

        public const string DefaultPrefixTitle = ConstantHelper.PrefixScattergramName;
        private int _zIndex;
        public const float DefaultZScaleMin = 0;
        public const float DefaultZScaleMax = 10;
        private int[,] _valueArray;
        private int[,] _densityArray;
        private List<Tuple<Point, int>> _dotTupleList = new List<Tuple<Point, int>>();

        #endregion

        #region Properties and Fields

        public int ZIndex
        {
            get { return _zIndex; }
        }

        public List<Tuple<Point, int>> DotTupleList
        {
            get { return _dotTupleList; }
        }

        private bool _isWhiteBackground;

        //public bool IsWhiteBackground
        //{
        //    get { return _isWhiteBackground; }
        //    set
        //    {
        //        if (_isWhiteBackground == value)
        //        {
        //            return;
        //        }
        //        _isWhiteBackground = value;
        //        GraphicModule.GraphicManagerVmInstance.IsBlackBackground = !_isWhiteBackground;
        //    }
        //}

        private bool _isMapChecked;

        public bool IsMapChecked
        {
            get { return _isMapChecked; }
            set
            {
                if (_isMapChecked == value)
                {
                    return;
                }
                if (value)
                {
                    SetZScaleDefault();
                    if (!_isExpression && !_isDensity)
                    {
                        IsDensity = true;
                    }
                }
                SetProperty(ref _isMapChecked, value);
                UpdateEvents();
            }
        }

        private bool _isDensity;

        public bool IsDensity
        {
            get { return _isDensity; }
            set
            {
                if (_isDensity == value)
                {
                    return;
                }
                SetProperty(ref _isDensity, value);
                if (_isExpression == value)
                {
                    IsExpression = !value;
                }
               
                if (_isDensity)
                {
                    UpdateEvents();
                }
            }
        }

        private bool _isExpression;

        public bool IsExpression
        {
            get { return _isExpression; }
            set
            {
                if (_isExpression == value)
                {
                    return;
                }
                if (value == _isDensity)
                {
                    IsDensity = !value;
                }
                SetProperty(ref _isExpression, value);
                if (_isExpression && _selecedZScaleFeature == null)
                {
                    SelectedZScaleFeature = _zScaleFeatureList[0];
                }

                IsZScaleChannelEnabled = _isExpression && _selecedZScaleFeature != null &&
                                         _selecedZScaleFeature.IsPerChannel;
                if (_isExpression)
                {
                    UpdateEvents();
                }
            }
        }

        private float _zScaleMin;

        public float ZScaleMin
        {
            get { return _zScaleMin; }
            set
            {
                if (Math.Abs(_zScaleMin - value) < double.Epsilon)
                {
                    return;
                }
                SetProperty(ref _zScaleMin, value);
                UpdateEvents();
            }
        }

        private float _zScaleMax;

        public float ZScaleMax
        {
            get { return _zScaleMax; }
            set
            {
                if (Math.Abs(_zScaleMax - value) < double.Epsilon)
                {
                    return;
                }
                SetProperty(ref _zScaleMax, value);
                UpdateEvents();
            }
        }

        private Feature _selecedZScaleFeature;

        public Feature SelectedZScaleFeature
        {
            get { return _selecedZScaleFeature; }
            set
            {
                if (_selecedZScaleFeature == value)
                {
                    return;
                }
                SetProperty(ref _selecedZScaleFeature, value);
                if (value.IsPerChannel)
                {
                    IsZScaleChannelEnabled = true;
                    SelecedZScaleChannel = _zScaleChannelList[0];
                }
                else
                {
                    IsZScaleChannelEnabled = false;
                    SelecedZScaleChannel = null;
                }
                CalcZIndex();
                UpdateEvents();
            }
        }

        private Channel _selecedZScaleChannel;

        public Channel SelecedZScaleChannel
        {
            get { return _selecedZScaleChannel; }
            set
            {
                if (_selecedZScaleChannel == value)
                {
                    return;
                }
                SetProperty(ref _selecedZScaleChannel, value);
                CalcZIndex();
                UpdateEvents();
            }
        }

        private bool _isZScaleChannelEnabled;

        public bool IsZScaleChannelEnabled
        {
            get { return _isZScaleChannelEnabled; }
            set
            {
                if (_isZScaleChannelEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isZScaleChannelEnabled, value);
            }
        }

        private bool _isShowQuadrant;

        public bool IsShowQuadrant
        {
            get { return _isShowQuadrant; }
            set
            {
                if (_isShowQuadrant == value)
                {
                    return;
                }
                SetProperty(ref _isShowQuadrant, value);
            }
        }

        private bool _isSnap;

        public bool IsSnap
        {
            get { return _isSnap; }
            set
            {
                if (_isSnap == value)
                {
                    return;
                }
                SetProperty(ref _isSnap, value);
            }
        }

        private readonly ImpObservableCollection<Point> _eventPointList = new ImpObservableCollection<Point>();

        public ImpObservableCollection<Point> EventPointList
        {
            get { return _eventPointList; }
        }

        private readonly ImpObservableCollection<Feature> _zScaleFeatureList = new ImpObservableCollection<Feature>();

        public ImpObservableCollection<Feature> ZScaleFeatureList
        {
            get { return _zScaleFeatureList; }
        }

        private readonly ImpObservableCollection<Channel> _zScaleChannelList = new ImpObservableCollection<Channel>();

        public ImpObservableCollection<Channel> ZScaleChannelList
        {
            get { return _zScaleChannelList; }
        }

        #endregion

        #region Constructor

        public ScattergramVm()
        {
            IsDrawEllipseEnabled = true;
            IsDrawPolygonEnabled = true;
            IsYAxisEnabled = true;
            BinCount = ConstantHelper.LowBinCount;
            GraphType = GraphStyle.DotPlot;
        }

        #endregion

        #region Methods

        public override void UpdateFeatures()
        {
            base.UpdateFeatures();
            var features = ComponentDataManager.Instance.GetFeatures(SelectedComponent).OrderBy(f => f.Name);
            var channels = ComponentDataManager.Instance.GetChannels(SelectedComponent).OrderBy(channel => channel.ChannelName);
            foreach (var feature in features)
            {
                _zScaleFeatureList.Add(feature);
            }

            foreach (var channel in channels)
            {
                _zScaleChannelList.Add(channel);
            }
        }

        //public void UpdateBackground()
        //{
        //    IsWhiteBackground = !GraphicModule.GraphicManagerVmInstance.IsBlackBackground;
        //    var color = GraphicModule.GraphicManagerVmInstance.IsBlackBackground ? Colors.White : Colors.Black;
        //    ColorRegionList[0].RegionColor = color;
        //    DefaultEventColorIndex = _isWhiteBackground ? RegionColorIndex.Black : RegionColorIndex.White;
        //    DefaultBackgroundIndex = _isWhiteBackground ? RegionColorIndex.White : RegionColorIndex.Black;
        //    OnPropertyChanged("IsWhiteBackground");
        //    UpdateEvents();
        //}

        public override void UpdateEvents(object args)
        {
            ClearPlot();
            XScale = (XAxis.MaxValue - XAxis.MinValue) / (Width - 1);
            YScale = (YAxis.MaxValue - YAxis.MinValue) / (Height - 1);
            base.UpdateEvents(args);
            UpdateGraphData();
        }

        protected override void UpdateGraphData()
        {
            if (_isMapChecked)
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var curVal = _isExpression ? _valueArray[x, y] : _densityArray[x, y];
                        if (curVal == 0)
                        {
                            continue;
                        }
                        var binColorIndex = GetBinnedZValue(curVal);
                        _dotTupleList.Add(new Tuple<Point, int>(new Point(x, y), binColorIndex));
                    }
                }
            }
            SendMessageToUpdate();
        }

        private void SendMessageToUpdate()
        {
            ViewDispatcher.Invoke(() => ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<GraphUpdateEvent>().Publish(new GraphUpdateArgs(Id)));
        }

        public override bool IsVisible(BioEvent ev, out Point point)
        {
            if (XAxis.SelectedNumeratorFeature == null)
            {
                point = new Point(-1, -1);
                return false;
            }

            double xValue = XAxis.GetFeatureValue(ev);
            double yValue = YAxis.GetFeatureValue(ev);

            point = new Point(xValue, yValue);

            if (point.X < XAxis.MinValue || point.Y < YAxis.MinValue || point.X >= XAxis.MaxValue || point.Y > YAxis.MaxValue)	// outside the plot area
                return false;
            return true;
        }

        protected override void ProcessEvent(BioEvent bioEvent)
        {
            Point point;
            if (!IsVisible(bioEvent, out point))
            {
                return;
            }

            if (!_isMapChecked)
            {
                _dotTupleList.Add(new Tuple<Point, int>(point, (int)bioEvent.ColorIndex));
            }
            else
            {
                var x = XAxis.IsLogScale ? XAxis.GetValueBaseOnLog(point.X) : (int)Math.Round((point.X - XAxis.MinValue) / XScale + 0.5);
                var y = YAxis.IsLogScale ? YAxis.GetValueBaseOnLog(point.Y) : (int)Math.Round((point.Y - YAxis.MinValue) / YScale + 0.5);
                ++_densityArray[x, y];
                if (_isExpression)
                {
                    _valueArray[x, y] = (int)((bioEvent[_zIndex] + (_densityArray[x, y] - 1) * _valueArray[x, y]) / _densityArray[x, y]);
                }
            }
        }

        public override void SetTitle()
        {
            if (string.IsNullOrEmpty(LabelTitle))
            {
                if (_isMapChecked)
                {
                    if (_isDensity)
                    {
                        Title = ConstantHelper.DensityString;
                    }
                    else
                    {
                        if (_selecedZScaleFeature != null)
                        {
                            Title = _selecedZScaleFeature.IsPerChannel
                                ? _selecedZScaleChannel.ChannelName + ' ' + _selecedZScaleFeature.Name
                                : _selecedZScaleFeature.Name;
                        }
                    }
                }
                else
                {
                    Title = ComponentName;
                }
            }
            else
            {
                Title = LabelTitle;
            }
        }

        protected override void ClearPlot()
        {
            _dotTupleList.Clear();
            if (_isMapChecked)
            {
                _densityArray = new int[Width, Height];
                if (_isExpression)
                {
                    _valueArray = new int[Width, Height];
                }
            }
        }

        protected override bool ValidateParameters()
        {
            if (_isMapChecked)
            {
                if (!_isExpression)
                {
                    if (_selecedZScaleFeature == null)
                    {
                        MessageBox.Show("Select z scale feature", "ThorCyte", MessageBoxButton.OK);
                        return false;
                    }

                    if (_selecedZScaleFeature.IsPerChannel)
                    {
                        if (_selecedZScaleChannel == null)
                        {
                            MessageBox.Show("Select z scale channel", "ThorCyte", MessageBoxButton.OK);
                            return false;
                        }
                    }
                }
            }
            return XAxis.IsValidate() && YAxis.IsValidate();
        }

        public override void InitGraphParams(string id)
        {
            Id = id;
            IsNormalizexy = true;
            IsInitialized = true;
            Init();
            XAxis.IsInitialized = false;
            YAxis.IsInitialized = false;
            XAxis.IsDefaultLabel = true;
            XAxis.SelectedNumeratorFeature =
                XAxis.NumeratorFeatureList.FirstOrDefault(feature => feature.FeatureType == FeatureType.XPos);
            XAxis.SetDefaultRange(XAxis.IsLogScale);
            XAxis.UpdateTitle();
            YAxis.IsDefaultLabel = true;
            YAxis.SelectedNumeratorFeature =
            YAxis.NumeratorFeatureList.FirstOrDefault(feature => feature.FeatureType == FeatureType.YPos);
            YAxis.SetDefaultRange(XAxis.IsLogScale);
            YAxis.UpdateTitle();
            //IsWhiteBackground = !GraphicModule.GraphicManagerVmInstance.IsBlackBackground;
            XAxis.IsInitialized = true;
            YAxis.IsInitialized = true;
        }

        /// <summary>
        /// Returns the binned value along the z axis
        /// </summary>
        /// <param name="zValue">raw input z value</param>
        /// <returns>The integer value of the color to be displayed.</returns>
        public int GetBinnedZValue(double zValue)
        {
            int count = ConstantHelper.TemperatureColors.Length;
            double min = _zScaleMin;
            double max = _zScaleMax;
            double scale = (max - min) / count;
            int index = 0;

            if ((Math.Abs(scale) < double.Epsilon) || (Math.Abs(zValue) < double.Epsilon))
            {
                return index;
            }
            index = (int)((zValue - min) / scale);

            if (index >= count)
                index = count - 1;	// true at max
            else if (index < 0)
                index = 0;
            return index;
        }

        private void CalcZIndex()
        {
            if (_selecedZScaleFeature != null)
            {
                _zIndex = _selecedZScaleFeature.Index;
                if (_selecedZScaleFeature.IsPerChannel)
                {
                    _zIndex += _selecedZScaleChannel.ChannelId;
                }
            }
        }

        private void SetZScaleDefault()
        {
            ZScaleMin = DefaultZScaleMin;
            ZScaleMax = DefaultZScaleMax;
        }

        #endregion
    }
}
