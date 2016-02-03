using System;
using System.Windows;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.Utils;
using Vector = ThorCyte.GraphicModule.Models.Vector;

namespace ThorCyte.GraphicModule.Helper
{
    public class RegionSnapHelper
    {
        const int Threshold = 6;
        const double AngleThreshold = 5; // degree

        public static bool SnapEdges(Scattergram parent, GraphicsBase graphic) // jcl-7386
        {

            if (graphic.GraphicType == RegionType.Rectangle)
            {

                foreach (var g in parent.VisualList)
                {
                    if (Equals(g, graphic))
                        continue;
                    var region = (GraphicsBase)g;
                    var r = (GraphicsRectangle)graphic;
                    if (region.GraphicType == RegionType.Rectangle)
                    {
                        var rect = (GraphicsRectangle)region;
                        SnapRectToRect(r, rect.Rectangle);
                    }
                    else if (region.GraphicType == RegionType.Polygon)
                    {
                        var polygon = (GraphicsPolygon)region;
                        SnapRectToPolygon(r, polygon);
                        polygon.RefreshDrawing();
                    }
                }

                //if (r == g)
                //    return false;
                //else
                //    return true;
            }
            return false;
        }


        private static void SnapRectToRect(GraphicsRectangle rect, Rect r)
        {
            var leftTop = rect.Rectangle.Location;
            var width = rect.Rectangle.Width;
            var height = rect.Rectangle.Height;

            if (Math.Abs(rect.Rectangle.TopLeft.Y - r.TopLeft.Y - r.Height) < Threshold &&
                (rect.Rectangle.TopLeft.X < (r.TopLeft.X + r.Width) && r.TopLeft.X < (rect.Rectangle.TopLeft.X + rect.Rectangle.Width)))
            {
                leftTop = new Point(rect.Rectangle.X, r.Y + r.Height);
                height = rect.Rectangle.Y + rect.Rectangle.Height - r.Y - r.Height;
                rect.Rectangle = new Rect(leftTop, new Size(rect.Rectangle.Width, height));
                return;
            }

            if (Math.Abs(rect.Rectangle.X + rect.Rectangle.Width - r.X) < Threshold &&
                (rect.Rectangle.Y < (r.Y + r.Height) && r.Y < (rect.Rectangle.Y + rect.Rectangle.Height)))
            {
                width = r.X - rect.Rectangle.X;
                rect.Rectangle = new Rect(leftTop, new Size(width, height));
                return;
            }

            if (Math.Abs(rect.Rectangle.Y + rect.Rectangle.Height - r.Y) < Threshold &&
                (rect.Rectangle.X < (r.X + r.Width) && r.X < (rect.Rectangle.X + rect.Rectangle.Width)))
            {
                height = r.Y - rect.Rectangle.Y;
                rect.Rectangle = new Rect(leftTop, new Size(width, height));
                return;
            }

            if (Math.Abs(rect.Rectangle.X - r.X - r.Width) < Threshold &&
                (rect.Rectangle.Y < (r.Y + r.Height) && r.Y < (rect.Rectangle.Y + rect.Rectangle.Height)))
            {
                leftTop = new Point(r.X + r.Width, rect.Rectangle.Y);
                width = rect.Rectangle.X + rect.Rectangle.Width - r.X - r.Width;
                rect.Rectangle = new Rect(leftTop, new Size(width, height));
            }
        }

