using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public abstract class GraphicsBase : DrawingVisual
    {
        protected bool Selected;
        protected Color GraphicsObjectColor;
        protected Tuple<double, double, double> GraphicsActualScale=new Tuple<double, double, double>(1,1,1);
        private static readonly SolidColorBrush HandleBrush1 = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private static readonly SolidColorBrush HandleBrush2 = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        private static readonly SolidColorBrush HandleBrush3 = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
        protected double RectangleLeft;
        protected double RectangleTop;
        protected double RectangleRight;
        protected double RectangleBottom;
        protected DrawingCanvas Canvas;
        protected double HandleSize
        {
            get { return 12.0 ; }
        }
        protected double GraphicsLineWidth
        {
            get { return 2.0 ; }
        }
        protected double LineHitTestWidth
        {
            get { return 6.0; }
        }
        public bool IsSelected
        {
            get { return Selected; }
            set
            {
                Selected = value;
                RefreshDrawing();
            }
        }
        public Color ObjectColor
        {
            get { return GraphicsObjectColor; }
            set
            {
                GraphicsObjectColor = value;
                RefreshDrawing();
            }
        }
        public virtual Tuple<double, double, double> ActualScale
        {
            get { return GraphicsActualScale; }
            set
            {
                GraphicsActualScale = value;
            }
        }
        public Rect Rectangle
        {
            get
            {
                double l, t, w, h;
                if (RectangleLeft <= RectangleRight)
                {
                    l = RectangleLeft;
                    w = RectangleRight - RectangleLeft;
                }
                else
                {
                    l = RectangleRight;
                    w = RectangleLeft - RectangleRight;
                }

                if (RectangleTop <= RectangleBottom)
                {
                    t = RectangleTop;
                    h = RectangleBottom - RectangleTop;
                }
                else
                {
                    t = RectangleBottom;
                    h = RectangleTop - RectangleBottom;
                }
                return new Rect(l, t, w, h);
            }
        }
        public Rect DisplayRectangle
        {
            get { return ConvertToDisplayRect(Rectangle); }
        }
        protected GraphicsBase()
        {
            this.Transform = new ScaleTransform();
            GraphicsObjectColor = Colors.AliceBlue;
        }
        public abstract int HandleCount { get; }
        public abstract Point GetHandle(int handleNumber);
        public abstract Cursor GetHandleCursor(int handleNumber);
        public abstract void Move(double deltaX, double deltaY);
        public abstract void MoveHandleTo(Point point, int handleNumber);
        public abstract int MakeHitTest(Point point);
        public abstract bool Contains(Point point);
        public abstract bool IntersectsWith(Rect rectangle);
        public void RefreshDrawing()
        {
            using (var dc = RenderOpen())
            {
                Draw(dc);
            }
        }
        public virtual void Draw(DrawingContext drawingContext)
        {
            if (IsSelected)
            {
                DrawTracker(drawingContext);
            }
        }
        public virtual void DrawTracker(DrawingContext drawingContext)
        {
            for (var i = 1; i <= HandleCount; i++)
            {
                DrawTrackerRectangle(drawingContext, GetHandleRectangle(i));
            }
        }
        private static void DrawTrackerRectangle(DrawingContext drawingContext, Rect rectangle)
        {
            drawingContext.DrawRectangle(HandleBrush1, null, rectangle);

            drawingContext.DrawRectangle(HandleBrush2, null,
                new Rect(rectangle.Left + rectangle.Width / 8,
                    rectangle.Top + rectangle.Height / 8,
                    rectangle.Width * 6 / 8,
                    rectangle.Height * 6 / 8));

            drawingContext.DrawRectangle(HandleBrush3, null,
                new Rect(rectangle.Left + rectangle.Width / 4,
                    rectangle.Top + rectangle.Height / 4,
                    rectangle.Width / 2,
                    rectangle.Height / 2));
        }
        public virtual void Normalize()
        {
            if (RectangleLeft > RectangleRight)
            {
                var tmp = RectangleLeft;
                RectangleLeft = RectangleRight;
                RectangleRight = tmp;
            }

            if (RectangleTop > RectangleBottom)
            {
                var tmp1 = RectangleTop;
                RectangleTop = RectangleBottom;
                RectangleBottom = tmp1;
            }
        }
        public Rect GetHandleRectangle(int handleNumber)
        {
            var point = GetHandle(handleNumber);
            var sizeX = HandleSize;
            return  new Rect(point.X - sizeX / 2, point.Y - sizeX / 2, sizeX, sizeX);
        }
        protected Point VerifyPoint(Point point)
        {
            if (Canvas == null) return point;
            var rect = new Rect(Math.Max(0, Canvas.CanvasDisplyRect.X),
                                Math.Max(0, Canvas.CanvasDisplyRect.Y),
                                Math.Min(Canvas.ImageSize.Width, Canvas.CanvasDisplyRect.Width),
                                Math.Min(Canvas.ImageSize.Height, Canvas.CanvasDisplyRect.Height));

            if (rect.IsEmpty) return point;
            if (rect.Contains(point)) return point;

            var left = rect.Left;
            var right = rect.Right;
            var top = rect.Top;
            var bottom = rect.Bottom;
            var x = point.X;
            var y = point.Y;
            if (x <= left)
                x = left;
            else if (x >= right)
                x = right;

            if (y <= top)
                y = top;
            else if (y >= bottom)
                y = bottom;
            return new Point(x, y);
        }
        protected Rect ConvertToDisplayRect(Rect actualRect)
        {
            var canvasRect = Canvas.CanvasDisplyRect;
            var scale = Canvas.ActualScale;
            var x = (actualRect.X - canvasRect.X) * scale.Item3* scale.Item1;
            var y = (actualRect.Y - canvasRect.Y) * scale.Item3 * scale.Item2;
            var width = actualRect.Width * scale.Item3 * scale.Item1;
            var height = actualRect.Height * scale.Item3 * scale.Item2;
            return new Rect(x, y, width, height);
        }
        protected Rect ConvertToDisplayRectWithVisualScale(Rect actualRect)
        {
            var canvasRect = Canvas.CanvasDisplyRect;
            var scale = Canvas.ActualScale;
            var x = (actualRect.X - canvasRect.X) * scale.Item3;
            var y = (actualRect.Y - canvasRect.Y) * scale.Item3;
            var width = actualRect.Width * scale.Item3;
            var height = actualRect.Height * scale.Item3;
            return new Rect(x, y, width, height);
        }
        protected Point ConvertToDisplayPoint(Point actualPoint)
        {
            var canvasRect = Canvas.CanvasDisplyRect;
            var scale = Canvas.ActualScale;
            var x = (actualPoint.X - canvasRect.X) * scale.Item3 * scale.Item1;
            var y = (actualPoint.Y - canvasRect.Y) * scale.Item3 * scale.Item2;
            return new Point(x, y);
        }
    }
}
