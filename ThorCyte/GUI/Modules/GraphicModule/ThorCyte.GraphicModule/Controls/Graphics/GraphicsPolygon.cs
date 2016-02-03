using System.Windows;
using System.Windows.Media;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    class GraphicsPolygon : GraphicsPolyLine
    {
        #region Properties

        public override RegionType GraphicType
        {
            get { return RegionType.Polygon; }
        }

        #endregion

        #region Constructors

        public GraphicsPolygon(Point[] points, double lineWidth, Color objectColor, Size parentSize, string name)
            : base(points, lineWidth, objectColor, parentSize, name)
        {
            PathGeometry.Figures[0].IsClosed = true;
        }

        #endregion Constructors

        #region Overriedes
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext dc)
        {
            PathGeometry.Figures[0].IsClosed = true;
            base.Draw(dc);
        }

        public override string ToString()
        {
            return "Polygon";
        }

        #endregion Overrides
    }
}
