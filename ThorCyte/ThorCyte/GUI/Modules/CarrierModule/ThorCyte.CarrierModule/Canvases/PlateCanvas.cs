using System.Collections;
using System.Collections.Generic;
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
        private static readonly double[] ScaleTable = { 0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 4.0, 8.0, 10.0, 12.0 };
        private Microplate _plate;
        private readonly VisualCollection _graphicsList;
        private readonly Hashtable _regionGraphicHashtable;
        private readonly Dictionary<string, Rect> _roomRectList = new Dictionary<string, Rect>();

        //96 plates
        private double _plateWidth = 127760.0;
        private double _plateHeight = 85480.0;
        private float _rx;
        private float _ry;
        private double _cOffsetX;
        private double _cOffsetY;


        private const double PlateMargin = 20;  //in pixel
        private bool _bLeftCtl;
        private Tool[] _tools;

        public static readonly DependencyProperty ToolProperty;
        public static readonly DependencyProperty ActualScaleProperty;
        public static readonly DependencyProperty MousePositionProperty;


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
        /// 
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

        public double ActualScale
        {
            get { return (double)GetValue(ActualScaleProperty); }
            set { SetValue(ActualScaleProperty, value); }
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
            var metaData = new PropertyMetadata(ToolType.Pointer);

            ToolProperty = DependencyProperty.Register(
                "Tool", typeof(ToolType), typeof(PlateCanvas),
                metaData);

            // ActualScale
            metaData = new PropertyMetadata(
                0.25,
                ActualScaleChanged);

            ActualScaleProperty = DependencyProperty.Register(
                "ActualScale", typeof(double), typeof(PlateCanvas),
                metaData);

            metaData = new PropertyMetadata("");
            MousePositionProperty = DependencyProperty.Register(
                 "MousePosition", typeof(string), typeof(PlateCanvas),
                 metaData);
        }

        public PlateCanvas()
        {
            _graphicsList = new VisualCollection(this);
            _regionGraphicHashtable = new Hashtable();


            _tools = new Tool[(int)ToolType.Max];
            _tools[(int)ToolType.Pointer] = new ToolPointer();
            _tools[(int)ToolType.Select] = new ToolSelect();
            _tools[(int)ToolType.Drag] = new ToolDrag();


            Loaded += DrawingCanvas_Loaded;
            MouseDown += DrawingCanvas_MouseDown;
            MouseMove += DrawingCanvas_MouseMove;
            MouseUp += DrawingCanvas_MouseUp;
            MouseLeave += DrawingCanvas_MouseLeave;
            MouseWheel += DrawingCanvas_MouseWheel;

            var drwidth = (int)(_plateWidth * 0.25 * 0.01f);
            var drHeight = (int)(_plateHeight * 0.25 * 0.01f);
            _rx = (float)(_plateWidth / drwidth);
            _ry = (float)(_plateHeight / drHeight);
        }

        #endregion Constructors

        #region Methods
        public void SelectAllGraphics()
        {
            PlateHelperFunctions.SelectAll(this);
            SetActiveRegions();
        }

        static void ActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
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

        private void InitPlate()
        {
            PlateHelperFunctions.DeleteAll(this);

            Width =
                (double)(_plate.Interval * (_plate.ColumnCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginLeft) /
                100 * ActualScale;
            Height = (double)(_plate.Interval * (_plate.RowCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginTop) /
                     100 * ActualScale;

            _cOffsetX = ((MicroplateDef)_plate.CarrierDef).MarginLeft - (_plate.Interval / 2);
            _cOffsetY = ((MicroplateDef)_plate.CarrierDef).MarginTop - (_plate.Interval / 2);

            _plateWidth = _plate.Interval * (_plate.ColumnCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginLeft;
            _plateHeight = _plate.Interval * (_plate.RowCount - 1) + 2 * ((MicroplateDef)_plate.CarrierDef).MarginTop;

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
                    var rect = new Rect(new Point()
                    {
                        X = (_plate.ColumnCount - col - 1) * _plate.Interval + _cOffsetX,  //form right to left
                        Y = row * _plate.Interval + _cOffsetY
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

        public void SetActiveRegions()
        {


            var prevCount = _plate.ActiveRegions.Count;
            _plate.ClearActiveRegions();
            foreach (var rgn in from GraphicsBase o in _graphicsList where o.IsSelected select _regionGraphicHashtable.Keys.OfType<ScanRegion>().FirstOrDefault(s => Equals(_regionGraphicHashtable[s], o)))
            {
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
                eventArgs.AddRange(_plate.ActiveRegions.Select(region => region.RegionId));
                EventAggregator.GetEvent<RegionsSelected>().Publish(eventArgs);

                if (GetBypassMode().Contains(CurrentScanInfo.Mode)) return;

                switch (CarrierModule.Mode)
                {
                    case DisplayMode.Review:
                        eventArgs = new List<int>();
                        eventArgs.Clear();
                        eventArgs.AddRange(_plate.ActiveRegions.Select(region => region.RegionId));
                        EventAggregator.GetEvent<SelectRegions>().Publish(eventArgs);
                        break;

                    case DisplayMode.Analysis:
                        eventArgs = new List<int>();
                        eventArgs.Clear();
                        eventArgs.AddRange(_plate.ActiveRegions.Select(region => region.WellId));
                        EventAggregator.GetEvent<SelectWells>().Publish(eventArgs);
                        break;
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
            var scale = ActualScale / 100;
            foreach (var rgn in _plate.TotalRegions)
            {
                var rc = rgn.Bound;

                switch (rgn.ScanRegionShape)
                {
                    case RegionShape.Ellipse:
                        var left = (_plateWidth - rc.X - rc.Width) * scale;
                        var top = rc.Y * scale;
                        var right = (_plateWidth - rc.X) * scale;
                        var bottom = (rc.Y + rc.Height) * scale;
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
                            ptList[i].X = (_plateWidth - ptList[i].X) * scale;
                            ptList[i].Y = ptList[i].Y * scale;
                        }
                        var polygon = new GraphicsPolygon(ptList, LineWidth, Colors.Green, ActualScale, 0);
                        _graphicsList.Add(polygon);
                        _regionGraphicHashtable.Add(rgn, polygon);
                        polygon.RefreshDrawing();
                        break;
                    case RegionShape.Rectangle:
                        var left1 = (_plateWidth - rc.X - rc.Width) * scale;
                        var top1 = rc.Y * scale;
                        var right1 = (_plateWidth - rc.X) * scale;
                        var bottom1 = (rc.Y + rc.Height) * scale;
                        var rectangle = new GraphicsRectangle(left1, top1, right1, bottom1, LineWidth, Colors.Green,
                            ActualScale, 0);
                        _graphicsList.Add(rectangle);
                        _regionGraphicHashtable.Add(rgn, rectangle);
                        rectangle.RefreshDrawing();
                        break;
                }
            }
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
            this.Background = Brushes.PowderBlue;
        }

        private void DrawPlateRooms(DrawingContext dc)
        {
            if (_plate == null)
            {
                return;
            }

            foreach (var rectRoom in _roomRectList)
            {
                DrawFunction.DrawRectangle(dc, Brushes.DarkGray, new Pen(Brushes.WhiteSmoke, LineWidth), rectRoom.Value);

                var center = new Point(
                             (rectRoom.Value.Left + rectRoom.Value.Right) / 2.0,
                             (rectRoom.Value.Top + rectRoom.Value.Bottom) / 2.0);

                var radiusX = (rectRoom.Value.Right - rectRoom.Value.Left) / 2.0;
                var radiusY = (rectRoom.Value.Bottom - rectRoom.Value.Top) / 2.0;

                dc.DrawEllipse(Brushes.White,
                    new Pen(Brushes.DarkCyan, LineWidth),
                    center,
                    radiusX,
                    radiusY);


                var formattedText = new FormattedText(rectRoom.Key,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                9 * ActualScale,
                Brushes.BlueViolet);
                dc.DrawText(formattedText, new Point(rectRoom.Value.X + 1, rectRoom.Value.Y));

                if (rectRoom.Key.StartsWith("A"))
                {
                    var str = rectRoom.Key.Replace("A", string.Empty);
                    var ft = new FormattedText(str, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 50 * ActualScale, Brushes.Black);

                    dc.DrawText(ft,
                        str.Length == 2
                            ? new Point(rectRoom.Value.X + rectRoom.Value.Width / 5, rectRoom.Value.Y - 60 * ActualScale)
                            : new Point(rectRoom.Value.X + rectRoom.Value.Width / 4, rectRoom.Value.Y - 60 * ActualScale));
                }


                if (rectRoom.Key.EndsWith("1") && rectRoom.Key.Length == 2)
                {
                    var str = rectRoom.Key.Replace("1", string.Empty);
                    var ft = new FormattedText(str, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 50 * ActualScale, Brushes.Black);
                    dc.DrawText(ft, new Point(rectRoom.Value.X - 60 * ActualScale, rectRoom.Value.Y + rectRoom.Value.Height / 4));
                }

            }

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.LeftCtrl)
            {
                _bLeftCtl = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.LeftCtrl)
            {
                _bLeftCtl = false;
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                int n = _graphicsList.Count;
                return n;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _graphicsList[index];
        }


        public void ZoomIn()
        {
            foreach (var t in ScaleTable.Where(t => ActualScale < t))
            {
                ActualScale = t;
                break;
            }
        }

        public void ZoomOut()
        {
            foreach (var t in ScaleTable.Reverse().Where(t => ActualScale > t))
            {
                ActualScale = t;
                break;
            }
        }

        private void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;
            if (e.Delta > 0)
            {
                ZoomIn();

            }
            else
            {
                ZoomOut();
            }
        }

        private void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MousePosition = "0, 0";
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

            var x = (int)(_plateWidth - pt.X * _rx);
            var y = (int)(pt.Y * _ry);

            if (x < 0) x = 0;
            if (x >= _plateWidth)
                x = (int)_plateWidth;
            if (y < 0) y = 0;
            if (y >= _plateHeight)
                y = (int)_plateHeight;

            MousePosition = x + ", " + y;

            if (e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
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
                        //HandleDoubleClick(e);        // special case for GraphicsText
                    }
                    else
                    {
                        _tools[(int)Tool].OnMouseDown(this, e);
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


        /// <summary>
        /// Move the showing area center to p
        /// </summary>
        /// <param name="p">Actual position of the plate center</param>
        public void MoveTo(Point p)
        {
            var scr = Parent as ScrollViewer;
            if (scr == null) return;
            double offsetX, offsetY;


            if (double.IsNaN(p.X))
            {
                offsetX = 0.0;
            }
            else
            {
                offsetX = scr.ScrollableWidth * (1.0 - p.X / _plateWidth);
            }


            if (double.IsNaN(p.Y))
            {
                offsetY = 0.0;
            }
            else
            {
                offsetY = scr.ScrollableHeight * (p.Y / _plateHeight);
            }

            scr.ScrollToHorizontalOffset(offsetX);
            scr.ScrollToVerticalOffset(offsetY);
        }

        /// <summary>
        /// Get current showing area center point
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentP()
        {
            var scr = Parent as ScrollViewer;
            if (scr == null) return new Point(0, 0);

            return new Point()
            {
                X = (1.0 - scr.HorizontalOffset / scr.ScrollableWidth) * _plateWidth,
                Y = scr.VerticalOffset / scr.ScrollableHeight * _plateHeight
            };

        }

        /// <summary>
        /// Drag this canvas 
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Drag(double dx, double dy)
        {
            var cp = GetCurrentP();

            var destp = new Point()
            {
                X = cp.X - dx,
                Y = cp.Y - dy
            };

            MoveTo(destp);
        }

        #endregion

    }
}
