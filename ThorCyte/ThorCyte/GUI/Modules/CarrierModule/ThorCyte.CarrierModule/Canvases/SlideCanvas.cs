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
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using CaptureMode = ThorCyte.Infrastructure.Types.CaptureMode;


namespace ThorCyte.CarrierModule.Canvases
{
    class SlideCanvas : Canvas
    {
        #region Static Members
        private static readonly double[] ScaleTable = { 0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 4.0, 8.0, 10.0, 12.0 };
        #endregion

        #region Class Members
        private readonly VisualCollection _graphicsList;
        private readonly Hashtable _regionGraphicHashtable;
        private readonly List<Rect> _roomRectList = new List<Rect>();
        private Slide _slideMod;

        public static readonly DependencyProperty ToolProperty;
        public static readonly DependencyProperty ActualScaleProperty;
        public static readonly DependencyProperty MousePositionProperty;

        private readonly Tool[] _tools;                   // Array of tools

        private double _slideWidth = 75000;
        private double _slideHeight = 25000;
        private float _rx;
        private float _ry;

        #endregion Class Members

        #region Constructors
        public SlideCanvas()
        {
            _graphicsList = new VisualCollection(this);
            _regionGraphicHashtable = new Hashtable();
            // create array of drawing tools
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


            var drwidth = (int)(_slideWidth * 0.5 * 0.01f);
            var drHeight = (int)(_slideHeight * 0.5 * 0.01f);
            _rx = (float)(_slideWidth / drwidth);
            _ry = (float)(_slideHeight / drHeight);
            IsLocked = true;
        }


        static SlideCanvas()
        {
            // Tool
            var metaData = new PropertyMetadata(ToolType.Pointer);

            ToolProperty = DependencyProperty.Register(
                "Tool", typeof(ToolType), typeof(SlideCanvas),
                metaData);

            // ActualScale
            metaData = new PropertyMetadata(
                0.5,                                                        // default value
                ActualScaleChanged);           // change callback

            ActualScaleProperty = DependencyProperty.Register(
                "ActualScale", typeof(double), typeof(SlideCanvas),
                metaData);

            metaData = new PropertyMetadata("");
            MousePositionProperty = DependencyProperty.Register(
                 "MousePosition", typeof(string), typeof(SlideCanvas),
                 metaData);
        }
        #endregion Constructor

        #region Properties

        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        public ScanInfo CurrentScanInfo { get; set; }

        public new object Parent
        {
            get { return LogicalTreeHelper.GetParent(this); }
        }

        public double LineWidth
        {
            get
            {
                return 1.0;
            }
        }

        public Color ObjectColor
        {
            get
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
        }

        public double ActualScale
        {
            get { return (double)GetValue(ActualScaleProperty); }
            set { SetValue(ActualScaleProperty, value); }
        }

        public Slide SlideMod
        {
            get { return _slideMod; }
            set
            {
                _slideMod = value;
                InitSlide();
            }
        }

        public int CurrentRoomNo { get; set; }

        public bool IsLocked { get; set; }

        /// <summary>
        /// Callback function called when ActualScale dependency property is changed.
        /// </summary>
        static void ActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var d = property as SlideCanvas;

            if (d == null) return;
            var scale = d.ActualScale;

            d.Width = d._slideWidth / 100 * scale;
            d.Height = d._slideHeight / 100 * scale;

            var drwidth = (int)(d._slideWidth * scale * 0.01f);
            var drHeight = (int)(d._slideHeight * scale * 0.01f);
            d._rx = (float)(d._slideWidth / drwidth);
            d._ry = (float)(d._slideHeight / drHeight);

            var rg = new RectangleGeometry(new Rect(0, 0, d.Width, d.Height));
            foreach (var o in d.GraphicsList.Cast<GraphicsBase>())
            {
                o.ActualScale = scale;
                o.Clip = rg;
            }
            d.UpdateRoomRects();
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

        /// <summary>
        /// Get number of graphic objects
        /// </summary>
        internal int Count
        {
            get
            {
                return _graphicsList.Count;
            }
        }

        /// <summary>
        /// Return list of graphics
        /// </summary>
        internal VisualCollection GraphicsList
        {
            get
            {
                return _graphicsList;
            }
        }

        /// <summary>
        /// Returns INumerable which may be used for enumeration
        /// of selected objects.
        /// </summary>
        internal IEnumerable<GraphicsBase> Selection
        {
            get
            {
                return from GraphicsBase o in _graphicsList where o.IsSelected select o;
            }
        }

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

