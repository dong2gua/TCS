using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using ThorCyte.ImageViewerModule.DrawTools.Tools;
using System.Diagnostics;
namespace ThorCyte.ImageViewerModule.DrawTools
{
    public class DrawingCanvas:Canvas
    {

        public static readonly DependencyProperty ImageSizeProperty = DependencyProperty.Register("ImageSize", typeof(Size), typeof(DrawingCanvas), new PropertyMetadata(new Size(0, 0)));
        public static readonly DependencyProperty ActualScaleProperty = DependencyProperty.Register("ActualScale", typeof(Tuple<double, double, double>), typeof(DrawingCanvas), new PropertyMetadata(new Tuple<double, double, double>(1, 1, 1), OnActualScaleChanged));
        public static readonly DependencyProperty DisplayImageProperty = DependencyProperty.Register("DisplayImage", typeof(Tuple<ImageSource, Int32Rect>), typeof(DrawingCanvas), new PropertyMetadata(new PropertyChangedCallback(OnDisplayImagePropertyChanged)));
        public Size ImageSize
        {
            get { return (Size)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }
        public Tuple<double, double, double> ActualScale
        {
            get { return (Tuple<double, double, double>)GetValue(ActualScaleProperty); }
            set { SetValue(ActualScaleProperty, value); }
        }
        public Tuple<ImageSource, Int32Rect> DisplayImage
        {
            get { return (Tuple<ImageSource, Int32Rect>)GetValue(DisplayImageProperty); }
            set { SetValue(DisplayImageProperty, value); }
        }
        private readonly Tool[] _tools = new Tool[(int)ToolType.Max];
        private ToolType _tool;
        public ToolType Tool
        {
            get
            {
                return _tool;
            }
            set
            {
                if ((int)value < 0 || (int)value >= (int)ToolType.Max||value==_tool) return;
                _tool = value;
                _tools[(int)Tool].SetCursor(this);
                if (Tool != ToolType.Profile && GraphicsList.Contains(graphicsProfile))
                    GraphicsList.Remove(graphicsProfile);
                if (Tool != ToolType.Ruler)
                {
                    if (GraphicsList.Contains(graphicsScaler))
                        GraphicsList.Remove(graphicsScaler);
                    if (GraphicsList.Contains(graphicsRuler))
                        GraphicsList.Remove(graphicsRuler);
                }
                if (Tool == ToolType.Ruler && !GraphicsList.Contains(graphicsScaler))
                {
                    GraphicsList.Add(graphicsScaler);
                    graphicsScaler.ActualScale = ActualScale;
                    graphicsScaler.Point = new Point(20, ActualHeight - 20);
                    graphicsScaler.RefreshDrawing();
                }
            }
        }
        private readonly VisualCollection _graphicsList;
        private GraphicsImage _graphicsImage;
        private GraphicsScaler graphicsScaler;
        private GraphicsRuler graphicsRuler;
        private GraphicsProfile graphicsProfile;
        public Point ZoomMiddlePoint;
        private Rect _tmpCanvasRect;
        private Rect _canvasDisplyRect;
        public Rect CanvasDisplyRect
        {
            get { return _canvasDisplyRect; }
        }
        public GraphicsBase this[int index]
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
        public int Count
        {
            get
            {
                return _graphicsList.Count;
            }
        }
        public int SelectionCount
        {
            get { return _graphicsList.OfType<GraphicsBase>().Count(g => g.IsSelected); }
        }
        public VisualCollection GraphicsList
        {
            get { return _graphicsList; }
        }
        public IEnumerable<GraphicsBase> Selection
        {
            get { return from GraphicsBase o in _graphicsList where o.IsSelected select o; }
        }

        public delegate void MousePointHandler(Point point);
        public event MousePointHandler MousePoint;
        public delegate void UpdateDisplayImageHandler(Rect canvasDisplayRect);
        public event UpdateDisplayImageHandler UpdateDisplayImage;

        private static void OnDisplayImagePropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs e)
        {
            var canvas = property as DrawingCanvas;
            if (canvas == null) return;
            var value = e.NewValue as Tuple<ImageSource, Int32Rect>;
            if (value == null) return;

            canvas._graphicsImage.SetImage(value.Item1, new Rect(value.Item2.X, value.Item2.Y, value.Item2.Width, value.Item2.Height));
            canvas._graphicsImage.RefreshDrawing();
        }
        private static void OnActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var canvas = property as DrawingCanvas;
            if (canvas == null) return;

