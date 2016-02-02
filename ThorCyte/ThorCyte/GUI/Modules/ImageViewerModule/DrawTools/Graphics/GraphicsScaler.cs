using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsScaler : GraphicsBase
    {
        public GraphicsScaler()
        {
            GraphicsObjectColor = Colors.Black;
        }
        public Point Point;
        public double XPixelSize { get; set; }
        public double YPixelSize { get; set; }
        public override void Draw(DrawingContext drawingContext)
        {
            if (XPixelSize == 0) return;
            int textLength;
            double displayLength, unitC = 1;
            var realLength = 200 / ActualScale.Item3 / ActualScale.Item1* XPixelSize;
            var unit = "µm";
            var digits =(int)Math.Floor( Math.Log10(realLength));
            int tmp = (int)(realLength / Math.Pow(10, digits));
            if (tmp >= 5) tmp = 5;
            else if (tmp >= 2) tmp = 2;
            else if (tmp >= 1) tmp = 1;
            else if (tmp >= 10 || tmp < 1) throw new Exception();
            else throw new Exception();
            if(digits>=6)
            {
                unit = "m";
                textLength = tmp *(int) Math.Pow(10, digits - 6);
                unitC = (int)Math.Pow(10, 6);
           }
            else if (digits>=3)
            {
                unit = "mm";
                textLength = tmp * (int)Math.Pow(10, digits - 3);
                unitC = (int)Math.Pow(10, 3);
            }
            else
            {
                textLength = tmp * (int)Math.Pow(10, digits);
            }
            displayLength = textLength * unitC * ActualScale.Item3* ActualScale.Item1 / XPixelSize;

            var start = new Point(Point.X , Point.Y);
            var end = new Point(Point.X  + displayLength, Point.Y );
            drawingContext.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)), null, new Rect(start.X-3,start.Y-20,displayLength+6,25), 2, 2);

            drawingContext.DrawLine(new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), start, end);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth),new Point( start.X,start.Y-5), start);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), new Point(end.X, end.Y - 5 ), end);
            var format = GetFormattedText(textLength.ToString()+" "+unit);
            var textPoint = new Point(Point.X  + displayLength/2 - format.Width/2 , (Point.Y - 20));
            drawingContext.DrawText(format, textPoint);

        }
        FormattedText GetFormattedText(string text)
        {
            var typeface = new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            return new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface,
                16 ,
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
