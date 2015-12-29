using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsScaler : GraphicsBase
    {
        public GraphicsScaler()
        {
        }
        public Point Point;


        public override void Draw(DrawingContext drawingContext)
        {
            var start = new Point(Point.X / ActualScale.Item1 , Point.Y / ActualScale.Item1);
            var End = new Point(Point.X / ActualScale.Item1 + 100 , Point.Y / ActualScale.Item1);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), start, End);
            var format = GetFormattedText("100");
            var textPoint = new Point(Point.X / ActualScale.Item1 + 50 - format.Width/2 , (Point.Y - 20) / ActualScale.Item1);
            drawingContext.DrawText(format, textPoint);

        }
        FormattedText GetFormattedText(string text)
        {


            var typeface = new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            return new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface,
                16 / Math.Min(ActualScale.Item1, ActualScale.Item2),
                new SolidColorBrush(GraphicsObjectColor));

        }

        public override int HandleCount { get { return 0; } }
        public override Point GetHandle(int handleNumber) { return new Point(0, 0); }
        public override Cursor GetHandleCursor(int handleNumber) { return Cursors.Arrow; }
        public override void Move(double deltaX, double deltaY) { }
        public override void MoveHandleTo(Point point, int handleNumber) { }
        public override int MakeHitTest(Point point) { return 0; }
        public override bool Contains(Point point) { return false; }
        public override bool IntersectsWith(Rect rectangle) { return false; }

    }
}
