using System;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    class GraphicsSelectionRectangle : GraphicsRectangleBase
    {

        public GraphicsSelectionRectangle(double left, double top, double right, double bottom, Tuple<double, double, double> actualScale)
        {
            RectangleLeft = left;
            RectangleTop = top;
            RectangleRight = right;
            RectangleBottom = bottom;
            ActualScale = actualScale;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");

            var dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);
            var dashedPen = new Pen(Brushes.White, GraphicsLineWidth) { DashStyle = dashStyle };

            drawingContext.DrawRectangle(null, dashedPen, Rectangle);
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