            var scale = canvas.ActualScale;
            var oldScale = args.OldValue as Tuple<double, double, double>;
            var newScale = args.NewValue as Tuple<double, double, double>;

            double width = canvas.ActualWidth / newScale.Item3/ newScale.Item1;
            double height = canvas.ActualHeight / newScale.Item3 / newScale.Item2;
            double x = (canvas._canvasDisplyRect.X - canvas.ZoomMiddlePoint.X) / oldScale.Item3 * newScale.Item3 + canvas.ZoomMiddlePoint.X;
            double y = (canvas._canvasDisplyRect.Y - canvas.ZoomMiddlePoint.Y) / oldScale.Item3 * newScale.Item3 + canvas.ZoomMiddlePoint.Y;
            canvas._canvasDisplyRect = new Rect(x, y, width, height);
            canvas.TestVirtualBound();
            foreach (var b in canvas.GraphicsList.Cast<GraphicsBase>())
            {
                b.ActualScale = scale;
                b.RefreshDrawing();
                b.Clip = new RectangleGeometry(new Rect(0, 0, canvas.ActualWidth / canvas.ActualScale.Item1, canvas.ActualHeight / canvas.ActualScale.Item2));
            }
            canvas._graphicsImage.ActualScale = scale;
            canvas._graphicsImage.RefreshDrawing();
            canvas._graphicsImage.Clip = new RectangleGeometry(new Rect(0, 0, canvas.ActualWidth / canvas.ActualScale.Item1, canvas.ActualHeight / canvas.ActualScale.Item2));

