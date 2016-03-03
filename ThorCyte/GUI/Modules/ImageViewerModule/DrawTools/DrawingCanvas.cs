using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using ThorCyte.ImageViewerModule.DrawTools.Tools;

namespace ThorCyte.ImageViewerModule.DrawTools
{
    public class DrawingCanvas : Canvas
    {
        public static readonly DependencyProperty ImageSizeProperty = DependencyProperty.Register("ImageSize", typeof(Size), typeof(DrawingCanvas), new PropertyMetadata(new Size(0, 0)));
        public static readonly DependencyProperty DisplayImageProperty = DependencyProperty.Register("DisplayImage", typeof(Tuple<ImageSource, Int32Rect>), typeof(DrawingCanvas), new PropertyMetadata(new PropertyChangedCallback(OnDisplayImagePropertyChanged)));
        public Size ImageSize
        {
            get { return (Size)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }
        public Tuple<ImageSource, Int32Rect> DisplayImage
        {
            get { return (Tuple<ImageSource, Int32Rect>)GetValue(DisplayImageProperty); }
            set { SetValue(DisplayImageProperty, value); }
        }
        private Tuple<double, double, double> _actualScale = new Tuple<double, double, double>(1, 1, 1);
        public Tuple<double, double, double> ActualScale
        {
            get { return _actualScale; }
            set
            {
                if (value == _actualScale) return;
                _actualScale = value;
            }
        }
        private bool _isShowScaler;
        public bool IsShowScaler
        {
            get { return _isShowScaler; }
            set
            {
                if (value == _isShowScaler) return;
                _isShowScaler = value;
                refreshScaler();
            }
        }
        private bool _isShowThumbnail;
        public bool IsShowThumbnail
        {
            get { return _isShowThumbnail; }
            set
            {
                if (value == _isShowThumbnail) return;
                _isShowThumbnail = value;
                refreshThumbnail();
            }
        }
        private readonly Tool[] _tools = new Tool[(int)ToolType.Max];
        private ToolType _tool;
        public ToolType Tool
        {
            get { return _tool; }
            set
            {
                if ((int)value < 0 || (int)value >= (int)ToolType.Max || value == _tool) return;
                _tool = value;
                _tools[(int)Tool].SetCursor(this);
                if (Tool != ToolType.Profile && GraphicsList.Contains(graphicsProfile))
                    GraphicsList.Remove(graphicsProfile);
                if (Tool != ToolType.Ruler && GraphicsList.Contains(graphicsRuler))
                    GraphicsList.Remove(graphicsRuler);
            }
        }
        private readonly VisualCollection _graphicsList;
        private GraphicsImage _graphicsImage;
        private GraphicsScaler graphicsScaler;
        private GraphicsThumbnail graphicsThumbnail;
        private GraphicsRuler graphicsRuler;
        private GraphicsLine graphicsProfile;
        private Rect _tmpCanvasRect;
        private Rect _canvasDisplyRect;
        private Point _zoomPoint;
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
        public delegate Task UpdateDisplayImageHandler(Rect canvasDisplayRect);
        public event UpdateDisplayImageHandler UpdateDisplayImage;
        public delegate Task ZoomHandler(int delta);
        public event ZoomHandler Zoom;

        private static void OnDisplayImagePropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs e)
        {
            var canvas = property as DrawingCanvas;
            if (canvas == null) return;
            var value = e.NewValue as Tuple<ImageSource, Int32Rect>;
            if (value == null)
            {
                canvas._graphicsImage.SetImage(null, new Rect(0, 0, 0, 0));
                canvas._graphicsImage.RefreshDrawing();
                return;
            }
            canvas._graphicsImage.SetImage(value.Item1, new Rect(value.Item2.X, value.Item2.Y, value.Item2.Width, value.Item2.Height));
            canvas._graphicsImage.RefreshDrawing();
            canvas.refreshThumbnail();
            canvas.refreshScaler();
        }
        public void SetActualScaleOnly(double scale1, double scale2, double scale3)
        {
            ActualScale = new Tuple<double, double, double>(scale1, scale2, scale3);
            var ZoomMiddlePoint = _zoomPoint;
            double width = ActualWidth / ActualScale.Item3 / ActualScale.Item1;
            double height = ActualHeight / ActualScale.Item3 / ActualScale.Item2;
            double x = (ImageSize.Width-width) / 2;
            double y = (ImageSize.Height - height) / 2;
            _canvasDisplyRect = new Rect(x, y, width, height);
            TestVisualBound();
            foreach (var b in GraphicsList.Cast<GraphicsBase>())
            {
                b.ActualScale = ActualScale;
                b.RefreshDrawing();
                b.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
            }
            _graphicsImage.ActualScale = ActualScale;
            _graphicsImage.RefreshDrawing();
            refreshThumbnail();
            refreshScaler();
            _graphicsImage.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth / ActualScale.Item1, ActualHeight / ActualScale.Item2));
        }

        public async Task SetActualScale(double scale1, double scale2, double scale3)
        {
            var oldScale = ActualScale;
            var newScale = new Tuple<double, double, double>(scale1, scale2, scale3);
            ActualScale = newScale;
            var ZoomMiddlePoint = _zoomPoint;
            double width = ActualWidth / newScale.Item3 / newScale.Item1;
            double height = ActualHeight / newScale.Item3 / newScale.Item2;
            double x = Math.Round(ZoomMiddlePoint.X - (ZoomMiddlePoint.X - _canvasDisplyRect.X) * oldScale.Item3 * oldScale.Item1 / newScale.Item3 / newScale.Item1);
            double y = Math.Round(ZoomMiddlePoint.Y - (ZoomMiddlePoint.Y - _canvasDisplyRect.Y) * oldScale.Item3 * oldScale.Item2 / newScale.Item3 / newScale.Item2);
            _canvasDisplyRect = new Rect(x, y, width, height);
            TestVisualBound();

            foreach (var b in GraphicsList.Cast<GraphicsBase>())
            {
                b.ActualScale = ActualScale;
                b.RefreshDrawing();
                b.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
            }
            _graphicsImage.ActualScale = ActualScale;
            _graphicsImage.RefreshDrawing();
            refreshThumbnail();
            refreshScaler();
            _graphicsImage.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth / ActualScale.Item1, ActualHeight / ActualScale.Item2));

            if (oldScale.Item3 == newScale.Item3 && _graphicsImage.Rectangle.Contains(CanvasDisplyRect))
                return;
            if (UpdateDisplayImage != null)
                await UpdateDisplayImage(_canvasDisplyRect);
        }
        private async void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _canvasDisplyRect.Width = ActualWidth / ActualScale.Item3 / ActualScale.Item1;
            _canvasDisplyRect.Height = ActualHeight / ActualScale.Item3 / ActualScale.Item2;
            await RefreshVisualBound();
            graphicsScaler.Point = getScalerPoint();
            graphicsScaler.RefreshDrawing();
            graphicsThumbnail.Vector = getThumbnailVector();
            graphicsThumbnail.RefreshDrawing();
        }
        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;
            var point = e.GetPosition(this);
            _tmpCanvasRect = new Rect(CanvasDisplyRect.Location, CanvasDisplyRect.Size);
            var actualPoint = ConvertToActualPoint(point, _tmpCanvasRect);
            Focus();
            _tools[(int)Tool].OnMouseDown(this, e, actualPoint);
            e.Handled = true;
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;
            var point = e.GetPosition(this);
            var actualPoint = Tool == ToolType.Dragger ? ConvertToActualPoint(point, _tmpCanvasRect) :
                                                         ConvertToActualPoint(point, CanvasDisplyRect);
            _tools[(int)Tool].OnMouseMove(this, e, actualPoint);
            if (MousePoint != null) MousePoint(actualPoint);
            e.Handled = true;
        }
        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;

            var point = e.GetPosition(this);
            var actualPoint = ConvertToActualPoint(point, CanvasDisplyRect);

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
        private async void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var point = e.GetPosition(this);
            _zoomPoint = ConvertToActualPoint(point, CanvasDisplyRect);
            if (Zoom != null)
                await Zoom(e.Delta);
        }

        public DrawingCanvas()
        {
            _graphicsImage = new GraphicsImage(this);
            graphicsScaler = new GraphicsScaler(this);
            graphicsThumbnail = new GraphicsThumbnail(this);
            this.AddVisualChild(_graphicsImage);
            this.AddVisualChild(graphicsScaler);
            this.AddVisualChild(graphicsThumbnail);
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
            MouseWheel += DrawingCanvas_MouseWheel;
            SizeChanged += DrawingCanvas_SizeChanged;
            LostMouseCapture += DrawingCanvas_LostMouseCapture;
            Tool = ToolType.Pointer;
            Focusable = true;
        }
        public void SetPixelSize(double xPixelSize, double yPixelSize)
        {
            graphicsRuler.XPixelSize = xPixelSize;
            graphicsRuler.YPixelSize = yPixelSize;
            graphicsScaler.XPixelSize = xPixelSize;
            graphicsScaler.YPixelSize = yPixelSize;
        }
        public void SetZoomPoint(double sx, double sy)
        {
            var x = CanvasDisplyRect.Width * sx + CanvasDisplyRect.X;
            var y = CanvasDisplyRect.Height * sy + CanvasDisplyRect.Y;
            _zoomPoint = new Point(x, y);
        }
        protected override int VisualChildrenCount
        {
            get
            {
                var n = _graphicsList.Count;
                if (_graphicsImage != null) n++;
                if (graphicsScaler != null) n++;
                if (graphicsThumbnail != null) n++;
                return n;
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            if (_graphicsImage != null && index == 0)
                return _graphicsImage;
            if (graphicsScaler != null && index == 1)
                return graphicsScaler;
            if (graphicsThumbnail != null && index == 2)
                return graphicsThumbnail;

            if (index >= 0 + 3 && index < _graphicsList.Count + 3)
                return _graphicsList[index - 3];
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
        public async Task Drag(double deltaX, double deltaY)
        {
            _canvasDisplyRect.X -= deltaX;
            _canvasDisplyRect.Y -= deltaY;
            await RefreshVisualBound();
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
        public void DeleteItem(string item)
        {
            switch (item)
            {
                case "Profile":
                    if (GraphicsList.Contains(graphicsProfile))
                        GraphicsList.Remove(graphicsProfile);
                    break;
                case "Ruler":
                    if (GraphicsList.Contains(graphicsRuler))
                        GraphicsList.Remove(graphicsRuler);
                    break;
                default:
                    break;
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
            _graphicsImage.SetImage(null, new Rect(0, 0, 0, 0));
            _graphicsImage.RefreshDrawing();
            if (this.GraphicsList.Count <= 0) return;
            this.GraphicsList.Clear();
        }
        private Point ConvertToActualPoint(Point displayPoint, Rect canvasRect)
        {
            displayPoint.X /= ActualScale.Item1;
            displayPoint.Y /= ActualScale.Item2;
            var x = (displayPoint.X / ActualScale.Item3 + canvasRect.X);
            var y = (displayPoint.Y / ActualScale.Item3 + canvasRect.Y);
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
        private async Task RefreshVisualBound()
        {
            TestVisualBound();
            foreach (var b in GraphicsList.Cast<GraphicsBase>())
            {
                b.RefreshDrawing();
                b.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
            }

            _graphicsImage.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth / ActualScale.Item1, ActualHeight / ActualScale.Item2));
            _graphicsImage.RefreshDrawing();
            refreshThumbnail();
            refreshScaler();
            if (((_canvasDisplyRect.Left < _graphicsImage.Rectangle.Left && _graphicsImage.Rectangle.Left > 0) ||
                (_canvasDisplyRect.Top < _graphicsImage.Rectangle.Top && _graphicsImage.Rectangle.Top > 0) ||
                (_canvasDisplyRect.Right > _graphicsImage.Rectangle.Right && _graphicsImage.Rectangle.Right < ImageSize.Width) ||
                (_canvasDisplyRect.Bottom > _graphicsImage.Rectangle.Bottom && _graphicsImage.Rectangle.Bottom < ImageSize.Height))
                && UpdateDisplayImage != null)
                await UpdateDisplayImage(_canvasDisplyRect);
        }
        private void TestVisualBound()
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
        private Point getScalerPoint()
        {
            var x = Math.Max(_graphicsImage.DisplayRectangle.Left, 0);
            var y = Math.Min(_graphicsImage.DisplayRectangle.Bottom, ActualHeight);
            return new Point(x + 20, y - 20);
        }
        private Vector getThumbnailVector()
        {
            var x = Math.Min(_graphicsImage.DisplayRectangle.Right, ActualWidth);
            var y = Math.Max(_graphicsImage.DisplayRectangle.Top, 0);
            return new Vector(x - 20, y + 20);
        }
        private void refreshScaler()
        {
            graphicsScaler.ActualScale = ActualScale;
            graphicsScaler.Point = getScalerPoint();
            graphicsScaler.RefreshDrawing();
        }
        private void refreshThumbnail()
        {
            graphicsThumbnail.ActualScale = ActualScale;
            graphicsThumbnail.Vector = getThumbnailVector();
            graphicsThumbnail.RefreshDrawing();
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
