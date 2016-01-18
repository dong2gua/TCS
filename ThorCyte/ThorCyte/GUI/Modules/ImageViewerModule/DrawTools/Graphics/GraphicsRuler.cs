using System;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsRuler : GraphicsLine
    {
        public double XPixelSize { get; set; }
        public double YPixelSize { get; set; }
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");
            double value = Math.Sqrt(Math.Pow((LineStart.X - LineEnd.X) * XPixelSize, 2) + Math.Pow((LineStart.Y - LineEnd.Y) * YPixelSize, 2));
            var format = GetFormattedText(value.ToString("0.00")+ "µm");
            var point = new Point((LineStart.X+LineEnd.X-format.Width * ActualScale.Item3) / 2, (LineStart.Y + LineEnd.Y - format.Height * ActualScale.Item3) / 2);
            drawingContext.DrawText(format,ConvertToDisplayPoint(point));
            base.Draw(drawingContext);
        }
        FormattedText GetFormattedText(string text)
        {
            var typeface = new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            return new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface,
                24 ,
                new SolidColorBrush(GraphicsObjectColor));
           
        }
    }
}
