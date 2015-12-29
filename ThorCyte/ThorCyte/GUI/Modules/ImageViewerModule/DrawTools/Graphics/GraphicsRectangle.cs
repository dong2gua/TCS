using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;


namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsRectangle : GraphicsRectangleBase
    {

        public GraphicsRectangle(double left, double top, double right, double bottom,  Tuple<double, double, double> actualScale)
        {
            RectangleLeft = left;
            RectangleTop = top;
            RectangleRight = right;
            RectangleBottom = bottom;
            ActualScale = actualScale;
        }
        public Rect RealRect { get; set; }
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");

            drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), Rectangle);

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
