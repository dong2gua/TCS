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
            drawLine(drawingContext, ConvertToDisplayPoint(LineStart), ConvertToDisplayPoint(LineEnd));
            drawText(drawingContext);
        }
        void drawLine(DrawingContext drawingContext,Point start,Point end)
        {
            Color arrowColor = Color.FromRgb(255, 0, 0);
            Color lineColor = Color.FromRgb(0, 255, 255);
            Vector vector1 = new Vector(4, 4);
            Vector vector2 = new Vector(4, -4);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(lineColor), GraphicsLineWidth), start, end);
        }
        void drawText(DrawingContext drawingContext)
        {
            double value = Math.Sqrt(Math.Pow((LineStart.X - LineEnd.X) * XPixelSize, 2) + Math.Pow((LineStart.Y - LineEnd.Y) * YPixelSize, 2));
            var typeface = new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var format = new FormattedText(value.ToString("0.00") + "µm", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 16, new SolidColorBrush(Color.FromRgb(20, 20, 20)));
            double x = LineEnd.X + (format.Width+10) / ActualScale.Item3 / ActualScale.Item1 > Canvas.CanvasDisplyRect.Right ? LineEnd.X - (format.Width+10) / ActualScale.Item3 / ActualScale.Item1 : LineEnd.X+10/ ActualScale.Item3 / ActualScale.Item1;
            double y = LineEnd.Y + (format.Height+20 )/ ActualScale.Item3 / ActualScale.Item2 > Canvas.CanvasDisplyRect.Bottom ? Canvas.CanvasDisplyRect.Bottom - (format.Height) / ActualScale.Item3 / ActualScale.Item2 : LineEnd.Y + 20 / ActualScale.Item3 / ActualScale.Item2;
            var point = new Point(x, y);
            var formatRect = new Rect(ConvertToDisplayPoint(point), new Size(format.Width, format.Height));
            drawingContext.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), null, formatRect,2,2);
            drawingContext.DrawText(format, ConvertToDisplayPoint(point));
        }
    }
}
