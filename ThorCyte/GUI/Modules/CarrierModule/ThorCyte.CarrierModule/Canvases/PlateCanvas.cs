using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.CarrierModule.Carrier;
using ThorCyte.CarrierModule.Common;
using ThorCyte.CarrierModule.Events;
using ThorCyte.CarrierModule.Graphics;
using ThorCyte.CarrierModule.Tools;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;
using CaptureMode = ThorCyte.Infrastructure.Types.CaptureMode;

namespace ThorCyte.CarrierModule.Canvases
{
    public class PlateCanvas : Canvas
    {
        #region Fileds
        private const double Tolerance = 0.00000001;
        private Microplate _plate;
        private readonly VisualCollection _graphicsList;
        private readonly Hashtable _regionGraphicHashtable;
        private readonly Dictionary<string, Rect> _roomRectList = new Dictionary<string, Rect>();
        private int _currentWellId;
        private int _lastWellId;
        public List<string> AnalyzedWells;
        public string AnalyzingWell;
        private List<int> _selectedWells;

        //96 plates
        private double _plateWidth = 127760.0;
        private double _plateHeight = 85480.0;
        private float _rx;
        private float _ry;
        private double _cOffsetX;
        private double _cOffsetY;
        private double _trimLeft;
        private double _trimRight;
        private double _trimTop;
        private double _trimBottom;

        public bool IsShowing = false;


        private const double PlateMargin = 20;  //in pixel
        private readonly Tool[] _tools;

        public static readonly DependencyProperty ToolProperty;
        public static readonly DependencyProperty OuterWidthProperty;
        public static readonly DependencyProperty OuterHeightProperty;
        public static readonly DependencyProperty MousePositionProperty;
        public static readonly DependencyProperty CarrierDescriptionProperty;

        private readonly Dictionary<DisplayMode, List<ScanRegion>> _regionListDic;


        #endregion

        #region Properties

        public new object Parent
        {
            get { return LogicalTreeHelper.GetParent(this); }
        }

        public string MousePosition
        {
            get { return (string)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        public string CarrierDescription
        {
            get { return (string)GetValue(CarrierDescriptionProperty); }
            set { SetValue(CarrierDescriptionProperty, value); }
        }


        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        public ScanInfo CurrentScanInfo { get; set; }

        /// <summary>
        /// Currently active drawing tool
        /// </summary>
        public ToolType Tool
        {
            get
            {
                return (ToolType)GetValue(ToolProperty);
            }
            set
            {
                if ((int)value >= 0 && (int)value < (int)ToolType.Max)
                {
                    SetValue(ToolProperty, value);
                }
            }
        }


        public double OuterWidth
        {
            get { return (double)GetValue(OuterWidthProperty); }
            set
            {
                if (value > 0)
                    SetValue(OuterWidthProperty, value);
            }
        }

        public double OuterHeight
        {
            get { return (double)GetValue(OuterHeightProperty); }
            set
            {
                if (value > 0)
                    SetValue(OuterHeightProperty, value);
            }
        }


        public Microplate Plate
        {
            get { return _plate; }
            set
            {
                _plate = value;
                if (_plate != null)
                    InitPlate();
            }
        }

        public double PlateWidth
        {
            get { return _plateWidth; }
        }

        public double PlateHeight
        {
            get { return _plateHeight; }
        }

        public double FrontPlateMargin
        {
            get { return PlateMargin; }
        }

        private double _actualScale;
        public double ActualScale
        {
            get { return _actualScale; }
            set
            {
                _actualScale = value;
                ActualScaleChanged(this);
            }
        }

        public VisualCollection GraphicsList
        {
            get
            {
                return _graphicsList;
            }
        }

        /// <summary>
        /// Get graphic object by index
        /// </summary>
        internal GraphicsBase this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                {
                    return (GraphicsBase)_graphicsList[index];
                }

                return null;
            }
        }

        internal int Count
        {
            get
            {
                return _graphicsList.Count;
            }
        }

        private static double LineWidth
        {
            get
            {
                return 1.0;
            }
        }
        #endregion Properties

        #region Constructors

