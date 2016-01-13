using System;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsRectangle : GraphicsRectangleBase
    {
        public GraphicsRectangle(Point point, DrawingCanvas canvas)
        {
            Canvas = canvas;
            ActualScale = Canvas.ActualScale;
            point = VerifyPoint(point);
            RectangleLeft = point.X;
            RectangleTop = point.Y;
            RectangleRight = point.X;
            RectangleBottom = point.Y;
            GraphicsObjectColor = Colors.Aqua;
        }
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");

            drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), ConvertToDisplayRect(Rectangle));

            base.Draw(drawingContext);
        }
        public override bool Contains(Point point)
        {
            return Rectangle.Contains(point);
        }
        public override bool IntersectsWith(Rect rectangle)
        {
            return Rectangle.IntersectsWith(rectangle);
        }
    }
}
