using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.CarrierModule.Graphics
{
    class GraphicsPolygon : GraphicsPolyLine
    {
        #region Constructors

        public GraphicsPolygon(Point[] points, double lineWidth, Color objectColor, double actualScale, int roomNo)
            : base(points, lineWidth, objectColor, actualScale, roomNo)
        {
            PathGeometry.Figures[0].IsClosed = true;
        }

        static GraphicsPolygon()
        {
            var stream = new MemoryStream(Properties.Resources.PolyHandle);

            HandleCursor = new Cursor(stream);
        }

        public static Cursor HandleCursor { get; set; }

        #endregion Constructors

        #region Overriedes
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext)
        {
            PathGeometry.Figures[0].IsClosed = true;

            base.Draw(drawingContext);
        }

        public override string ToString()
        {
            return "Polygon";
        }

        #endregion Overrides
    }
}
