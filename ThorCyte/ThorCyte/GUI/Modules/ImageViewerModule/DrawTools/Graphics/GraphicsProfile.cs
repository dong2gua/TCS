using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;


namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsProfile : GraphicsLine
    {
        public GraphicsProfile()
        {
        }
        public void UpdatePoint(Point start, Point end, Tuple<double, double, double> actualScale)
        {
            LineStart = start;
            LineEnd = end;
            RectangleLeft = Math.Min(LineStart.X, LineEnd.X);
            RectangleTop = Math.Min(LineStart.Y, LineEnd.Y);
            RectangleRight = Math.Max(LineStart.X, LineEnd.X);
            RectangleBottom = Math.Max(LineStart.X, LineEnd.X);
            ActualScale = actualScale;

        }
        public GraphicsProfile(Point start, Point end, Tuple<double, double, double> actualScale)
        {

            LineStart = start;
            LineEnd = end;
            RectangleLeft = Math.Min(LineStart.X, LineEnd.X);
            RectangleTop = Math.Min(LineStart.Y, LineEnd.Y);
            RectangleRight = Math.Max(LineStart.X, LineEnd.X);
            RectangleBottom = Math.Max(LineStart.X, LineEnd.X);
            ActualScale = actualScale;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");
            //drawingContext.DrawLine(new Pen(new SolidColorBrush(ObjectColor), GraphicsLineWidth), LineStart, LineEnd);
            //double value = Math.Sqrt(Math.Abs(LineStart.X - LineEnd.X) * Math.Abs(LineStart.X - LineEnd.X) + Math.Abs(LineStart.Y - LineEnd.Y) * Math.Abs(LineStart.Y - LineEnd.Y));
            //var format = GetFormattedText(value.ToString("0.00"));
            //drawingContext.DrawText(format, LineEnd);
            base.Draw(drawingContext);
        }
        FormattedText GetFormattedText(string text)
        {


            var typeface = new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            return new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface,
                24 / Math.Min(ActualScale.Item1, ActualScale.Item2),
                new SolidColorBrush(GraphicsObjectColor));
           
        }




    }
}
