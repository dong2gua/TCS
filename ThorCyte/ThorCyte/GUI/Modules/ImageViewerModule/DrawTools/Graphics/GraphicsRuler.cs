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
            drawingContext.DrawLine(new Pen(new SolidColorBrush(arrowColor), GraphicsLineWidth), start - vector1, start + vector1);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(arrowColor), GraphicsLineWidth), start - vector2, start + vector2);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(arrowColor), GraphicsLineWidth), end - vector1, end + vector1);
            drawingContext.DrawLine(new Pen(new SolidColorBrush(arrowColor), GraphicsLineWidth), end - vector2, end + vector2);

        }
        void drawText(DrawingContext drawingContext)
        {
            double value = Math.Sqrt(Math.Pow((LineStart.X - LineEnd.X) * XPixelSize, 2) + Math.Pow((LineStart.Y - LineEnd.Y) * YPixelSize, 2));
            var typeface = new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var format = new FormattedText(value.ToString("0.00") + "µm", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 20, new SolidColorBrush(Color.FromRgb(255, 20, 20)));
            var point = new Point((LineStart.X + LineEnd.X - format.Width / ActualScale.Item3 / ActualScale.Item1) / 2, (LineStart.Y + LineEnd.Y - format.Height / ActualScale.Item3 / ActualScale.Item2) / 2);
            var formatRect = new Rect(ConvertToDisplayPoint(point), new Size(format.Width, format.Height));
            drawingContext.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(80,255,255,128)), new Pen(new SolidColorBrush(Color.FromArgb(160,255, 255, 128)), GraphicsLineWidth), formatRect,2,2);
            drawingContext.DrawText(format, ConvertToDisplayPoint(point));
        }
    }
}
