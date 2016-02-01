using System.Windows.Media;
using ThorCyte.CarrierModule.Common;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace ThorCyte.CarrierModule.Graphics
{
    /// <summary>
    /// Selection Rectangle graphics object, used for group selection.
    /// 
    /// Instance of this class should be created only for group selection
    /// and removed immediately after group selection finished.
    /// </summary>
    class GraphicsSelectionRectangle : GraphicsRectangleBase
    {
        #region Constructors

        public GraphicsSelectionRectangle(double left, double top, double right, double bottom, double actualScale)
        {
            RectangleLeft = left;
            RectangleTop = top;
            RectangleRight = right;
            RectangleBottom = bottom;
            GraphicsLineWidth = 1.0;
            GraphicsActualScale = actualScale;
        }

        public GraphicsSelectionRectangle()
            :
            this(0.0, 0.0, 100.0, 100.0, 1.0)
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Draw graphics object
        /// </summary>
        public override void Draw(DrawingContext drawingContext)
        {
            DrawFunction.DrawRectangle(drawingContext,
                null,
                new Pen(Brushes.White, ActualLineWidth),
                Rectangle);

            var dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);

            //var dashedPen = new Pen(Brushes.CornflowerBlue, 2) {DashStyle = dashStyle};
            var dashedPen = new Pen(Brushes.HotPink, ActualLineWidth);
            var bsh = new SolidColorBrush(Color.FromArgb(50,255,105,180));


            DrawFunction.DrawRectangle(drawingContext,
                bsh,
                dashedPen,
                Rectangle);
        }

        public override bool Contains(Point point)
        {
            return Rectangle.Contains(point);
        }

        #endregion Overrides
    }
}
