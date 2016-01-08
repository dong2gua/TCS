using System;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    class GraphicsSelectionRectangle : GraphicsRectangleBase
    {

        public GraphicsSelectionRectangle(Point point, DrawingCanvas canvas)
        {
            Canvas = canvas;
            ActualScale = Canvas.ActualScale;
            point = VerifyPoint(point);
            RectangleLeft = point.X;
            RectangleTop = point.Y;
            RectangleRight = point.X;
            RectangleBottom = point.Y;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");

            var dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);
            var dashedPen = new Pen(Brushes.White, GraphicsLineWidth) { DashStyle = dashStyle };

            drawingContext.DrawRectangle(null, dashedPen, ConvertToDisplayRect(Rectangle));
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
