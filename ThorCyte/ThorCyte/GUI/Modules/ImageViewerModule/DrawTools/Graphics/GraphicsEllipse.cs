using System;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsEllipse : GraphicsRectangleBase
    {

        public GraphicsEllipse(double left, double top, double right, double bottom,Tuple<double, double,double> actualScale)
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

            var r = Rectangle;

            var center = new Point((r.Left + r.Right) / 2.0, (r.Top + r.Bottom) / 2.0);

            var radiusX = (r.Right - r.Left) / 2.0;
            var radiusY = (r.Bottom - r.Top) / 2.0;

            drawingContext.DrawEllipse(null, new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), center, radiusX, radiusY);

            base.Draw(drawingContext);
        }

        public override bool Contains(Point point)
        {
            var g = new EllipseGeometry(Rectangle);
            return g.FillContains(point) ;
        }

        public override bool IntersectsWith(Rect rectangle)
        {
            var rg = new RectangleGeometry(rectangle);   
            var eg = new EllipseGeometry(Rectangle);        

            var p = Geometry.Combine(rg, eg, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }
        

    }
}