            if (canvas.UpdateDisplayImage != null)
                canvas.UpdateDisplayImage(canvas._canvasDisplyRect);
        }
        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _canvasDisplyRect.Width = ActualWidth / ActualScale.Item3/ActualScale.Item1;
            _canvasDisplyRect.Height = ActualHeight / ActualScale.Item3 / ActualScale.Item2;
            graphicsScaler.Point = new Point(20, ActualHeight - 20);
            graphicsScaler.RefreshDrawing();
            RefreshVirtualBound();
        }
        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;

            var point = e.GetPosition(this);
            point.X /= ActualScale.Item1;
            point.Y /= ActualScale.Item2;
            _tmpCanvasRect = new Rect(CanvasDisplyRect.Location, CanvasDisplyRect.Size);
            var actualPoint = ConvertToActualPoint(point, _tmpCanvasRect, ActualScale.Item3);
            Focus();
            _tools[(int)Tool].OnMouseDown(this, e, actualPoint);
            e.Handled = true;
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            { }
            var point = e.GetPosition(this);
            point.X /= ActualScale.Item1;
            point.Y /= ActualScale.Item2;
            var actualPoint = Tool == ToolType.Dragger ? ConvertToActualPoint(point, _tmpCanvasRect, ActualScale.Item3) : 
                                                         ConvertToActualPoint(point, CanvasDisplyRect, ActualScale.Item3);
            _tools[(int)Tool].OnMouseMove(this, e, actualPoint);
            if (MousePoint != null) MousePoint(actualPoint);
            e.Handled = true;
        }
        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;

            var point = e.GetPosition(this);
            point.X /= ActualScale.Item1;
            point.Y /= ActualScale.Item2;
            _tmpCanvasRect = new Rect(CanvasDisplyRect.Location, CanvasDisplyRect.Size);
            var actualPoint = ConvertToActualPoint(point, _tmpCanvasRect, ActualScale.Item3);

            _tools[(int)Tool].OnMouseUp(this, e, actualPoint);
            e.Handled = true;
        }
        private void DrawingCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (!IsMouseCaptured) return;
            CancelCurrentOperation();
        }
        private void DrawingCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelCurrentOperation();
                return;
            }
            if (e.Key == Key.Delete)
            {
                DeleteSelection();
            }
            if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SelectAll();
            }

        }

        public DrawingCanvas()
        {
            _graphicsImage = new GraphicsImage(this);
            this.AddVisualChild(_graphicsImage);
            graphicsScaler = new GraphicsScaler();
            _graphicsList = new VisualCollection(this);

            var toolRuler = new ToolRuler();
            graphicsRuler = toolRuler.Ruler;
            var toolProfile = new ToolProfile();
            graphicsProfile = toolProfile.Profile;
            _tools[(int)ToolType.Pointer] = new ToolPointer();
            //_tools[(int)ToolType.Line] = new ToolLine();
            _tools[(int)ToolType.Dragger] = new ToolDragger();
            _tools[(int)ToolType.Rectangle] = new ToolRectangle();
            _tools[(int)ToolType.Ellipse] = new ToolEllipse();
            _tools[(int)ToolType.Ruler] = toolRuler;
            _tools[(int)ToolType.Profile] = toolProfile;

            MouseDown += DrawingCanvas_MouseDown;
            MouseMove += DrawingCanvas_MouseMove;
            MouseUp += DrawingCanvas_MouseUp;
            KeyDown += DrawingCanvas_KeyDown;
            SizeChanged += DrawingCanvas_SizeChanged;
            LostMouseCapture += DrawingCanvas_LostMouseCapture;
            Tool = ToolType.Pointer;
            Focusable = true;
        }
        public void SetPixelSize(double xPixelSize,double yPixelSize)
        {
            graphicsRuler.XPixelSize = xPixelSize;
            graphicsRuler.YPixelSize = yPixelSize;
            graphicsScaler.XPixelSize = xPixelSize;
            graphicsScaler.YPixelSize = yPixelSize;
        }
        protected override int VisualChildrenCount
        {
            get
            {
                var n = _graphicsList.Count;
                if (_graphicsImage != null) n++;

                return n;
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index > 0 && index <= _graphicsList.Count)
                return _graphicsList[index - 1];

            if (_graphicsImage != null && index == 0)
                return _graphicsImage;


            throw new ArgumentOutOfRangeException("index");
        }
        public void Move(double deltaX, double deltaY)
        {
            if (this.SelectionCount <= 0) return;
            double left = ImageSize.Width, right = 0, top = ImageSize.Height, bottom = 0;
            foreach (var o in this.Selection)
            {
                left = Math.Min(left, o.Rectangle.Left);
                right = Math.Max(right, o.Rectangle.Right);
                top = Math.Min(top, o.Rectangle.Top);
                bottom = Math.Max(bottom, o.Rectangle.Bottom);
            }
            if (left + deltaX < 0) deltaX = 0;
            else if (right + deltaX > ImageSize.Width) deltaX = ImageSize.Width - right;
            if (top + deltaY < 0) deltaY = 0;
            else if (bottom + deltaY > ImageSize.Height) deltaY = ImageSize.Height - bottom;
            foreach (var o in this.Selection)
            {
                o.Move(deltaX, deltaY);
            }
        }
        public void Drag(double deltaX, double deltaY)
        {
            _canvasDisplyRect.X -= deltaX;
            _canvasDisplyRect.Y -= deltaY;
            RefreshVirtualBound();
        }
        public void SelectAll()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var g = this[i];
                g.IsSelected = true;
            }
        }
        public void UnselectAll()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this[i].IsSelected = false;
            }
        }
        public void DeleteSelection()
        {

            for (var i = this.Count - 1; i >= 0; i--)
            {
                if (!this[i].IsSelected) continue;
                this.GraphicsList.RemoveAt(i);
            }

        }
        public void DeleteAll()
        {
            if (this.GraphicsList.Count <= 0) return;
            this.GraphicsList.Clear();
        }
        private Point ConvertToActualPoint(Point displayPoint, Rect canvasRect, double scale)
        {
            var x = (displayPoint.X / scale + canvasRect.X) ;
            var y = (displayPoint.Y / scale + canvasRect.Y) ;
            return new Point(x, y);
        }
        private void CancelCurrentOperation()
        {
            if (Tool == ToolType.Pointer)
            {
                if (_graphicsList.Count > 0)
                {
                    if (_graphicsList[_graphicsList.Count - 1] is GraphicsSelectionRectangle)
                    {
                        _graphicsList.RemoveAt(_graphicsList.Count - 1);
                    }
                }
            }
            else if (Tool > ToolType.Pointer && Tool < ToolType.Max)
            {
                if (_graphicsList.Count > 0)
                {
                    _graphicsList.RemoveAt(_graphicsList.Count - 1);
                }
            }

            Tool = ToolType.Pointer;

            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }
        private void TestGraphicsBound(GraphicsBase obj)
        {
            if (_canvasDisplyRect.Width > ImageSize.Width)
                _canvasDisplyRect.X = ImageSize.Width / 2 - _canvasDisplyRect.Width / 2;
            else
            {
                if (_canvasDisplyRect.Left < 0) _canvasDisplyRect.X = 0;
                else if (_canvasDisplyRect.Right > ImageSize.Width) _canvasDisplyRect.X = ImageSize.Width - _canvasDisplyRect.Width;
            }
            if (_canvasDisplyRect.Height > ImageSize.Height)
                _canvasDisplyRect.Y = ImageSize.Height / 2 - _canvasDisplyRect.Height / 2;
            else
            {
                if (_canvasDisplyRect.Top < 0) _canvasDisplyRect.Y = 0;
                else if (_canvasDisplyRect.Bottom > ImageSize.Height) _canvasDisplyRect.Y = ImageSize.Height - _canvasDisplyRect.Height;
            }

        }
        private void RefreshVirtualBound()
        {
            TestVirtualBound();
            foreach (var b in GraphicsList.Cast<GraphicsBase>())
            {
                b.RefreshDrawing();
                b.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth / ActualScale.Item1, ActualHeight / ActualScale.Item2));
            }

            _graphicsImage.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth / ActualScale.Item1, ActualHeight / ActualScale.Item2));
            if (((_canvasDisplyRect.Left < _graphicsImage.Rectangle.Left && _graphicsImage.Rectangle.Left > 0) ||
                (_canvasDisplyRect.Top < _graphicsImage.Rectangle.Top && _graphicsImage.Rectangle.Top > 0) ||
                (_canvasDisplyRect.Right > _graphicsImage.Rectangle.Right && _graphicsImage.Rectangle.Right < ImageSize.Width) ||
                (_canvasDisplyRect.Bottom > _graphicsImage.Rectangle.Bottom && _graphicsImage.Rectangle.Bottom < ImageSize.Height))
                && UpdateDisplayImage != null)
                UpdateDisplayImage(_canvasDisplyRect);
            else { }
                _graphicsImage.RefreshDrawing();

        }
        private void TestVirtualBound()
        {
            if (_canvasDisplyRect.Width > ImageSize.Width)
                _canvasDisplyRect.X = ImageSize.Width / 2 - _canvasDisplyRect.Width / 2;
            else
            {
                if (_canvasDisplyRect.Left < 0)
                    _canvasDisplyRect.X = 0;
                else if (_canvasDisplyRect.Right > ImageSize.Width)
                    _canvasDisplyRect.X = ImageSize.Width - _canvasDisplyRect.Width;
            }
            if (_canvasDisplyRect.Height > ImageSize.Height)
                _canvasDisplyRect.Y = ImageSize.Height / 2 - _canvasDisplyRect.Height / 2;
            else
            {
                if (_canvasDisplyRect.Top < 0)
                    _canvasDisplyRect.Y = 0;
                else if (_canvasDisplyRect.Bottom > ImageSize.Height)
                    _canvasDisplyRect.Y = ImageSize.Height - _canvasDisplyRect.Height;
            }

        }

    }
    public enum ToolType
    {
        None,
        Pointer,
        Dragger,
        Ruler,
        Profile,
        Rectangle,
        Ellipse,
        Line,
        Polygon,
        Text,
        Custom,
        Max
    };


}
