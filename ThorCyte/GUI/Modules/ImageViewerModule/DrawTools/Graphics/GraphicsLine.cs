using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsLine : GraphicsBase
    {
        protected Point LineStart;
        protected Point LineEnd;
        public Point Start
        {
            get { return LineStart; }
            set { LineStart = value; }
        }
        public Point End
        {
            get { return LineEnd; }
            set { LineEnd = value; }
        }
        public void UpdatePoint(Point point, DrawingCanvas canvas)
        {
            Canvas = canvas;
            ActualScale = Canvas.ActualScale;
            point = VerifyPoint(point);
            LineStart = point;
            LineEnd = point;
            RectangleLeft = point.X;
            RectangleTop = point.Y;
            RectangleRight = point.X;
            RectangleBottom = point.Y;
            GraphicsObjectColor = Colors.DeepPink;
        }
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth),
                ConvertToDisplayPoint(LineStart),
                ConvertToDisplayPoint(LineEnd));
            base.Draw(drawingContext);
        }
        public override int HandleCount { get { return 2; } }
        public override Point GetHandle(int handleNumber)
        {
            return handleNumber == 1 ? LineStart : LineEnd;
        }
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1: return Cursors.SizeAll;
                case 2: return Cursors.SizeAll;
                default: return Cursors.Arrow;
            }
        }
        public override void Move(double deltaX, double deltaY)
        {
            LineStart.X += deltaX;
            LineStart.Y += deltaY;
            LineEnd.X += deltaX;
            LineEnd.Y += deltaY;
            RectangleLeft = Math.Min(LineStart.X, LineEnd.X);
            RectangleTop = Math.Min(LineStart.Y, LineEnd.Y);
            RectangleRight = Math.Max(LineStart.X, LineEnd.X);
            RectangleBottom = Math.Max(LineStart.Y, LineEnd.Y);
            RefreshDrawing();
        }
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            point = VerifyPoint(point);
            if (handleNumber == 1)
                LineStart = point;
            else
                LineEnd = point;
            RefreshDrawing();
        }
        public override int MakeHitTest(Point point)
        {
            if (IsSelected)
            {
                for (var i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(ConvertToDisplayPoint( point)))
                        return i;
                }
            }
            if (Contains(point))
                return 0;

            return -1;
        }
        public override bool Contains(Point point)
        {
            var g = new LineGeometry(LineStart, LineEnd);
            return g.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }
        public override bool IntersectsWith(Rect rectangle)
        {
            var rg = new RectangleGeometry(rectangle);
            var lg = new LineGeometry(LineStart, LineEnd);
            var widen = lg.GetWidenedPathGeometry(new Pen(Brushes.Black, LineHitTestWidth));
            var p = Geometry.Combine(rg, widen, GeometryCombineMode.Intersect, null);
            return (!p.IsEmpty());
        }
    }
}
