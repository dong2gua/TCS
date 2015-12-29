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
using ThorCyte.CarrierModule.Graphics;
using ThorCyte.CarrierModule.Tools;

namespace ThorCyte.CarrierModule.Canvases
{
    public enum MyPlateTool
    {
        None,
        ActiveRegion,
        Pointer,
        Enable,
        Disable,
        Move
    };

    public class MyPlateCanvas : Canvas
    {
        #region Fileds
        private Microplate _plate;
        private readonly VisualCollection _graphicsList;
        private double _plateWidth = 1.0;
        private double _plateHeight = 1.0;
        private const double PlateMargin = 20;
        private GraphicSelectRectangle _gwSelectRect;
        private bool _bLeftCtl;

        public static readonly DependencyProperty ActualScaleProperty;
        public static readonly DependencyProperty IsSelectRectProperty;
        public MyPlateTool Tool;
        #endregion

        #region Properties
        private static IEventAggregator EventAggregator
        {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }

        public Microplate Plate
        {
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

        public GraphicWell this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                {
                    return (GraphicWell)_graphicsList[index];
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

        public bool IsSelectRect
        {
            get { return (bool)GetValue(IsSelectRectProperty); }
            set { SetValue(IsSelectRectProperty, value); }
        }

        #endregion Properties

        #region Constructors

        static MyPlateCanvas()
        {
            // ActualScale
            var metaData = new PropertyMetadata(
                4.0,
                ActualScaleChanged);

            ActualScaleProperty = DependencyProperty.Register(
                "ActualScale", typeof(double), typeof(MyPlateCanvas),
                metaData);

            metaData = new PropertyMetadata(
                false);

            IsSelectRectProperty = DependencyProperty.Register(
                "IsSelectRect", typeof(bool), typeof(MyPlateCanvas),
                metaData);

        }
        public MyPlateCanvas()
        {
            _graphicsList = new VisualCollection(this);

            _gwSelectRect = new GraphicSelectRectangle(0, 0, 0, 0);
            MouseLeftButtonDown += OnMouseDown;
            MouseLeftButtonUp += OnMouseUp;
            MouseMove += OnMouseMove;
            MouseLeave += OnMouseLeave;
            MouseEnter += OnMouseEnter;
        }


        #endregion Constructors

        #region Methods

        static void ActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var pc = property as MyPlateCanvas;
            if (pc == null) return;
            pc.Width = pc._plateWidth * pc.ActualScale + 20;
            pc.Height = pc._plateHeight * pc.ActualScale + 20;

            var rg = new RectangleGeometry(new Rect(0, 0, pc.Width, pc.Height));

            foreach (var gw in pc._graphicsList.Cast<GraphicWell>())
            {
                gw.Clip = rg;
                gw.ActualScale = pc.ActualScale;
            }
        }

        private void InitPlate()
        {
            if (_graphicsList.Count > 0)
            {
                _graphicsList.Clear();
            }
            var interval = _plate.Interval / 1000.0;
            _plateWidth = interval * _plate.ColumnCount;
            _plateHeight = interval * _plate.RowCount;

            Width = _plateWidth * ActualScale + 20;
            Height = _plateHeight * ActualScale + 20;

            //Draw all plates
            var plateNo = 0;
            for (var row = 0; row < _plate.RowCount; row++)
            {
                for (var col = 0; col < _plate.ColumnCount; col++)
                {
                    var gw = new GraphicWell(plateNo, PlateMargin, interval, row, col);
                    _graphicsList.Add(gw);
                    plateNo++;
                }
            }

        }

        private void DrawGrid(DrawingContext dc)
        {
            var width = _plate.Interval / 1000.0 * ActualScale;
            var pt0 = new Point();
            var pt1 = new Point();
            var pen = new Pen(Brushes.Black, 0.5);

            for (var i = 0; i < _plate.ColumnCount; i++)
            {
                pt0.X = pt1.X = i * width + PlateMargin;
                pt0.Y = 0;
                pt1.Y = Height;
                DrawFunction.DrawLine(dc, pen, pt0, pt1);

                var index = i + 1;
                var formattedText = new FormattedText(index.ToString(CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                12,
                Brushes.Black);

                pt0.X += (width - 12) / 2;
                pt0.Y += (PlateMargin - 12) / 2;
                dc.DrawText(formattedText, pt0);
            }

            for (var i = 0; i < _plate.RowCount; i++)
            {
                pt0.X = 0;
                pt1.X = Width;
                pt0.Y = pt1.Y = i * width + PlateMargin;
                DrawFunction.DrawLine(dc, pen, pt0, pt1);

                var index = i + 65;
                var c = (char)index;
                var formattedText = new FormattedText(c.ToString(CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                12,
                Brushes.Black);

                pt0.X += (PlateMargin - 12) / 2;
                pt0.Y += (width - 12) / 2;
                dc.DrawText(formattedText, pt0);
            }
        }

        private void DrawPannel(DrawingContext dc)
        {
            DrawFunction.DrawRectangle(dc, null, new Pen(Brushes.Black, 0.5), new Rect(0, 0, Width, Height));
            var brush = new SolidColorBrush(Color.FromArgb(255, 47, 96, 127));
            DrawFunction.DrawRectangle(dc, brush, null, new Rect(PlateMargin, PlateMargin, Width - PlateMargin, Height - PlateMargin));
        }

        void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_graphicsList.Contains(_gwSelectRect))
            {   // for mouse leave
                _graphicsList.Remove(_gwSelectRect);
            }

            if (IsSelectRect)
            {
                var pt = e.GetPosition(this);
                _gwSelectRect.Left = pt.X;
                _gwSelectRect.Top = pt.Y;
                _gwSelectRect.Right = pt.X;
                _gwSelectRect.Bottom = pt.Y;
                _graphicsList.Add(_gwSelectRect);
            }

        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsSelectRect || e.LeftButton != MouseButtonState.Pressed) return;
            var pt = e.GetPosition(this);
            _gwSelectRect.Right = pt.X;
            _gwSelectRect.Bottom = pt.Y;
            _gwSelectRect.RefreshDrawing();

            _plate.ClearActiveRegions();
            var pt0 = new Point();
            var index = 1;
            //need modify
            foreach (var visual in _graphicsList.Cast<DrawingVisual>().Where(w => w is GraphicWell))
            {
                var gw = (GraphicWell)visual;
                var rc = gw.WellRect;
                pt0.X = rc.Left + rc.Width / 2;
                pt0.Y = rc.Top + rc.Height / 2;

                if (_gwSelectRect.Rectangle.Contains(pt0))
                {
                    if (gw.IsEnable)
                    {
                        gw.IsSelected = true;
                        _plate.AddActiveRegion(_plate[index]);
                    }
                    else
                    {
                        gw.IsSelected = false;
                    }
                }
                else
                {
                    gw.IsSelected = false;
                }
                index++;
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsSelectRect || e.LeftButton != MouseButtonState.Pressed) return;
            OnMouseUp(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsSelectRect || e.LeftButton != MouseButtonState.Pressed) return;
            var pt = e.GetPosition(this);
            OnMouseDown(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
            var pt = e.GetPosition(this);
            if (IsSelectRect)
            {
                _graphicsList.Remove(_gwSelectRect);
            }
            switch (Tool)
            {
                case MyPlateTool.ActiveRegion:
                    {
                        if (IsSelectRect)
                        {
                            SendActiveRegionChangedEvent();
                        }
                        else
                        {
                            var index = 1;
                            var selectDisableWell = false;
                            var count = _plate.ActiveRegions.Count;
                            foreach (var visual in _graphicsList)
                            {
                                var gw = (GraphicWell)visual;
                                var rc = gw.WellRect;
                                if (rc.Contains(pt))
                                {
                                    if (gw.IsEnable)
                                    {
                                        if (_bLeftCtl)
                                        {
                                            if (gw.IsSelected)
                                            {
                                                gw.IsSelected = false;
                                                _plate.RemoveActiveRegion(_plate[index]);
                                                SendActiveRegionChangedEvent();
                                            }
                                            else
                                            {
                                                gw.IsSelected = true;
                                                _plate.AddActiveRegion(_plate[index]);
                                                SendActiveRegionChangedEvent();
                                            }

                                        }
                                        else
                                        {
                                            gw.IsSelected = true;
                                            _plate.AddSingleActiveRegion(_plate[index]);
                                            SendActiveRegionChangedEvent();
                                        }
                                    }
                                    else
                                    {
                                        gw.IsSelected = false;
                                        selectDisableWell = true;
                                    }
                                }
                                else
                                {
                                    if (!_bLeftCtl)
                                    {
                                        gw.IsSelected = false;
                                    }
                                }
                                index++;
                            }

                            if (selectDisableWell && count != 0)
                            {
                                _plate.ClearActiveRegions();
                                SendActiveRegionChangedEvent();
                            }
                        }
                        break;
                    }

            }
            IsSelectRect = false;
            _gwSelectRect = new GraphicSelectRectangle(0, 0, 0, 0);

        }

        private void SendActiveRegionChangedEvent()
        {
            using (new WaitCursor())
            {
                EventAggregator.GetEvent<ActiveRegionChanged>().Publish(new ActiveRegionChangedEventArgs
                {
                    RegionList = _plate.ActiveRegions
                });
            }
        }

        public void UpdateScanArea()
        {
            for (int i = 1; i < _plate.RegionCount + 1; i++)
            {
                var gw = (GraphicWell)_graphicsList[i - 1];
                gw.IsEnable = true;
            }
        }

        #endregion Methods

        #region Override Functions
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawPannel(dc);
            DrawGrid(dc);
            foreach (var visual in _graphicsList)
            {
                var gw = (GraphicWell)visual;
                gw.ActualScale = ActualScale;
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
        #endregion

    }
}
