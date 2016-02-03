using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.GraphicModule.Helper
{
    public class DrawHelper
    {
        public static void DrawLine(DrawingContext dc, Pen pen, Point ptStart, Point ptEnd)
        {
            //var halfPenWidth = pen.Thickness / 2;
            //var g = new GuidelineSet();
            //g.GuidelinesX.Add(ptStart.X + halfPenWidth);
            //g.GuidelinesX.Add(ptEnd.X + halfPenWidth);
            //g.GuidelinesY.Add(ptStart.Y + halfPenWidth);
            //g.GuidelinesY.Add(ptEnd.Y + halfPenWidth);
            //dc.PushGuidelineSet(g);
            dc.DrawLine(pen, ptStart, ptEnd);
            //dc.Pop();
        }

        public static void DrawRectangle(DrawingContext dc, Brush brush, Pen pen, Rect rect)
        {
            if (pen != null)
            {
                //var halfPenWidth = pen.Thickness / 2;
                //var g = new GuidelineSet();
                //g.GuidelinesX.Add(rect.Left + halfPenWidth);
                //g.GuidelinesX.Add(rect.Right + halfPenWidth);
                //g.GuidelinesY.Add(rect.Top + halfPenWidth);
                //g.GuidelinesY.Add(rect.Bottom + halfPenWidth);
                //dc.PushGuidelineSet(g);
                dc.DrawRectangle(brush, pen, rect);
                // dc.Pop();
            }
            else
            {
                dc.DrawRectangle(brush, null, rect);
            }
        }

        public static void DrawText(DrawingContext dc, string title, Point origin, double fontsize, Transform transform, Brush brush, TextAlignment alignment)
        {
            var formatTitle = new FormattedText(title, new CultureInfo("en-US"), FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Light, FontStretches.Normal), fontsize, brush)
            {
                TextAlignment = alignment,
                MaxTextWidth = 60,
                MaxTextHeight = 30
            };
            if (transform != null)
            {
                dc.PushTransform(transform);
            }
            dc.DrawText(formatTitle, origin);
            if (transform != null)
            {
                dc.Pop();
            }
        }

        public static FormattedText GetFormatText(string title, double fontsize, Brush brush)
        {
            return new FormattedText(title, new CultureInfo("en-US"), FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Light, FontStretches.Normal), fontsize, brush)
            {
                Trimming = TextTrimming.CharacterEllipsis,
                MaxTextHeight = 200,
                MaxTextWidth = 200
            };
        }
    }
}