        public string MousePosition
        {
            get { return (string)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        #endregion Properties

        #region Override Functions
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawFunction.DrawRectangle(dc,
                null,
                new Pen(new SolidColorBrush(ObjectColor), LineWidth),
                new Rect(0, 0, Width, Height));

            DrawRooms(dc);

            //DrawGrid(dc);
            var rcWidth = 10 * ActualScale;
            DrawFunction.DrawRectangle(dc,
                Brushes.Black,
                null,
                new Rect(Width - rcWidth, 0, rcWidth, rcWidth));
        }
        #endregion

        #region Mouse Event Handlers
        void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
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

        void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_tools[(int)Tool] == null)
            {
                return;
            }

            var pt = e.GetPosition(this);

            var x = (int)(_slideWidth - pt.X * _rx - 1);
            var y = (int)(pt.Y * _ry);

            if (x < 0) x = 0;
            if (x >= _slideWidth)
                x = (int)_slideWidth - 1;
            if (y < 0) y = 0;
            if (y >= _slideHeight)
                y = (int)_slideHeight - 1;

            MousePosition = x + ", " + y;

            if (e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                _tools[(int)Tool].OnMouseMove(this, e);
                //UpdateState();
            }
            else
            {
                Cursor = HelperFunctions.DefaultCursor;
            }
        }

        void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
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

        void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MousePosition = "0, 0";
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

        #endregion

        #region Other Event Handlers