        private static void SnapRectToPolygon(GraphicsRectangle rect, GraphicsPolygon polygon)
        {
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < polygon.Points.Length - 1; j++)
                {
                    if (IsTooBigAngle(rect, i, polygon, j))
                        continue;

                    double vi1 = 0.0, vi2 = 0.0;
                    if (!IsCloseEnough(rect, i, polygon, j, ref vi1, ref vi2))
                        continue;

                    if (IsOpositeDirection(polygon, j, i, (vi1 + vi2) / 2))
                        continue;

                    SnapPolyEdgeToRect(rect, i, polygon, j); // use this function because it can only snap from a poly to rect
                    return;
                }
            }
        }

        /// <summary>
        /// edge: pt[i] -> pt[i + 1]; 
        /// m = i - 1
        /// n = i + 2
        /// </summary>
        private static void FindConsecutiveNodesForEdge(GraphicsPolygon polygon, int i, ref int m, ref int n)
        {
            if (i == 0)
                m = polygon.Points.Length - 2;
            else
                m = i - 1;

            if (i == polygon.Points.Length - 2)
                n = 1;
            else
                n = i + 2;
        }

        private static void LimitToBounds(ref Point pt1, ref Point pt2)
        {
            //Rectangle rect = m_parent.BoundingRect;

            //int x, y;
            //if (pt1.X < rect.Left)
            //{
            //    // pt1.x = pt2.x won't happen
            //    y = (rect.Left - pt2.X) * (pt1.Y - pt2.Y) / (pt1.X - pt2.X) + pt2.Y;
            //    pt1.X = rect.Left;
            //    pt1.Y = y;
            //}

            //if (pt1.X > rect.Right)
            //{
            //    y = (rect.Right - pt2.X) * (pt1.Y - pt2.Y) / (pt1.X - pt2.X) + pt2.Y;
            //    pt1.X = rect.Right;
            //    pt1.Y = y;
            //}

            //if (pt1.Y < rect.Top)
            //{
            //    x = (rect.Top - pt2.Y) * (pt1.X - pt2.X) / (pt1.Y - pt2.Y) + pt2.X;
            //    pt1.X = x;
            //    pt1.Y = rect.Top;
            //}

            //if (pt1.Y > rect.Bottom)
            //{
            //    x = (rect.Bottom - pt2.Y) * (pt1.X - pt2.X) / (pt1.Y - pt2.Y) + pt2.X;
            //    pt1.X = x;
            //    pt1.Y = rect.Bottom;
            //}

            //if (pt2.X < rect.Left)
            //{
            //    y = (rect.Left - pt1.X) * (pt1.Y - pt2.Y) / (pt1.X - pt2.X) + pt1.Y;
            //    pt2.X = rect.Left;
            //    pt2.Y = y;
            //}

            //if (pt2.X > rect.Right)
            //{
            //    y = (rect.Right - pt1.X) * (pt1.Y - pt2.Y) / (pt1.X - pt2.X) + pt1.Y;
            //    pt2.X = rect.Right;
            //    pt2.Y = y;
            //}

            //if (pt2.Y < rect.Top)
            //{
            //    x = (rect.Top - pt1.Y) * (pt1.X - pt2.X) / (pt1.Y - pt2.Y) + pt1.X;
            //    pt2.X = x;
            //    pt2.Y = rect.Top;
            //}

            //if (pt2.Y > rect.Bottom)
            //{
            //    x = (rect.Bottom - pt1.Y) * (pt1.X - pt2.X) / (pt1.Y - pt2.Y) + pt1.X;
            //    pt2.X = x;
            //    pt2.Y = rect.Bottom;
            //}
        }

        private static void SnapPolyEdgeToRect(GraphicsRectangle rect, int i, GraphicsPolygon polygon, int j)
        {
            int m = 0, n = 0;
            FindConsecutiveNodesForEdge(polygon, i, ref m, ref n);

            var pt1 = new PointD(polygon.Points[m]);
            var pt2 = new PointD(polygon.Points[i]);
            var pt3 = new PointD(polygon.Points[n]);
            var pt4 = new PointD(polygon.Points[i + 1]);
            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            switch (j)
            {
                case 0:
                    y1 = y2 = rect.Rectangle.Y;
                    x1 = pt1.X + (y1 - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y);
                    x2 = pt3.X + (y2 - pt3.Y) * (pt4.X - pt3.X) / (pt4.Y - pt3.Y);
                    break;
                case 1:
                    x1 = x2 = rect.Rectangle.X + rect.Rectangle.Width;
                    y1 = pt1.Y + (x1 - pt1.X) * (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
                    y2 = pt3.Y + (x2 - pt3.X) * (pt4.Y - pt3.Y) / (pt4.X - pt3.X);
                    break;
                case 2:
                    y1 = y2 = rect.Rectangle.Y + rect.Rectangle.Height;
                    x1 = pt1.X + (y1 - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y);
                    x2 = pt3.X + (y2 - pt3.Y) * (pt4.X - pt3.X) / (pt4.Y - pt3.Y);
                    break;
                case 3:
                    x1 = x2 = rect.Rectangle.X;
                    y1 = pt1.Y + (x1 - pt1.X) * (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
                    y2 = pt3.Y + (x2 - pt3.X) * (pt4.Y - pt3.Y) / (pt4.X - pt3.X);
                    break;
            }

            var ppt1 = new Point((int)x1, (int)y1); // jcl-7684
            var ppt2 = new Point((int)x2, (int)y2);
            //LimitToBounds(ref ppt1, ref ppt2);

            polygon.Points[i] = ppt1;
            polygon.Points[i + 1] = ppt2;

            if (i == 0)
                polygon.Points[polygon.Points.Length - 1] = polygon.Points[0];
            if (i == polygon.Points.Length - 2)
                polygon.Points[0] = polygon.Points[polygon.Points.Length - 1];

            //polygon.RecalcRegionBounds();
        }


        //private static bool IsTooBigAngle(MaskRegion rgn, int i, MaskRegion r, int j)
        private static bool IsTooBigAngle(GraphicsRectangle rect, int i, GraphicsPolygon polygon, int j)
        {
            var ln1 = new Line(new PointD(polygon.Points[j]), new PointD(polygon.Points[j + 1]));

            // rect: j = 0 top; j = 1 right; j = 2 bottom; j = 3 left 
            Line ln2;
            switch (j)
            {
                case 0:
                    ln2 = new Line(new PointD(rect.Rectangle.X, rect.Rectangle.Y),
                        new PointD(rect.Rectangle.X + rect.Rectangle.Width, rect.Rectangle.Y));
                    break;
                case 1:
                    ln2 = new Line(new PointD(rect.Rectangle.X + rect.Rectangle.Width, rect.Rectangle.Y),
                        new PointD(rect.Rectangle.X + rect.Rectangle.Width, rect.Rectangle.Y + rect.Rectangle.Height));
                    break;
                case 2:
                    ln2 = new Line(new PointD(rect.Rectangle.X + rect.Rectangle.Width, rect.Rectangle.Y + rect.Rectangle.Height),
                        new PointD(rect.Rectangle.X, rect.Rectangle.Y + rect.Rectangle.Height));
                    break;
                case 3:
                    ln2 = new Line(new PointD(rect.Rectangle.X, rect.Rectangle.Y + rect.Rectangle.Height),
                        new PointD(rect.Rectangle.X, rect.Rectangle.Y));
                    break;
                default:
                    throw new Exception("RegionTool");
            }

            double angle = Line.Angle(ln1, ln2);

            if (Math.Abs(angle) < AngleThreshold || Math.Abs(angle - 180) < AngleThreshold ||
                Math.Abs(angle - 360) < AngleThreshold)
                return false;

            return true;
        }

        private static bool IsCloseEnough(GraphicsRectangle rect, int i, GraphicsPolygon polygon, int j, ref double vi1, ref double vi2)
        {
            double x1, x2, y1, y2;
            if (polygon.Points[i].X > polygon.Points[i + 1].X)
            {
                x1 = polygon.Points[i + 1].X;
                x2 = polygon.Points[i].X;
            }
            else
            {
                x2 = polygon.Points[i + 1].X;
                x1 = polygon.Points[i].X;
            }
            if (polygon.Points[i].Y > polygon.Points[i + 1].Y)
            {
                y1 = polygon.Points[i + 1].Y;
                y2 = polygon.Points[i].Y;
            }
            else
            {
                y2 = polygon.Points[i + 1].Y;
                y1 = polygon.Points[i].Y;
            }

            double xmd, ymd;
            // rect: j = 0 top; j = 1 right; j = 2 bottom; j = 3 left 
            switch (j)
            {
                case 0:
                    if (Intersection(x1, x2, rect.Rectangle.X, rect.Rectangle.X + rect.Rectangle.Width, ref vi1, ref vi2))
                    {
                        ymd = GetOtherCoord(new PointD(polygon.Points[i]), new PointD(polygon.Points[i + 1]), (vi1 + vi2) / 2.0, true);
                        if (Math.Abs(ymd - rect.Rectangle.Y) < Threshold)
                            return true;
                        else
                            return false;
                    }
                    break;
                case 1:
                    if (Intersection(y1, y2, rect.Rectangle.Y, rect.Rectangle.Y + rect.Rectangle.Height, ref vi1, ref vi2))
                    {
                        xmd = GetOtherCoord(new PointD(polygon.Points[i]), new PointD(polygon.Points[i + 1]), (vi1 + vi2) / 2.0, false);
                        if (Math.Abs(xmd - rect.Rectangle.X - rect.Rectangle.Width) < Threshold)
                            return true;
                        else
                            return false;
                    }
                    break;
                case 2:
                    if (Intersection(x1, x2, rect.Rectangle.X, rect.Rectangle.X + rect.Rectangle.Width, ref vi1, ref vi2))
                    {
                        ymd = GetOtherCoord(new PointD(polygon.Points[i]), new PointD(polygon.Points[i + 1]), (vi1 + vi2) / 2.0, true);
                        if (Math.Abs(ymd - rect.Rectangle.Y - rect.Rectangle.Height) < Threshold)
                            return true;
                        else
                            return false;
                    }
                    break;
                case 3:
                    if (Intersection(y1, y2, rect.Rectangle.Y, rect.Rectangle.Y + rect.Rectangle.Height, ref vi1, ref vi2))
                    {
                        xmd = GetOtherCoord(new PointD(polygon.Points[i]), new PointD(polygon.Points[i + 1]), (vi1 + vi2) / 2.0, false);
                        if (Math.Abs(xmd - rect.Rectangle.X) < Threshold)
                            return true;
                        else
                            return false;
                    }
                    break;
                default:
                    throw new Exception("No this line");
            }

            return false;
        }

        private static bool Intersection(double v1, double v2, double v01, double v02, ref double vi1, ref double vi2)
        {
            vi1 = Math.Max(v1, v01);
            vi2 = Math.Min(v2, v02);

            if (vi1 < vi2)
                return true;
            else
                return false;
        }

        private static double GetOtherCoord(PointD pt1, PointD pt2, double v, bool isX)
        {
            if (isX) // return y coord
            {
                if (IsDoubleEqual(pt1.Y, pt2.Y))
                    return pt1.Y;
                else
                    return (v - pt1.X) * (pt2.Y - pt1.Y) / (pt2.X - pt1.X) + pt1.Y;
            }
            else // return x coord
            {
                if (IsDoubleEqual(pt1.X, pt2.X))
                    return pt1.X;
                else
                    return (v - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y) + pt1.X;
            }
        }

        private static bool IsOpositeDirection(GraphicsPolygon polygon, int i, int j, double vi)
        {
            // rect: j = 0 top; j = 1 right; j = 2 bottom; j = 3 left 
            switch (j)
            {
                case 0:
                    if (IntersectWithLines(polygon, i, vi, new Vector(0, 1), j))
                        return true;
                    else
                        return false;
                case 1:
                    if (IntersectWithLines(polygon, i, vi, new Vector(-1, 0), j))
                        return true;
                    else
                        return false;
                case 2:
                    if (IntersectWithLines(polygon, i, vi, new Vector(0, -1), j))
                        return true;
                    else
                        return false;
                case 3:
                    if (IntersectWithLines(polygon, i, vi, new Vector(1, 0), j))
                        return true;
                    else
                        return false;
                default:
                    throw new Exception("No this line");
            }
        }

        private static bool IntersectWithLine(PointD pt1, PointD pt2, PointD pt, Vector v)
        {
            // edge: pt1 + s * (pt2 - pt1); s is in [0, 1]
            // line: pt + t * vector
            // if has intersect return true, otherwise return false
            double xd = pt2.X - pt1.X;
            double yd = pt2.Y - pt1.Y;

            double s = (pt.X * v.Y - pt.Y * v.X - v.Y * pt1.X + v.X * pt1.Y) / (v.Y * xd - v.X * yd);
            double t = (xd * pt1.Y - yd * pt1.X - xd * pt.Y + yd * pt.X) / (xd * v.Y - yd * v.X);

            if (s >= 0 && s <= 1 && t >= 0)
                return true;
            else
                return false;
        }

        private static bool IntersectWithLines(GraphicsPolygon rgn, int i, double vi, Vector vector, int j)
        {
            // pt[j] = rgn.Points[j]
            // edge: pt[j] + s * (pt[j + 1] - pt[j]), s is in [0, 1], j != i
            // and in edge: pt[i], pt[i + 1], the POint correstpond to vi and vector
            // to above two can be insected

            // find the point
            // j = 0, 2 is x axis, vi is x coord. j = 1, 3 is y axis, vi is y coord
            double x, y;
            if (j == 0 || j == 2)
            {
                x = vi;
                y = GetOtherCoord(new PointD(rgn.Points[i]), new PointD(rgn.Points[i + 1]), vi, true);
            }
            else // j == 1 || j == 3
            {
                y = vi;
                x = GetOtherCoord(new PointD(rgn.Points[i]), new PointD(rgn.Points[i + 1]), vi, false);
            }
            PointD pt = new PointD(x, y);

            for (int k = 0; k < i - 1; k++)
            {
                if (IntersectWithLine(new PointD(rgn.Points[k]), new PointD(rgn.Points[k + 1]), pt, vector))
                    return true;
            }

            for (int k = i + 1; k < rgn.Points.Length - 1; k++)
            {
                if (IntersectWithLine(new PointD(rgn.Points[k]), new PointD(rgn.Points[k + 1]), pt, vector))
                    return true;
            }

            return false;
        }
        private static void SnapPolyToPoly(GraphicsPolygon rgn, GraphicsPolygon r)
        {
            //var points = r.Points;
            //points = RemoveDuplicatePoints(points);
            //if (r.Points.Length != points.Length)
            //    r.Points = points;

            //for (int i = 0; i < rgn.Points.Length - 1; i++)
            //{
            //    if (IsTooShort(rgn.Points[i], rgn.Points[i + 1]))
            //        continue;

            //    for (int j = 0; j < r.Points.Length - 1; j++)
            //    {
            //        if (IsTooBigAnglePoly(rgn, i, r, j))
            //            continue;

            //        PointD pt1 = new PointD(0, 0), pt2 = new PointD(0, 0),
            //            pt3 = new PointD(0, 0), pt4 = new PointD(0, 0);
            //        double t1 = 0.0, t2 = 0.0;
            //        if (!IsCloseEnoughPoly(rgn, i, r, j, ref pt1, ref pt2, ref t1, ref pt3, ref pt4, ref t2))
            //            continue;

            //        Vector vn = null;
            //        if (IsOpositeDirectionPoly(rgn, i, r, j, ref vn))
            //            continue;

            //        SnapPolyEdgeToPoly(rgn, i, r, j, vn, pt1, pt2, t1, pt3, pt4, t2);
            //        return;
            //    }
            //}
        }

        private static Point[] RemoveDuplicatePoints(Point[] points)
        {
            var k = 0;
            for (var i = 0; i < points.Length - 1; i++)
            {
                if (!IsDoubleEqual(points[i].X, points[i + 1].X) || !IsDoubleEqual(points[i].Y, points[i + 1].Y))
                    k++;
            }

            var pts = new Point[k + 1];
            var j = 0;
            for (var i = 0; i < points.Length - 1; i++)
            {
                if (!IsDoubleEqual(points[i].X, points[i + 1].X) || !IsDoubleEqual(points[i].Y, points[i + 1].Y))
                {
                    pts[j] = points[i];
                    j++;
                }
            }
            pts[k] = points[0];
            return pts;
        }

        private static bool IsDoubleEqual(double num1, double num2)
        {
            return Math.Abs(num1 - num2) < double.Epsilon;
        }
    }
}
