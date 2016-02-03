using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.CarrierModule.Common
{
    public class DrawFunction
    {
        public static void DrawLine(DrawingContext dc, Pen pen, Point pt0, Point pt1)
        {
            var halfPenWidth = pen.Thickness / 2;
            var g = new GuidelineSet();
            g.GuidelinesX.Add(pt0.X + halfPenWidth);
            g.GuidelinesX.Add(pt1.X + halfPenWidth);
            g.GuidelinesY.Add(pt0.Y + halfPenWidth);
            g.GuidelinesY.Add(pt1.Y + halfPenWidth);
            dc.PushGuidelineSet(g);
            dc.DrawLine(pen, pt0, pt1);
            dc.Pop();
        }

        public static void DrawRectangle(DrawingContext dc, Brush brush, Pen pen, Rect rect)
        {
            if (pen != null)
            {
                var halfPenWidth = pen.Thickness / 2;
                var g = new GuidelineSet();
                g.GuidelinesX.Add(rect.Left + halfPenWidth);
                g.GuidelinesX.Add(rect.Right + halfPenWidth);
                g.GuidelinesY.Add(rect.Top + halfPenWidth);
                g.GuidelinesY.Add(rect.Bottom + halfPenWidth);

                dc.PushGuidelineSet(g);
                dc.DrawRectangle(brush, pen, rect);
                dc.Pop();
            }
            else
            {
                dc.DrawRectangle(brush, null, rect);
            }

        }

        public static void DrawGeometry(DrawingContext dc, Brush brush, Pen pen, PathGeometry pathGeometry, Point[] points)
        {
            if (pen != null)
            {
                var halfPenWidth = pen.Thickness / 2;
                var g = new GuidelineSet();
                foreach (var pt in points)
                {
                    g.GuidelinesX.Add(pt.X + halfPenWidth);
                    g.GuidelinesY.Add(pt.Y + halfPenWidth);
                    dc.PushGuidelineSet(g);
                }
                dc.DrawGeometry(brush, pen, pathGeometry);
                dc.Pop();
            }
            else
            {
                dc.DrawGeometry(brush, null, pathGeometry);
            }
        }
    }
}
