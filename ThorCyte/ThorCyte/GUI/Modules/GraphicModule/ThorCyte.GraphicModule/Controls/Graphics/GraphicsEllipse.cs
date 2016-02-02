using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    /// <summary>
    ///  Rectangle graphics object.
    /// </summary>
    public class GraphicsEllipse : GraphicsRectangleBase
    {
        #region Properties

        public override RegionType GraphicType
        {
            get { return RegionType.Ellipse; }
        }

        #endregion

        #region Constructors

        public GraphicsEllipse(double left, double top, double right, double bottom,
             double lineWidth, Color objectColor, double xScale, Size parentSize, string name)
        {
            rectangleLeft = left;
            rectangleTop = top;
            rectangleRight = right;
            rectangleBottom = bottom;
            _graphicsLineWidth = lineWidth;
            _graphicsObjectColor = objectColor;
            _xScale = xScale;
            CreatedCanvasSize = parentSize;
            Name = name;
            IsDrawTrackerAll = true;
        }
        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            var brush = new SolidColorBrush(ObjectColor);
            var r = Rectangle;
            var center = new Point( (r.Left + r.Right) / 2.0,(r.Top + r.Bottom) / 2.0);
            double radiusX = (r.Right - r.Left) / 2.0;
            double radiusY = (r.Bottom - r.Top) / 2.0;
            var formatText = new FormattedText(Name, new CultureInfo("en-US"), FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), FontSize, new SolidColorBrush(Colors.White));
            dc.DrawEllipse(_fillObjectBrush, new Pen(brush, ActualLineWidth), center, radiusX, radiusY);
            center.X -= formatText.Width / 2;
            center.Y -= formatText.Height / 2;
            dc.DrawText(formatText, center);
            base.Draw(dc);
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

        public override string ToString()
        {
            return "Ellipse";
        }

        #endregion Overrides

    }
}
