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

        public static readonly DependencyProperty DisplayImageProperty = DependencyProperty.Register("DisplayImage", typeof(ImageSource), typeof(DrawingCanvas), new PropertyMetadata(new PropertyChangedCallback(OnDisplayImagePropertyChangedCallback)));
        public static readonly DependencyProperty ToolProperty = DependencyProperty.Register("Tool", typeof(ToolType), typeof(DrawingCanvas), new PropertyMetadata(ToolType.Pointer));
        public static readonly DependencyProperty ActualScaleProperty = DependencyProperty.Register("ActualScale", typeof(Tuple<double, double, double>), typeof(DrawingCanvas), new PropertyMetadata(new Tuple<double, double, double>(1, 1,1), OnActualScaleChanged));
        public static readonly DependencyProperty PointProperty = DependencyProperty.Register("Point", typeof(Point), typeof(DrawingCanvas), new PropertyMetadata(new Point(0, 0)));
        public ImageSource DisplayImage
        {
            get { return (ImageSource)GetValue(DisplayImageProperty); }
            set { SetValue(DisplayImageProperty, value); }
        }
        public ToolType Tool
        {
            get
            {
                return (ToolType)GetValue(ToolProperty);
            }
            set
            {
                if ((int)value < 0 || (int)value >= (int)ToolType.Max) return;
                SetValue(ToolProperty, value);
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
                    graphicsScaler.Point = new Point(20, ActualHeight - 20);
                    graphicsScaler.RefreshDrawing();
                }
            }
        }
        public Tuple<double, double,double> ActualScale
        {
            get { return (Tuple<double, double, double>)GetValue(ActualScaleProperty); }
            set { SetValue(ActualScaleProperty, value); }
        }
        public Point Point
        {
            get
            {
                return (Point)GetValue(PointProperty);
            }
            set
            {
                SetValue(PointProperty, value);
            }
        }

        private GraphicsImage _graphicsImage;
        public GraphicsImage GraphicsImage
        {
            get { return _graphicsImage; }
        }
        private GraphicsScaler graphicsScaler;

        private readonly VisualCollection _graphicsList;
        private GraphicsRuler graphicsRuler;
        private GraphicsProfile graphicsProfile;

        private static void OnDisplayImagePropertyChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs e)
        {
            var canvas = property as DrawingCanvas;
            if (canvas == null) return;
            
            var scale = canvas.ActualScale;
            var imageSource = e.NewValue as ImageSource;
            if (imageSource == null) return;
            canvas.LimitX = imageSource.Width > canvas.ActualWidth / scale.Item1 ? 0 : (canvas.ActualWidth / scale.Item1 - imageSource.Width) / 2;
            canvas.LimitY = imageSource.Height > canvas.ActualHeight / scale.Item2 ? 0 : (canvas.ActualHeight / scale.Item2 - imageSource.Height) / 2;
            double rectX, rectY;
            if(canvas._graphicsImage.Rectangle.Width==imageSource.Width&& canvas._graphicsImage.Rectangle.Height == imageSource.Height)
            {
                rectX =canvas._graphicsImage.Rectangle.X;
                rectY = canvas._graphicsImage.Rectangle.Y;
            }
            else
            {
                rectX = imageSource.Width > canvas.ActualWidth / scale.Item1 ? (-(imageSource.Width - canvas.ActualWidth / scale.Item1) / 2) : canvas.LimitX;
                rectY = imageSource.Height > canvas.ActualHeight / scale.Item2 ? (-(imageSource.Height - canvas.ActualHeight / scale.Item2) / 2) :canvas.LimitY;
            }
            canvas._graphicsImage.SetImage(imageSource, new Rect(rectX, rectY, imageSource.Width, imageSource.Height));
            canvas._graphicsImage.Clip = new RectangleGeometry(new Rect(canvas.LimitX, canvas.LimitY, Math.Min(canvas.ActualWidth / canvas.ActualScale.Item1, canvas. _graphicsImage.Rectangle.Width), Math.Min(canvas.ActualHeight / canvas.ActualScale.Item2, canvas._graphicsImage.Rectangle.Height)));

            canvas.Drag(0, 0);
        }
        private static void OnActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var canvas = property as DrawingCanvas;
            if (canvas == null) return;

            var scale = canvas.ActualScale;
            var imageRect = canvas.GraphicsImage.Rectangle;
            canvas.LimitX = imageRect.Width > canvas.ActualWidth / scale.Item1 ? 0 : (canvas.ActualWidth / scale.Item1 - imageRect.Width) / 2;
            canvas.LimitY = imageRect.Height > canvas.ActualHeight / scale.Item2 ? 0 : (canvas.ActualHeight / scale.Item2 - imageRect.Height) / 2;

            foreach (var b in canvas.GraphicsList.Cast<GraphicsBase>())
            {
                b.ActualScale = scale;
                if ((b as GraphicsScaler) != null)
                {
                    b.RefreshDrawing();
                    continue;
                }
                b.Clip = new RectangleGeometry(new Rect(canvas.LimitX, canvas.LimitY, Math.Min(canvas.ActualWidth / canvas.ActualScale.Item1, imageRect.Width), Math.Min(canvas.ActualHeight / canvas.ActualScale.Item2 , imageRect.Height)));
            }
            canvas._graphicsImage.ActualScale = scale;
            canvas._graphicsImage.Clip = new RectangleGeometry(new Rect(canvas.LimitX, canvas.LimitY, Math.Min(canvas.ActualWidth / canvas.ActualScale.Item1 , imageRect.Width) , Math.Min(canvas.ActualHeight / canvas.ActualScale.Item2 , imageRect.Height ) ));
            canvas.Drag(0, 0);

        }
        private readonly Tool[] _tools = new Tool[(int)ToolType.Max];

        public DrawingCanvas()
        {
            _graphicsImage = new GraphicsImage();
            this.AddVisualChild(_graphicsImage);
            graphicsScaler = new GraphicsScaler();
            _graphicsList = new VisualCollection(this);
            _tools[(int)ToolType.Pointer] = new ToolPointer();
            //_tools[(int)ToolType.Line] = new ToolLine();
            _tools[(int)ToolType.Dragger] = new ToolDragger();
            _tools[(int)ToolType.Rectangle] = new ToolRectangle();
            _tools[(int)ToolType.Ellipse] = new ToolEllipse();

            var toolRuler = new ToolRuler();
            graphicsRuler = toolRuler.Ruler;
            _tools[(int)ToolType.Ruler] = toolRuler;
            var toolProfile = new ToolProfile();
            graphicsProfile = toolProfile.Profile;
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
        public delegate void MousePointHandler(Point point);
        public event MousePointHandler MousePoint;
        public delegate Vector MoveVisualRectHandler(Vector vector);
        public event MoveVisualRectHandler MoveVisualRect;
        public delegate void CanvasSizeChangedHandler(object sender, SizeChangedEventArgs e);
        public event CanvasSizeChangedHandler CanvasSizeChanged;
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

		internal int SelectionCount
        {
            get { return _graphicsList.OfType<GraphicsBase>().Count(g => g.IsSelected); }
        }

        internal VisualCollection GraphicsList
        {
            get { return _graphicsList; }
        }

        internal IEnumerable<GraphicsBase> Selection
        {
            get { return from GraphicsBase o in _graphicsList where o.IsSelected select o; }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;

            var point = e.GetPosition(this);
            point.X /= ActualScale.Item1;
            point.Y /= ActualScale.Item2;
            Focus();
            _tools[(int)Tool].OnMouseDown(this, e, point);
            e.Handled = true;
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;

            var point = e.GetPosition(this);
            point.X /= ActualScale.Item1;
            point.Y /= ActualScale.Item2;
            Point = point;
            _tools[(int)Tool].OnMouseMove(this, e, point);
            if (MousePoint != null) MousePoint(new Point(point.X-_graphicsImage.Rectangle.Left,point.Y - _graphicsImage.Rectangle.Top));
            e.Handled = true;
        }
        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_tools[(int)Tool] == null) return;

            var point = e.GetPosition(this);
            point.X /= ActualScale.Item1;
            point.Y /= ActualScale.Item2;

            _tools[(int)Tool].OnMouseUp(this, e, point);
            e.Handled = true;
        }
        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CanvasSizeChanged != null) CanvasSizeChanged(sender,e);
            var imageRect = GraphicsImage.Rectangle;
            LimitX = imageRect.Width > ActualWidth / ActualScale.Item1 ? 0 : (ActualWidth / ActualScale.Item1 - imageRect.Width) / 2;
            LimitY = imageRect.Height > ActualHeight / ActualScale.Item2 ? 0 : (ActualHeight / ActualScale.Item2 - imageRect.Height) / 2;

            _graphicsImage.Clip = new RectangleGeometry(new Rect(LimitX, LimitY, Math.Min( this.ActualWidth / this.ActualScale.Item1, _graphicsImage.Rectangle.Width), Math.Min(this.ActualHeight / this.ActualScale.Item2, _graphicsImage.Rectangle.Height)  ));
            for (int i = 0; i < _graphicsList.Count; i++)
            {
                var graphics = _graphicsList[i] as GraphicsBase;
                if (graphics != null)
                {
                    graphics.Clip = new RectangleGeometry(new Rect(LimitX, LimitY, Math.Min(this.ActualWidth / this.ActualScale.Item1, _graphicsImage.Rectangle.Width) , Math.Min(this.ActualHeight / this.ActualScale.Item2, _graphicsImage.Rectangle.Height) ));
                }
            }
            Drag(0, 0);
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
        public void Move(double deltaX, double deltaY)
        {
            var imageRect = _graphicsImage.Rectangle;
            double left = imageRect.Right, ritht = imageRect.Left, top = imageRect.Bottom, bottom = imageRect.Top;
            foreach (var o in this.Selection)
            {
                left = Math.Min(left, o.Rectangle.Left);
                ritht = Math.Max(ritht, o.Rectangle.Right);
                top = Math.Min(top, o.Rectangle.Top);
                bottom = Math.Max(bottom, o.Rectangle.Bottom);
            }
            deltaX = left + deltaX > imageRect.Left ? deltaX : imageRect.Left - left;
            deltaX = ritht + deltaX < imageRect.Right ? deltaX : imageRect.Right - ritht;
            deltaY = top + deltaY > imageRect.Top ? deltaY : imageRect.Top - top;
            deltaY = bottom + deltaY < imageRect.Bottom ? deltaY : imageRect.Bottom - bottom;
            foreach (var o in this.Selection)
            {
                o.Move(deltaX, deltaY);
            }
        }
        public double LimitX { get; set; }
        public double LimitY { get; set; }
        public void Drag(double deltaX, double deltaY)
        {
          var  imageRect = _graphicsImage.Rectangle;
            deltaX = imageRect.Left + deltaX > -(imageRect.Width - this.ActualWidth / ActualScale.Item1) ? deltaX : -(imageRect.Width - this.ActualWidth / ActualScale.Item1) - imageRect.Left;
            deltaX = imageRect.Left + deltaX < LimitX ? deltaX : LimitX - imageRect.Left;
            deltaY = imageRect.Top + deltaY > -(imageRect.Height - this.ActualHeight / ActualScale.Item2) ? deltaY : -(imageRect.Height - this.ActualHeight / ActualScale.Item2) - imageRect.Top;
            deltaY = imageRect.Top + deltaY < LimitY ? deltaY : LimitY - imageRect.Top;
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Move(deltaX, deltaY);
            }
            this._graphicsImage.Move(deltaX, deltaY);
        }
        public void Drag2(double deltaX, double deltaY)
        {
            var imageRect = _graphicsImage.Rectangle;
            if ((imageRect.Left + deltaX < -(imageRect.Width - this.ActualWidth / ActualScale.Item1) && (imageRect.Width > this.ActualWidth / ActualScale.Item1)) ||
               imageRect.Left + deltaX > LimitX ||
               (imageRect.Top + deltaY < -(imageRect.Height - this.ActualHeight / ActualScale.Item2) && (imageRect.Height > this.ActualHeight / ActualScale.Item2)) ||
               imageRect.Top + deltaY > LimitY
               )
            {
                var x = -imageRect.Left - (imageRect.Width - this.ActualWidth) / 2;
                var y = -imageRect.Top - (imageRect.Height - this.ActualHeight) / 2;
                var v = MoveVisualRect(new Vector(x, y));
                this._graphicsImage.Move(v.X, v.Y);
            }
            Drag(deltaX, deltaY);
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
