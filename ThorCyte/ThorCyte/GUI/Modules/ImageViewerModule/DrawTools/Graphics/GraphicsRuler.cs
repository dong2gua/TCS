using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;


namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsRuler : GraphicsLine
    {
        public double XPixelSize { get; set; }
        public double YPixelSize { get; set; }
        public void UpdatePoint(Point point, DrawingCanvas canvas)
        {
            Canvas = canvas;
            ActualScale = Canvas.ActualScale;
            point = VerifyPoint(point);
            LineStart = point;
            LineEnd = point;
            RectangleLeft = point.X;
            RectangleTop = point.Y;
            RectangleRight = point.X;
            RectangleBottom = point.Y;
        }
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");
            double value = Math.Sqrt(Math.Pow((LineStart.X - LineEnd.X) * XPixelSize, 2) + Math.Pow((LineStart.Y - LineEnd.Y) * YPixelSize, 2));
            var format = GetFormattedText(value.ToString("0.00"));
            drawingContext.DrawText(format,ConvertToDisplayPoint(LineEnd));
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