        static PlateCanvas()
        {
            // Tool
            var metaData = new PropertyMetadata(ToolType.Select);

            ToolProperty = DependencyProperty.Register(
                "Tool", typeof(ToolType), typeof(PlateCanvas),
                metaData);

            metaData = new PropertyMetadata(200.0D, OuterSizeChanged);
            OuterWidthProperty = DependencyProperty.Register("OuterWidth", typeof(double), typeof(PlateCanvas), metaData);

            metaData = new PropertyMetadata(100.0D, OuterSizeChanged);
            OuterHeightProperty = DependencyProperty.Register("OuterHeight", typeof(double), typeof(PlateCanvas), metaData);

            metaData = new PropertyMetadata("");
            MousePositionProperty = DependencyProperty.Register(
                 "MousePosition", typeof(string), typeof(PlateCanvas),
                 metaData);

            metaData = new PropertyMetadata("");
            CarrierDescriptionProperty = DependencyProperty.Register(
                 "CarrierDescription", typeof(string), typeof(PlateCanvas),
                 metaData);
        }


        public PlateCanvas()
        {
            EventAggregator.GetEvent<MacroRunEvent>().Subscribe(MacroRun, ThreadOption.UIThread, true);
            EventAggregator.GetEvent<MacroStartEvnet>().Subscribe(MacroStart, ThreadOption.UIThread, true);
            EventAggregator.GetEvent<MacroFinishEvent>().Subscribe(MacroFinish, ThreadOption.UIThread, true);
            EventAggregator.GetEvent<ShowRegionEvent>().Subscribe(ShowRegionEventHandler, ThreadOption.UIThread, true);
            EventAggregator.GetEvent<DisplayRegionTileSelectionEvent>().Subscribe(DisplayRegionTile, ThreadOption.UIThread, true);

            _graphicsList = new VisualCollection(this);
            _regionGraphicHashtable = new Hashtable();

            _tools = new Tool[(int)ToolType.Max];
            _tools[(int)ToolType.Select] = new ToolSelect();

            Loaded += DrawingCanvas_Loaded;
            MouseDown += DrawingCanvas_MouseDown;
            MouseMove += DrawingCanvas_MouseMove;
            MouseUp += DrawingCanvas_MouseUp;
            MouseEnter += DrawingCanvas_MouseEnter;
            MouseLeave += DrawingCanvas_MouseLeave;

            var drwidth = (int)(_plateWidth * 0.25 * 0.01f);
            var drHeight = (int)(_plateHeight * 0.25 * 0.01f);
            _rx = (float)(_plateWidth / drwidth);
            _ry = (float)(_plateHeight / drHeight);

            ActualScale = 0.25;
            AnalyzedWells = new List<string>();
            _currentWellId = 0;
            _lastWellId = 0;

            _regionListDic = new Dictionary<DisplayMode, List<ScanRegion>>();
            _selectedWells = new List<int>();
            ShowRegionEventHandler("ReviewModule");
        }

        #endregion Constructors

        #region Methods

        public void SelectAllGraphics()
        {
            PlateHelperFunctions.SelectAll(this);
            SetActiveRegions();
        }

        private static void OuterSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cnvs = d as PlateCanvas;

            if (cnvs == null) return;

            if (Math.Abs(cnvs.OuterWidth) < Tolerance || Math.Abs(cnvs.OuterHeight) < Tolerance)
            {
                cnvs.ActualScale = Tolerance;
                return;
            }

            var factorW = cnvs.OuterWidth / cnvs.PlateWidth;
            cnvs.ActualScale = factorW * 100;
        }

        static void ActualScaleChanged(object property)
        {
            var d = property as PlateCanvas;

            if (d == null) return;
            var scale = d.ActualScale;

            d.Width = d._plateWidth / 100 * scale;
            d.Height = d._plateHeight / 100 * scale;

            d._rx = (float)(d._plateWidth / d.Width);
            d._ry = (float)(d._plateHeight / d.Height);

            var rg = new RectangleGeometry(new Rect(0, 0, d.Width, d.Height));
            foreach (var o in d.GraphicsList.Cast<GraphicsBase>())
            {
                o.ActualScale = scale;
                o.Clip = rg;
            }
            d.UpdatePlateRoomsRect();
        }

        /// <summary>
        /// Cut Plate From all directions
        /// </summary>
        private void SetPlateTrim()
        {
            _trimLeft = ((MicroplateDef)_plate.CarrierDef).MarginLeft * 0.2;
            _trimRight = ((MicroplateDef)_plate.CarrierDef).MarginLeft * 0.55;
            _trimTop = ((MicroplateDef)_plate.CarrierDef).MarginTop * 0.1;
            _trimBottom = ((MicroplateDef)_plate.CarrierDef).MarginTop * 0.45;
        }