        void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;      // to handle keyboard messages
            foreach (var o in _graphicsList.Cast<GraphicsBase>())
            {
                o.RefreshDrawing();
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
        #endregion

        #region Private Functions

        private void InitSlide()
        {
            HelperFunctions.DeleteAll(this);

            Width = SlideMod.Size.Width / 100 * ActualScale;
            Height = SlideMod.Size.Height / 100 * ActualScale;

            _slideWidth = SlideMod.Width;
            _slideHeight = SlideMod.Height;
            UpdateRoomRects();
            InvalidateVisual();
        }

        private void DrawRooms(DrawingContext dc)
        {
            if (SlideMod == null)
            {
                return;
            }

            var index = 0;
            foreach (CarrierRoom room in SlideMod.Rooms)
            {
                var rect = _roomRectList[index];

                var formattedText = new FormattedText(room.No.ToString(CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    10,
                    Brushes.Gray);

                switch (room.ScannableShape)
                {
                    case RegionShape.Rectangle:
                        {
                            DrawFunction.DrawRectangle(dc, Brushes.White, new Pen(new SolidColorBrush(ObjectColor), LineWidth), rect);

                            dc.DrawText(formattedText, new Point(rect.Left + 1, rect.Top));
                            break;
                        }
                    case RegionShape.Ellipse:
                        {
                            var center = new Point(
                                (rect.Left + rect.Right) / 2.0,
                                (rect.Top + rect.Bottom) / 2.0);

                            var radiusX = (rect.Right - rect.Left) / 2.0;
                            var radiusY = (rect.Bottom - rect.Top) / 2.0;

                            dc.DrawEllipse(
                                Brushes.White,
                                new Pen(new SolidColorBrush(ObjectColor), LineWidth),
                                center,
                                radiusX,
                                radiusY);

                            var pt = new Point
                            {
                                X = (rect.Left + rect.Width / 2) - 4 * ActualScale,
                                Y = (rect.Top + rect.Height / 2) - 4 * ActualScale
                            };
                            dc.DrawText(formattedText, pt);

                            break;
                        }
                }
                index++;
            }
        }

        private void DrawGrid(DrawingContext dc)
        {
            var rcWidth = 10 * ActualScale;
            var pt0 = new Point();
            var pt1 = new Point();

            var pen1 = new Pen(Brushes.Black, 0.5);

            var pen2 = new Pen(Brushes.Black, 0.2);
            Pen pen;

            for (var i = 1; i < Width / (10 * ActualScale); i++)
            {
                pt0.X = pt1.X = i * rcWidth;
                pt0.Y = 0;
                pt1.Y = Height;
                pen = i % 5 == 0 ? pen1 : pen2;
                DrawFunction.DrawLine(dc, pen, pt0, pt1);
            }

            for (var i = 1; i < Height / (10 * ActualScale); i++)
            {
                pt0.X = 0;
                pt1.X = Width;
                pt0.Y = pt1.Y = i * rcWidth;
                pen = i % 5 == 0 ? pen1 : pen2;
                DrawFunction.DrawLine(dc, pen, pt0, pt1);
            }
        }

        private void UpdateRoomRects()
        {
            _roomRectList.Clear();
            foreach (CarrierRoom room in SlideMod.Rooms)
            {
                var rect = room.ScannableRect;
                var scale = ActualScale / 100;
                rect.X *= scale;
                rect.Y *= scale;
                rect.Width *= scale;
                rect.Height *= scale;

                rect.X = Width - rect.X - rect.Width;
                _roomRectList.Add(rect);
            }
        }

        #endregion

        #region Public Functions

        public void Draw(DrawingContext drawingContext)
        {
            Draw(drawingContext, false);
        }

        public void Draw(DrawingContext drawingContext, bool withSelection)
        {
            var oldSelection = false;

            foreach (var b in _graphicsList.Cast<GraphicsBase>())
            {
                if (!withSelection)
                {
                    // Keep selection state and unselect
                    oldSelection = b.IsSelected;
                    b.IsSelected = false;
                }

                b.Draw(drawingContext);

                if (!withSelection)
                {
                    // Restore selection state
                    b.IsSelected = oldSelection;
                }
            }
        }

        public void SelectAllGraphics()
        {
            HelperFunctions.SelectAll(this);
            SetActiveRegions();
        }

        public void Delete()
        {
            HelperFunctions.DeleteSelection(this);
        }

        public void ZoomIn()
        {
            foreach (var t in ScaleTable.Where(t => ActualScale < t))
            {
                ActualScale = t;

                //adjust scroll position,always put current point at center

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

        public void MovePostion(double x, double y)
        { }


        public void SetDefault()
        {
            Tool = ToolType.Pointer;
            Cursor = HelperFunctions.DefaultCursor;
        }

        public Rect GetRoomRect(int roomNo)
        {
            return _roomRectList[roomNo - 1];
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
            foreach (var rgn in _slideMod.TotalRegions)
            {
                var rc = rgn.Bound;

                switch (rgn.ScanRegionShape)
                {
                    case RegionShape.Ellipse:
                        var left = (_slideWidth - rc.X - rc.Width) * scale;
                        var top = rc.Y * scale;
                        var right = (_slideWidth - rc.X) * scale;
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
                            ptList[i].X = (_slideWidth - ptList[i].X) * scale;
                            ptList[i].Y = ptList[i].Y * scale;
                        }
                        var polygon = new GraphicsPolygon(ptList, LineWidth, Colors.Green, ActualScale, 0);
                        _graphicsList.Add(polygon);
                        _regionGraphicHashtable.Add(rgn, polygon);
                        polygon.RefreshDrawing();
                        break;
                    case RegionShape.Rectangle:
                        var left1 = (_slideWidth - rc.X - rc.Width) * scale;
                        var top1 = rc.Y * scale;
                        var right1 = (_slideWidth - rc.X) * scale;
                        var bottom1 = (rc.Y + rc.Height) * scale;
                        var rectangle = new GraphicsRectangle(left1, top1, right1, bottom1, LineWidth, Colors.Green, ActualScale, 0);
                        _graphicsList.Add(rectangle);
                        _regionGraphicHashtable.Add(rgn, rectangle);
                        rectangle.RefreshDrawing();
                        break;
                }
            }
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
            var prevCount = _slideMod.ActiveRegions.Count;
            _slideMod.ClearActiveRegions();
            foreach (var rgn in from GraphicsBase o in _graphicsList where o.IsSelected select _regionGraphicHashtable.Keys.OfType<ScanRegion>().FirstOrDefault(s => Equals(_regionGraphicHashtable[s], o)))
            {
                _slideMod.AddActiveRegion(rgn);
            }
            if (_slideMod.ActiveRegions.Count == prevCount && prevCount == 0)
            {
                return;
            }

            using (new WaitCursor())
            {
                //No region event publish to outside.
                //just internal use
                var eventArgs = new List<int>();
                eventArgs.Clear();
                eventArgs.AddRange(_slideMod.ActiveRegions.Select(region => region.RegionId));
                EventAggregator.GetEvent<RegionsSelected>().Publish(eventArgs);

                if (GetBypassMode().Contains(CurrentScanInfo.Mode)) return;

                switch (CarrierModule.Mode)
                {
                    case DisplayMode.Review:
                        eventArgs = new List<int>();
                        eventArgs.Clear();
                        eventArgs.AddRange(_slideMod.ActiveRegions.Select(region => region.RegionId));
                        EventAggregator.GetEvent<SelectRegions>().Publish(eventArgs);
                        break;

                    case DisplayMode.Analysis:
                        eventArgs = new List<int>();
                        eventArgs.Clear();
                        eventArgs.AddRange(_slideMod.ActiveRegions.Select(region => region.WellId));
                        EventAggregator.GetEvent<SelectWells>().Publish(eventArgs);
                        break;
                }
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
                offsetX = scr.ScrollableWidth * (1.0 - p.X / _slideWidth);
            }


            if (double.IsNaN(p.Y))
            {
                offsetY = 0.0;
            }
            else
            {
                offsetY = scr.ScrollableHeight * (p.Y / _slideHeight);
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
                X = (1.0 - scr.HorizontalOffset / scr.ScrollableWidth) * _slideWidth,
                Y = scr.VerticalOffset / scr.ScrollableHeight * _slideHeight
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

            if(cp.X < 0 || cp.Y < 0) return;

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
