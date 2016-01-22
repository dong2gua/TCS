using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Controls.RegionTools;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Controls
{
    public abstract class RegionCanvas : Canvas
    {
        #region Fields

        public double DefaultWidth;

        public double DefaultHeight;

        protected const double DefaultCoordinateLineWidth = 1.0;

        protected const double DefaultTickLineWidth = 1.0;

        protected const double DefaultPowerFontSize = 8.0;

        protected const double DefaultValueFontSize = 10.0;

        protected const double DefaultTitleFontSize = 12.0;

        protected const double DefaulRegiontFontSize = 12.0;

        protected const double DefaultAxisTitleFontSize = 11.0;

        protected double XvalueHeight = double.NaN;

        protected double YvalueHeight = double.NaN;

        protected Brush DrawBrush = new SolidColorBrush(Colors.Black);

        protected RegionTool[] Tools; // Array of _tools

        protected RegionTool CurrentTool;

        private readonly VisualCollection _visualList;

        protected bool _isLoading = true;

        #endregion

        #region Properties

        public double LineWidth
        {
            get { return 1.0; }
        }

        /// <summary>
        /// Return list of graphics
        /// </summary>
        public VisualCollection VisualList
        {
            get { return _visualList; }
        }

        protected override int VisualChildrenCount
        {
            get { return _visualList.Count; }
        }

        public virtual double XScale
        {
            get
            {
                return Math.Abs(ActualWidth) < double.Epsilon ? 1.0 : ConstantHelper.LowBinCount / (_vm.XAxis.MaxValue - _vm.XAxis.MinValue) /
                 (ConstantHelper.LowBinCount / ActualWidth);
            }
        }

        public virtual double YScale
        {
            get
            {
                return Math.Abs(ActualHeight) < double.Epsilon ? 1.0 : ConstantHelper.LowBinCount / (_vm.YAxis.MaxValue - _vm.YAxis.MinValue) /
                    (ConstantHelper.LowBinCount / ActualHeight);
            }
        }

        public Point EndXPoint
        {
            get { return new Point(ActualWidth, ActualHeight); }
        }

        private readonly Point _endYPoint = new Point(0, 0);

        public Point EndYPoint
        {
            get { return _endYPoint; }
        }

        public Point OriginalPoint
        {
            get { return new Point(0, ActualHeight); }
        }

        private GraphicVmBase _vm;

        public GraphicVmBase Vm
        {
            get { return _vm; }
        }

        /// <summary>
        /// Get graphic object by index
        /// </summary>
        public GraphicsBase this[int index]
        {
            get
            {
                if (!(_visualList[index] is GraphicsBase))
                {
                    return null;
                }
                if (index >= 0 && index < _visualList.Count)
                {
                    return (GraphicsBase)_visualList[index];
                }
                return null;
            }
        }

        /// <summary>
        /// Returns INumerable which may be used for enumeration
        /// of _isSelected objects.
        /// </summary>
        internal IEnumerable<GraphicsBase> Selection
        {
            get
            {
                foreach (var o in _visualList)
                {
                    if (!(o is GraphicsBase))
                    {
                        continue;
                    }
                    var g = (GraphicsBase)o;
                    if (g.IsSelected)
                    {
                        yield return g;
                    }
                }
            }
        }

        public string Id { get; set; }

        private Color _objectColor = Colors.Black;

        public Color ObjectColor
        {
            get { return _objectColor; }
            protected set { _objectColor = value; }
        }

        /// <summary>
        /// Currently active drawing tool
        /// </summary>
        /// 
        public ToolType Tool
        {
            get { return (ToolType)GetValue(ToolProperty); }
            set { SetValue(ToolProperty, value); }
        }

        public Color RegionColor
        {
            get { return (Color)GetValue(RegionColorProperty); }
            set { SetValue(RegionColorProperty, value); }
        }

        public static readonly DependencyProperty ToolProperty =
            DependencyProperty.Register("Tool", typeof(ToolType), typeof(RegionCanvas), new PropertyMetadata(ToolType.Pointer, ToolTypeChaned));

        public static readonly DependencyProperty RegionColorProperty =
            DependencyProperty.Register("RegionColor", typeof(Color), typeof(RegionCanvas), new PropertyMetadata(Colors.White, RegionColorChanged));

        #endregion

        #region Constructor

        protected RegionCanvas()
        {
            Focusable = true;
            _visualList = new VisualCollection(this);
            PreviewKeyDown += OnKeyDown;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            Loaded += OnLoaded;
        }

        #endregion


        #region Callback

        private static void ToolTypeChaned(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var type = (ToolType)e.NewValue;
            var canvas = d as RegionCanvas;
            if (canvas != null)
            {
                if (canvas.Tools != null)
                {
                    var toolIndex = (int)type;
                    if (toolIndex >= canvas.Tools.Length)
                    {
                        return;
                    }
                    canvas.CurrentTool = canvas.Tools[toolIndex];
                }
            }
        }

        protected static void Refresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = d as RegionCanvas;
            if (graph != null)
            {
                graph.Refresh();
            }
        }
        
        private static void RegionColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var graph = (RegionCanvas)d;
            var list = new List<MaskRegion>();
            foreach (var graphic in graph.VisualList)
            {
                if (!(graphic is GraphicsBase))
                {
                    continue;
                }
                var g = (GraphicsBase)graphic;
                if (g.GraphicType == RegionType.Line)
                {
                    continue;
                }
                if (g.IsSelected)
                {
                    if (g.ObjectColor == color)
                    {
                        continue;
                    }
                    g.ObjectColor = color;
                    var region = ROIManager.Instance.GetRegion(g.Name);
                    region.Color = color;
                    list.Add(region);
                }
            }
            if (list.Count > 0)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(graph.Id, list, RegionUpdateType.Color));
            }
        }

        #endregion

        #region Methods

        public void AddRegion(GraphicsBase graphic)
        {
            var region = GetRegion(graphic);
            if (region == null)
            {
                return;
            }
            var list = new List<MaskRegion>
            {
                region
            };
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(Id, list, RegionUpdateType.Add));
        }

        protected virtual void SetRegionCommonParas(MaskRegion region)
        {
            RegionHelper.SetCommonRegionParas(region, _vm);
        }
        
        private MaskRegion GetRegion(GraphicsBase graphic)
        {
            MaskRegion region = null;
            var size = new Size();
            var id = int.Parse(graphic.Name.Remove(0, 1));
            var point = new Point(0, 0);
            if (graphic.GraphicType == RegionType.Ellipse)
            {
                region = new EllipseRegion(id,size, point);
            }
            else if (graphic.GraphicType == RegionType.Rectangle)
            {
                region = new RectangleRegion(id,size, point);
            }
            else if (graphic.GraphicType == RegionType.Polygon)
            {
                region = new PolygonRegion(id,new List<Point>());
            }
            if (region == null)
                return null;
            RegionHelper.UpdateRegionLocation(region, graphic, this, _vm);
            region.GraphicId = Id;
            region.Color = graphic.ObjectColor;
            region.ComponentName = _vm.SelectedComponent;
            SetRegionCommonParas(region);
            RegionHelper.SetCommonRegionParas(region,_vm);
            region.LeftParent = (!string.IsNullOrEmpty(_vm.SelectedGate1) && _vm.SelectedGate1.StartsWith(ConstantHelper.PrefixRegionName)) ? _vm.SelectedGate1 : string.Empty;
            region.RightParent = (!string.IsNullOrEmpty(_vm.SelectedGate2) && _vm.SelectedGate2.StartsWith(ConstantHelper.PrefixRegionName)) ? _vm.SelectedGate2 : string.Empty;
            return region;
        }

        protected virtual void Refresh()
        {
            InvalidateVisual();
        }

        protected override Visual GetVisualChild(int index)
        {
            return _visualList[index];
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateGraphics();
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            DefaultHeight = ActualHeight;
            DefaultWidth = ActualWidth;
            _vm = DataContext as GraphicVmBase;
        }

        private void OnKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelected();
            }

            if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SelectAll();
            }
            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // special case for GraphicsText
                }
                else
                {
                    if ((Tool == ToolType.Polygon) || (Tool == ToolType.Rectangle) || (Tool == ToolType.Ellipse))
                    {

                    }

                    CurrentTool.OnMouseDown(this, e);
                    foreach (var g in VisualList)
                    {
                        if (!(g is GraphicsBase))
                        {
                            continue;
                        }
                        var graphic = (GraphicsBase)g;
                        if (graphic.GraphicType == RegionType.Line)
                        {
                            continue;
                        }
                        if (graphic.IsSelected)
                        {
                            if (RegionColor != graphic.ObjectColor)
                            {
                                RegionColor = graphic.ObjectColor;
                                break;
                            }
                        }
                    }
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (Tool == ToolType.Polygon)
                {
                    CurrentTool.OnMouseDown(this, e);
                }
            }
        }

        public bool IsPointInCanvas(Point pt)
        {
            return (pt.X >= OriginalPoint.X && pt.X <= EndXPoint.X) && (pt.Y >= EndYPoint.Y && pt.Y <= OriginalPoint.Y);
        }

        protected void UpdateGraphics()
        {
            foreach (var child in _visualList)
            {
                var graphic = child as GraphicsBase;
                if (graphic == null)
                    continue;

                var rect = new Rect(_endYPoint, RenderSize);
                graphic.XScale = ActualWidth / graphic.CreatedCanvasSize.Width;
                graphic.YScale = ActualHeight / graphic.CreatedCanvasSize.Height;
                graphic.Clip = new RectangleGeometry(rect);
            }
        }

        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(this);
            if (!IsPointInCanvas(point))
            {
                return;
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

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
            if (e.ChangedButton == MouseButton.Left)
            {
                CurrentTool.OnMouseUp(this, e);
            }

            if ((Tool == ToolType.Polygon) && (e.ChangedButton == MouseButton.Right))
            {
                CurrentTool.OnMouseUp(this, e);
            }
        }

        private void DeleteSelected()
        {
            var count = _visualList.Count - 1;
            var list = new List<MaskRegion>();
            for (var i = count; i >= 0; i--)
            {
                if (this[i] == null)
                {
                    continue;
                }
                if (this[i].IsSelected)
                {
                    var region = ROIManager.Instance.GetRegion(this[i].Name);
                    list.Add(region);
                    _visualList.RemoveAt(i);
                }
            }
            if (list.Count > 0)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(Id, list, RegionUpdateType.Delete));
            }
        }

        private void SelectAll()
        {
            foreach (var child in _visualList)
            {
                var graphic = child as GraphicsBase;
                if (graphic != null && graphic.GraphicType != RegionType.Line)
                {
                    graphic.IsSelected = true;
                }
            }
        }

        protected void InitGraphics()
        {
            var ids = ROIManager.Instance.GetRegionIdList();
            foreach (var id in ids)
            {
                var region = ROIManager.Instance.GetRegion(id);
                if (region != null && region.GraphicId == Id)
                {
                    GraphicsBase graphic;
                    if (region is RectangleRegion)
                    {
                        var rect = (RectangleRegion)region;
                        var left = new Point(rect.LeftUp.X * XScale, ActualHeight - rect.LeftUp.Y * YScale);
                        var isDrawTrackerAll = (this as Scattergram) != null;
                        graphic = new GraphicsRectangle(left.X, left.Y,
                            left.X + rect.Size.Width * XScale, left.Y + rect.Size.Height * YScale, 2.0, rect.Color, 1.0,
                            RenderSize, id, isDrawTrackerAll);

                    }
                    else if (region is EllipseRegion)
                    {
                        var ellipse = (EllipseRegion)region;
                        var width = ellipse.Axis.Width * XScale;
                        var height = ellipse.Axis.Height * YScale;
                        var center = new Point(ellipse.Center.X * XScale, ellipse.Center.Y * YScale);
                        graphic = new GraphicsEllipse(center.X - width / 2.0, center.Y - height / 2.0,
                            center.X + width / 2.0, center.Y + height / 2.0, 2.0, ellipse.Color, 1.0,
                            RenderSize, id);

                    }
                    else
                    {
                        var polygon = (PolygonRegion)region;
                        var list = new List<Point>();
                        foreach (var p in polygon.Vertex)
                        {
                            list.Add(new Point(p.X * XScale, ActualHeight - p.Y * YScale));
                        }
                        graphic = new GraphicsPolygon(list.ToArray(), 2.0, polygon.Color,
                            RenderSize, id);
                    }

                    graphic.FontSize = 12;
                    graphic.CreatedCanvasSize = RenderSize;
                    graphic.Clip = new RectangleGeometry(new Rect(_endYPoint, RenderSize));
                    _visualList.Add(graphic);
                }
            }
        }

        public void UnSelectAll()
        {
            foreach (var child in _visualList)
            {
                var graphic = child as GraphicsBase;
                if (graphic != null)
                {
                    graphic.IsSelected = false;
                }
            }
        }


        public GraphicsBase GetGraphic(string id)
        {
            GraphicsBase graphicBase = null;
            foreach (var g in _visualList)
            {
                var graphic = g as GraphicsBase;
                if (graphic != null && !string.IsNullOrEmpty(graphic.Name) && graphic.Name.StartsWith(ConstantHelper.PrefixRegionName))
                {
                    if (graphic.Name == id)
                    {
                        graphicBase = graphic;
                    }
                }
            }
            return graphicBase;
        }
        #endregion
    }
}