        private void InitPlate()
        {
            PlateHelperFunctions.DeleteAll(this);

            CarrierDescription = ((MicroplateDef)_plate.CarrierDef).Description;

            SetPlateTrim();

            Width =
                (_plate.Interval * (_plate.ColumnCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginLeft - _trimRight - _trimLeft) /
                100 * ActualScale;
            Height = (_plate.Interval * (_plate.RowCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginTop - _trimBottom - _trimTop) /
                     100 * ActualScale;

            _cOffsetX = ((MicroplateDef)_plate.CarrierDef).MarginLeft - (_plate.Interval / 2);
            _cOffsetY = ((MicroplateDef)_plate.CarrierDef).MarginTop - (_plate.Interval / 2);

            _plateWidth = _plate.Interval * (_plate.ColumnCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginLeft - _trimRight - _trimLeft;
            _plateHeight = _plate.Interval * (_plate.RowCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginTop - _trimBottom - _trimTop;

            UpdatePlateRoomsRect();
            InvalidateVisual();
        }

        /// <summary>
        /// Draw all plate rooms.
        /// </summary>
        private void UpdatePlateRoomsRect()
        {
            if (_plate == null) return;

            _roomRectList.Clear();

            for (var row = 0; row < _plate.RowCount; row++)
            {
                for (var col = 0; col < _plate.ColumnCount; col++)
                {
                    var rect = new Rect(new Point
                    {
                        X = (_plate.ColumnCount - col - 1) * _plate.Interval + _cOffsetX - _trimRight,  //form right to left
                        Y = row * _plate.Interval + _cOffsetY - _trimTop
                    }, new Size()
                    {
                        Height = _plate.Interval,
                        Width = _plate.Interval
                    });

                    var scale = ActualScale / 100;
                    rect.X *= scale;
                    rect.Y *= scale;
                    rect.Width *= scale;
                    rect.Height *= scale;

                    rect.X = Width - rect.X - rect.Width;

                    _roomRectList.Add(GetPlateId(row, col), rect);

                }
            }
        }

        private string GetPlateId(int row, int col)
        {
            var strRow = (char)('A' + row);
            var strCol = (col + 1).ToString();

            return strRow + strCol;
        }

        private string GetPlateId(int wellId)
        {
            wellId = wellId - 1;
            var r = _plate.RowCount;
            var c = _plate.ColumnCount;

            var row = (int)Math.Floor((double)wellId / c);
            var col = wellId % c;
            return GetPlateId(row, col);
        }

        private List<CaptureMode> GetBypassMode()
        {
            var ret = new List<CaptureMode> 
            {
                CaptureMode.Mode2DStream,
                CaptureMode.Mode2DTimingStream,
                CaptureMode.Mode3DFastZStream,
                CaptureMode.Mode3DStream,
                CaptureMode.Mode3DTimingStream
            };

            return ret;
        }

        private DisplayMode _currentDisplayMode = DisplayMode.Review;

        private void ShowRegionEventHandler(string moduleName)
        {
            try
            {
                if (!IsShowing) return;
                var mode = DisplayMode.Review;
                switch (moduleName)
                {
                    case "ReviewModule":
                        mode = DisplayMode.Review;
                        break;
                    case "ProtocolModule":
                        mode = DisplayMode.Protocol;
                        break;
                    case "AnalysisModule":
                        mode = DisplayMode.Analysis;
                        break;
                }

                if (mode == _currentDisplayMode) return;
                SwitchSelections(mode);
                _currentDisplayMode = mode;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occurred in PlateCanvas ShowRegionEventHandler " + ex.Message);
            }
        }

        private void SwitchSelections(DisplayMode mode)
        {
            RecordSelections(_currentDisplayMode);
            ApplySelections(mode);
        }

        private void RecordSelections(DisplayMode mode)
        {
            if (_plate.ActiveRegions == null) return;
            var rList = _plate.ActiveRegions.ToList();

            if (_regionListDic.ContainsKey(mode))
            {
                _regionListDic.Remove(mode);
            }

            _regionListDic.Add(mode, rList);
        }

        private void ClearSelections()
        {
            _plate.ClearActiveRegions();
            _selectedWells.Clear();
            foreach (var gphcs in _graphicsList.Cast<GraphicsBase>())
            {
                gphcs.IsSelected = false;
            }
            InvalidateVisual();
        }

        private void ApplySelections(DisplayMode mode)
        {
            ClearSelections();

            if (!_regionListDic.ContainsKey(mode)) return;

            foreach (var region in _regionListDic[mode])
            {
                var gph = (GraphicsBase)_regionGraphicHashtable[region];

                if (gph == null) continue;
                gph.IsSelected = true;
                _plate.AddActiveRegion(region);
            }

            if (mode == DisplayMode.Analysis)
            {
                _selectedWells.AddRange(_plate.ActiveRegions.Select(region => region.WellId));
                InvalidateVisual();
            }
        }

        /// <summary>
        /// When mode equals DisplauMode.Review, will receive a message of current choose region.
        /// </summary>
        /// <param name="rt"></param>
        private void DisplayRegionTile(RegionTile rt)
        {
            try
            {
                ClearSelections();
                var region = _plate.TotalRegions.FirstOrDefault(r => r.RegionId == rt.RegionId);
                if (region == null) return;
                var gph = (GraphicsBase)_regionGraphicHashtable[region];
                if (gph == null) return;

                gph.IsSelected = true;
                _plate.AddActiveRegion(region);

                InvalidateVisual();
            }
            catch (Exception ex)
            {
                CarrierModule.Logger.Write("Error occurred in PlateCanvas.DisplayRegionTile.", ex);
            }
        }

        public void SetActiveRegions()
        {
            var prevCount = _plate.ActiveRegions.Count;
            _plate.ClearActiveRegions();
            foreach (var rgn in from GraphicsBase o in _graphicsList where o.IsSelected select _regionGraphicHashtable.Keys.OfType<ScanRegion>().FirstOrDefault(s => Equals(_regionGraphicHashtable[s], o)))
            {
                if (rgn != null)
                    _plate.AddActiveRegion(rgn);
            }
            if (_plate.ActiveRegions.Count == prevCount && prevCount == 0)
            {
                return;
            }

            using (new WaitCursor())
            {
                //No region event publish to outside.
                //just internal use
                var eventArgs = new List<int>();
                eventArgs.Clear();
                _selectedWells.Clear();
                if (_plate.ActiveRegions != null)
                {
                    //Tileview will subscribe.
                    eventArgs.AddRange(_plate.ActiveRegions.Select(region => region.RegionId));
                    EventAggregator.GetEvent<RegionsSelected>().Publish(eventArgs);

                    if (GetBypassMode().Contains(CurrentScanInfo.Mode))
                    {
                        MessageHelper.SendStreamingStatus(true);
                        return;
                    }

                    MessageHelper.SendStreamingStatus(false);

                    switch (CarrierModule.Mode)
                    {
                        case DisplayMode.Review:
                        //case DisplayMode.Protocol:
                            eventArgs = new List<int>();
                            eventArgs.Clear();
                            eventArgs.AddRange(_plate.ActiveRegions.Select(region => region.RegionId));
                            EventAggregator.GetEvent<SelectRegions>().Publish(eventArgs);
                            break;

                        case DisplayMode.Analysis:
                            //Draw a Circle to mark choosed well.

                            eventArgs = new List<int>();
                            eventArgs.Clear();
                            eventArgs.AddRange(_plate.ActiveRegions.Select(region => region.WellId));
                            _selectedWells.AddRange(eventArgs);
                            EventAggregator.GetEvent<SelectWells>().Publish(eventArgs);
                            break;
                    }

                    InvalidateVisual();

                }

            }
        }

        public Point ClientToWorld(Point pt)
        {
            var rPt = new Point { X = pt.X * _rx / 100.0 * ActualScale, Y = pt.Y * _ry / 100.0 * ActualScale };
            return rPt;
        }

        public void UpdateScanArea()
        {
            _graphicsList.Clear();
            _regionGraphicHashtable.Clear();
            _regionListDic.Clear();
            var scale = ActualScale / 100.0;
            foreach (var rgn in _plate.TotalRegions)
            {
                var rc = rgn.Bound;

                switch (rgn.ScanRegionShape)
                {
                    case RegionShape.Ellipse:
                        var left = (_plateWidth - rc.X - rc.Width + _trimRight) * scale;
                        var top = (rc.Y - _trimTop) * scale;
                        var right = (_plateWidth - rc.X + _trimRight) * scale;
                        var bottom = (rc.Y + rc.Height - _trimTop) * scale;
                        var ellipse = new GraphicsEllipse(left, top, right, bottom, LineWidth, Colors.Green, ActualScale, 0);
                        _graphicsList.Add(ellipse);
                        _regionGraphicHashtable.Add(rgn, ellipse);
                        ellipse.RefreshDrawing();
                        break;
                    case RegionShape.Polygon:
                        var ptList = new Point[rgn.Points.Length];
                        for (var i = 0; i < rgn.Points.Length; i++)
                        {
                            ptList[i] = rgn.Points[i];
                            ptList[i].X = (_plateWidth - ptList[i].X + _trimRight) * scale;
                            ptList[i].Y = (ptList[i].Y - _trimTop) * scale;
                        }
                        var polygon = new GraphicsPolygon(ptList, LineWidth, Colors.Green, ActualScale, 0);
                        _graphicsList.Add(polygon);
                        _regionGraphicHashtable.Add(rgn, polygon);
                        polygon.RefreshDrawing();
                        break;
                    case RegionShape.Rectangle:
                        var left1 = (_plateWidth - rc.X - rc.Width + _trimRight) * scale;
                        var top1 = (rc.Y - _trimTop) * scale;
                        var right1 = (_plateWidth - rc.X + _trimRight) * scale;
                        var bottom1 = (rc.Y + rc.Height - _trimTop) * scale;
                        var rectangle = new GraphicsRectangle(left1, top1, right1, bottom1, LineWidth, Colors.Green,
                            ActualScale, 0);
                        _graphicsList.Add(rectangle);
                        _regionGraphicHashtable.Add(rgn, rectangle);
                        rectangle.RefreshDrawing();
                        break;
                }
            }

        }

        private bool IsWellChanged(int wellid)
        {
            if (wellid == _currentWellId)
                return false;
            return true;
        }

        private void MacroRun(int obj)
        {
            AnalyzedWells.Clear();
            InvalidateVisual();
        }

        private void MacroStart(MacroStartEventArgs args)
        {
            if (!IsShowing) return;
            if (!IsWellChanged(args.WellId)) return;

            IsEnabled = false;

            _lastWellId = _currentWellId;
            _currentWellId = args.WellId;


            var thiskey = GetPlateId(args.WellId);
            AnalyzingWell = thiskey;

            var lastkey = GetPlateId(_lastWellId);
            if (lastkey != string.Empty && !AnalyzedWells.Contains(lastkey))
                AnalyzedWells.Add(lastkey);

            InvalidateVisual();
        }

        private void MacroFinish(int scanid)
        {
            if (!IsShowing) return;

            AnalyzingWell = string.Empty;

            var lastkey = GetPlateId(_currentWellId);
            if (lastkey != string.Empty && !AnalyzedWells.Contains(lastkey))
                AnalyzedWells.Add(lastkey);

            IsEnabled = true;
            InvalidateVisual();
            _currentWellId = 0;
            _lastWellId = 0;
        }


        #endregion Methods

        #region Override Functions

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawBackgrouds(dc);
            DrawPlateRooms(dc);
        }

        private void DrawBackgrouds(DrawingContext dc)
        {
            Background = Brushes.DimGray;
        }

        private void DrawPlateRooms(DrawingContext dc)
        {
            if (_plate == null)
            {
                return;
            }

            foreach (var rectRoom in _roomRectList)
            {
                //Draw mark circle
                var bhRect = Brushes.DarkGray;
                foreach (var selectedWellid in _selectedWells)
                {
                    if (GetPlateId(selectedWellid) == rectRoom.Key)
                    {
                        bhRect = Brushes.Pink;
                    }
                }

                DrawFunction.DrawRectangle(dc, bhRect, new Pen(Brushes.WhiteSmoke, LineWidth), rectRoom.Value);

                var center = new Point(
                             (rectRoom.Value.Left + rectRoom.Value.Right) / 2.0,
                             (rectRoom.Value.Top + rectRoom.Value.Bottom) / 2.0);

                var radiusX = (rectRoom.Value.Right - rectRoom.Value.Left) / 2.0 * _plate.WellSize / _plate.Interval;
                var radiusY = (rectRoom.Value.Bottom - rectRoom.Value.Top) / 2.0 * _plate.WellSize / _plate.Interval;


                var bh = AnalyzedWells.Contains(rectRoom.Key) ? Brushes.Green : Brushes.SlateGray;

                var bhcircle = AnalyzedWells.Contains(rectRoom.Key) ? Brushes.LimeGreen : Brushes.White;
                if (AnalyzingWell == rectRoom.Key)
                {
                    bhcircle = Brushes.Lime;
                }


                dc.DrawEllipse(bhcircle,
                    new Pen(bh, LineWidth),
                    center,
                    radiusX,
                    radiusY);

                const double ftMaxScale = 0.5;

                if (rectRoom.Key.StartsWith("A"))
                {
                    var str = rectRoom.Key.Replace("A", string.Empty);

                    var ftsize = 16.0;
                    if (ActualScale > ftMaxScale)
                    {
                        const double tempscale = ftMaxScale;
                        ftsize = 50 * tempscale;
                        var ft = new FormattedText(str, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                           new Typeface("Verdana"), ftsize, Brushes.LightGray);

                        dc.DrawText(ft,
                            str.Length == 2
                                ? new Point(rectRoom.Value.X + rectRoom.Value.Width / 4.5, rectRoom.Value.Y - 60 * tempscale)
                                : new Point(rectRoom.Value.X + rectRoom.Value.Width / 2.5, rectRoom.Value.Y - 60 * tempscale));
                    }
                    else
                    {
                        ftsize = 50 * ActualScale;
                        var ft = new FormattedText(str, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                                   new Typeface("Verdana"), ftsize, Brushes.LightGray);

                        dc.DrawText(ft,
                            str.Length == 2
                                ? new Point(rectRoom.Value.X + rectRoom.Value.Width / 5, rectRoom.Value.Y - 60 * ActualScale)
                                : new Point(rectRoom.Value.X + rectRoom.Value.Width / 4, rectRoom.Value.Y - 60 * ActualScale));
                    }

                }


                if (rectRoom.Key.EndsWith("1") && rectRoom.Key.Length == 2)
                {
                    var str = rectRoom.Key.Replace("1", string.Empty);

                    if (ActualScale > ftMaxScale)
                    {
                        const double tempscale = ftMaxScale;
                        var ft = new FormattedText(str, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 50 * tempscale, Brushes.LightGray);
                        dc.DrawText(ft, new Point(rectRoom.Value.X - 60 * tempscale, rectRoom.Value.Y + rectRoom.Value.Height / 4));
                    }
                    else
                    {
                        var ft = new FormattedText(str, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 50 * ActualScale, Brushes.LightGray);
                        dc.DrawText(ft, new Point(rectRoom.Value.X - 60 * ActualScale, rectRoom.Value.Y + rectRoom.Value.Height / 4));
                    }

                }
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                var n = _graphicsList.Count;
                return n;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _graphicsList[index];
        }

        private void DrawingCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MousePosition = "0, 0";

            if (_tools[(int)Tool] == null)
            {
                return;
            }

            _tools[(int)Tool].OnMouseLeave(this, e);
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null)
            {
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                _tools[(int)Tool].OnMouseUp(this, e);
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_tools[(int)Tool] == null)
            {
                return;
            }

            var pt = e.GetPosition(this);

            var x = (int)(_plateWidth - pt.X * _rx + _trimRight);
            var y = (int)(pt.Y * _ry + _trimTop);

            if (x < 0) x = 0;
            if (x >= _plateWidth + _trimRight)
                x = (int)(_plateWidth + _trimRight);
            if (y < 0) y = 0;
            if (y >= _plateHeight + _trimTop)
                y = (int)(_plateHeight + _trimTop);

            MousePosition = x + ", " + y;

            if (e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released && e.LeftButton == MouseButtonState.Pressed)
            {
                _tools[(int)Tool].OnMouseMove(this, e);
            }
            else
            {
                Cursor = HelperFunctions.DefaultCursor;
            }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null)
            {
                return;
            }

            Focus();

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (e.ClickCount == 2)
                    {
                        //HandleDoubleClick(e);  
                    }
                    else
                    {
                        _tools[(int)Tool].OnMouseDown(this, e);
                        _selectedWells.Clear();
                        InvalidateVisual();
                    }
                    break;

                case MouseButton.Right:
                    break;
                //ShowContextMenu(e);
            }
        }

        private void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;      // to handle keyboard messages

            foreach (var o in _graphicsList.Cast<GraphicsBase>())
            {
                o.RefreshDrawing();
            }
        }
        #endregion
    }
}
