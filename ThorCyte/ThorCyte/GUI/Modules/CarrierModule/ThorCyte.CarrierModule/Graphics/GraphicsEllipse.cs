using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.CarrierModule.Graphics
{
    /// <summary>
    ///  Rectangle graphics object.
    /// </summary>
    public class GraphicsEllipse : GraphicsRectangleBase
    {
        #region Constructors

        public GraphicsEllipse(double left, double top, double right, double bottom,
            double lineWidth, Color objectColor, double actualScale, int roomNo)
        {
            RectangleLeft = left;
            RectangleTop = top;
            RectangleRight = right;
            RectangleBottom = bottom;
            GraphicsLineWidth = lineWidth;
            GraphicsObjectColor = objectColor;
            GraphicsActualScale = actualScale;
            RoomId = roomNo;
        }
        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            var r = Rectangle;

            var center = new Point(
                (r.Left + r.Right) / 2.0,
                (r.Top + r.Bottom) / 2.0);

            var radiusX = (r.Right - r.Left) / 2.0;
            var radiusY = (r.Bottom - r.Top) / 2.0;

            drawingContext.DrawEllipse(
                FillObjectBrush,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                center,
                radiusX,
                radiusY);

            base.Draw(drawingContext);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            if (IsSelected)
            {
                return Rectangle.Contains(point);
            }
            var g = new EllipseGeometry(Rectangle);

            return g.FillContains(point) || g.StrokeContains(new Pen(Brushes.Black, ActualLineWidth), point);
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            var rg = new RectangleGeometry(rectangle);    // parameter
            var eg = new EllipseGeometry(Rectangle);        // this object rectangle

            var p = Geometry.Combine(rg, eg, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }


        ///// <summary>
        ///// Serialization support
        ///// </summary>
        //public override PropertiesGraphicsBase CreateSerializedObject()
        //{
        //    return new PropertiesGraphicsEllipse(this);
        //}

        public override string ToString()
        {
            return "Ellipse";
        }

        #endregion Overrides

    }
}
