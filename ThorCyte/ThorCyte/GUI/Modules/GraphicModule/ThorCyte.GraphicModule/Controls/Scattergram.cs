using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Controls.RegionTools;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Controls
{
    public class Scattergram : RegionCanvas
    {
        #region Fields

        private readonly IList<Tuple<Point, int>> _valueList = new List<Tuple<Point, int>>();
        private readonly GraphicQuarant _graphicQuarant;

        #endregion

        #region Properties

        public Point QuadrantCenterPoint { get; set; }

        public IList<Tuple<Point, int>> ValueList
        {
            get { return _valueList; }
        }

        public GraphicQuarant Quarant
        {
            get { return _graphicQuarant; }
        }

        //public bool IsWhiteBackgroud
        //{
        //    get { return (bool)GetValue(IsWhiteBackgroudProperty); }
        //    set { SetValue(IsWhiteBackgroudProperty, value); }
        //}

        public bool IsShowQuadrant
        {
            get { return (bool)GetValue(IsShowQuadrantProperty); }
            set { SetValue(IsShowQuadrantProperty, value); }
        }

        public bool IsSnap
        {
            get { return (bool)GetValue(IsSnapProperty); }
            set { SetValue(IsSnapProperty, value); }
        }

        #endregion

        #region Dependency Properties

        //public static readonly DependencyProperty IsWhiteBackgroudProperty =
        //    DependencyProperty.Register("IsWhiteBackgroud", typeof(bool), typeof(Scattergram), new PropertyMetadata(BackGroundChanged));

        public static readonly DependencyProperty IsShowQuadrantProperty =
            DependencyProperty.Register("IsShowQuadrant", typeof(bool), typeof(Scattergram), new PropertyMetadata(false, Refresh));

        public static readonly DependencyProperty IsSnapProperty =
            DependencyProperty.Register("IsSnap", typeof(bool), typeof(Scattergram), new PropertyMetadata(false));

        #endregion

        #region Constructors

        public Scattergram()
        {
            QuadrantCenterPoint = new Point(double.NaN, double.NaN);
            // create array of drawing _tools
            Tools = new RegionTool[]
            {
                new RegionToolPointer(),
                new RegionToolRectangle(),
                new RegionToolEllipse(),
                new RegionToolPolygon()
            };
            CurrentTool = Tools[0];
            _graphicQuarant = new GraphicQuarant
            {
                ObjectColor = ObjectColor
            };
            VisualList.Add(_graphicQuarant);
        }

        #endregion Constructor

        #region Draw

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var widthChanged = sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width;
            var heightChanged = sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height;
            _graphicQuarant.ClientRect = new Rect(EndYPoint, RenderSize);
            if (Math.Abs(widthChanged) > double.Epsilon || Math.Abs(heightChanged) > double.Epsilon)
            {
                if (IsShowQuadrant)
                {
                    _graphicQuarant.Draw();
                }
            }
           
        }

        public void Update(List<Point> pointList)
        {
            _graphicQuarant.EventPoints = pointList;
            _graphicQuarant.BaseRect = new Rect(new Point(0,0),RenderSize);
            if (IsShowQuadrant)
            {
                _graphicQuarant.Draw();
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(this);
            if (!IsPointInCanvas(point))
            {
                if (CurrentTool == null && !_graphicQuarant.QuadrantLine.HorizonLine.IsSelected &&
                    !_graphicQuarant.QuadrantLine.VerticalLine.IsSelected)
                {
                    Cursor = Cursors.Arrow;
                }
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_graphicQuarant.QuadrantLine != null)
                {
                    if (_graphicQuarant.QuadrantLine.HorizonLine.IsSelected)
                    {
                        _graphicQuarant.QuadrantLine.HorizonLine.Start = new Point(point.X, OriginalPoint.Y);
                        _graphicQuarant.QuadrantLine.HorizonLine.End = new Point(point.X, EndYPoint.Y);
                        _graphicQuarant.Draw();
                        return;
                    }
                    if (_graphicQuarant.QuadrantLine.VerticalLine.IsSelected)
                    {
                        _graphicQuarant.QuadrantLine.VerticalLine.Start = new Point(OriginalPoint.X, point.Y);
                        _graphicQuarant.QuadrantLine.VerticalLine.End = new Point(EndXPoint.X, point.Y);
                        _graphicQuarant.Draw();
                        return;
                    }
                }
            }
            if (e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                CurrentTool.OnMouseMove(this, e);
            }
            else
            {
                Cursor = Cursors.None;
            }
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            if (_isLoading)
            {
                InitGraphics();
                _graphicQuarant.BaseRect = new Rect(new Point(0, 0), RenderSize);
                SetQuadrantPosition();
            }
            _isLoading = false;
        }

        #endregion

        #region Methods

        protected override void SetRegionCommonParas(MaskRegion region)
        {
            var scatterVm = Vm as ScattergramVm;
            if (scatterVm == null)
            {
                return;
            }
            RegionHelper.Set2DCommonRegionParas(region,scatterVm);
        }

        protected override void Refresh()
        {
            base.Refresh();
            if (_graphicQuarant.QuadrantLine != null)
            {
                _graphicQuarant.IsShow = IsShowQuadrant;
            }
        }

        public void SetQuadrantPosition()
        {
            var centerX = (OriginalPoint.X + EndXPoint.X) / 2;
            var centerY = (OriginalPoint.Y + EndYPoint.Y) / 2;
            var horline = new GraphicsLine(new Point(centerX, OriginalPoint.Y),
                new Point(centerX, EndYPoint.Y), 2.0, ObjectColor, 1.0, LineType.Horizon);
            var verline = new GraphicsLine(new Point(OriginalPoint.X, centerY),
                new Point(EndXPoint.X, centerY), 2.0, ObjectColor, 1.0, LineType.Vertical);

            if (!double.IsNaN(QuadrantCenterPoint.X) && !double.IsNaN(QuadrantCenterPoint.Y))
            {
                var x = (ConstantHelper.DefaultOriginalX + QuadrantCenterPoint.X) * XScale;
                horline.Start = new Point(x, OriginalPoint.Y);
                horline.End = new Point(x, EndYPoint.Y);

                var y = (ConstantHelper.DefaultY + QuadrantCenterPoint.Y) * XScale;
                verline.Start = new Point(OriginalPoint.X, y);
                verline.End = new Point(EndXPoint.X, y);
            }
            if (_graphicQuarant.QuadrantLine == null)
            {
                _graphicQuarant.QuadrantLine = new RegionToolLine(horline, verline, this);
            }
            _graphicQuarant.IsShow = IsShowQuadrant;
        }

        #endregion

        #region CallBacks

        //private static void BackGroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var canvas = d as Scattergram;
        //    if (canvas != null)
        //    {
        //        canvas.UnSelectAll();
        //        var color = Colors.Black;
        //        if ((bool)e.NewValue)
        //        {
        //            canvas.DrawBrush = Brushes.Black;
        //        }
        //        else
        //        {
        //            color = Colors.White;
        //            canvas.DrawBrush = Brushes.White;
        //        }

        //        canvas.ObjectColor = color;
        //        canvas._graphicQuarant.ObjectColor = color;

        //        foreach (var child in canvas.VisualList)
        //        {
        //            if (!(child is GraphicsBase))
        //            {
        //                continue;
        //            }
        //            var graphic = (GraphicsBase)child;
        //            if (graphic.GraphicType == RegionType.Line)
        //            {
        //                graphic.ObjectColor = color;
        //                continue;
        //            }
        //            var region = ROIManager.Instance.GetRegion(graphic.Name);
        //            if (region != null)
        //            {
        //                if ((bool)e.NewValue)
        //                {
        //                    graphic.ObjectColor = region.Color == Colors.White ? color : region.Color;
        //                }
        //                else
        //                {
        //                    graphic.ObjectColor = region.Color == Colors.Black ? color : region.Color;
        //                }
        //            }
        //        }
        //        if (canvas.IsShowQuadrant)
        //        {
        //            canvas._graphicQuarant.Draw();
        //        }
        //    }
        //}

        #endregion
    }
}
