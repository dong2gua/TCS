using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsThumbnail : GraphicsBase
    {
        public GraphicsThumbnail(DrawingCanvas canvas)
        {
            Canvas = canvas;
            GraphicsObjectColor = Colors.LightBlue;
        }
        public Vector Vector = new Vector(0,0);
        public override void Draw(DrawingContext drawingContext)
        {
            if (!Canvas.IsShowThumbnail) return;
            Rect rect1 =new Rect( Canvas.ImageSize);
            Rect rect2 = new Rect(Math.Max(Canvas.CanvasDisplyRect.X, 0), Math.Max(Canvas.CanvasDisplyRect.Y, 0), Math.Min(Canvas.CanvasDisplyRect.Width, rect1.Width), Math.Min(Canvas.CanvasDisplyRect.Height, rect1.Height));
            rect1.Scale(ActualScale.Item1, ActualScale.Item2);
            rect2.Scale(ActualScale.Item1, ActualScale.Item2);
            if (Math.Abs(rect1.Width - rect2.Width) < 0.001 && Math.Abs(rect1.Height - rect2.Height) < 0.001) return;
            double scale=  Math.Min(100 / rect1.Width, 100 / rect1.Height);
            rect1.Scale(scale, scale);
            rect1.Offset(Vector.X- rect1.Width,Vector.Y);
            rect2.Scale(scale, scale);
            rect2.Offset(Vector.X - rect1.Width, Vector.Y);
            drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), rect1);
            drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), rect2);
        }
        public override int HandleCount { get { return 0; } }
        public override Point GetHandle(int handleNumber) { return new Point(0, 0); }
        public override Cursor GetHandleCursor(int handleNumber) { return Cursors.Arrow; }
        public override void Move(double deltaX, double deltaY) { }
        public override void MoveHandleTo(Point point, int handleNumber) { }
        public override int MakeHitTest(Point point) { return 0; }
        public override bool Contains(Point point) { return false; }
        public override bool IntersectsWith(Rect rectangle) { return false; }
    }
}
