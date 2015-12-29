using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.Infrastructure.Types
{
    public class Well
    {
        #region Properties

        public int WellId { get; set; } 

        #endregion

        #region Constructors
        public Well(int wellId, RegionShape shape, Rect bound)
        {
            WellId = wellId;
        }
        #endregion
    }

    public class ScanRegion
    {
        public Point[] Points { get; set; }
        public int WellId { get; set; }
        public int RegionId { get; set; }
        public RegionShape ScanRegionShape { get; set; }

        public List<Scanfield> ScanFieldList;

        public Rect Bound { get; set; }

        public ScanRegion(int wellId, int regionId, RegionShape shape, Point[] points)
        {
            WellId = wellId;
            RegionId = regionId;
            ScanRegionShape = shape;
            Points = (Point[]) points.Clone();

            if (Points.Length > 0)
            {
                double minX = Points[0].X, minY = Points[0].Y, maxX = Points[0].X, maxY = Points[0].Y;

                foreach (Point pt in Points)
                {
                    if (pt.X < minX)
                    {
                        minX = pt.X;
                    }

                    if (pt.X > maxX)
                    {
                        maxX = pt.X;
                    }

                    if (pt.Y < minY)
                    {
                        minY = pt.Y;
                    }

                    if (pt.Y > maxY)
                    {
                        maxY = pt.Y;
                    }
                }
                Bound = new Rect(minX, minY, Math.Round(maxX - minX,6), Math.Round(maxY - minY, 6));
            }
            else
            {
                Bound = new Rect(0, 0, 0, 0);
            }

            ScanFieldList = new List<Scanfield>();
        }

#if DEBUG
        public ScanRegion()
        {
        }
#endif

        public void BulidTiles(double width, double height, double xInterval, double yInterval,
            ScanPathType scanPathType)
        {
            Geometry gm;

            if (ScanRegionShape == RegionShape.Rectangle)
                gm = new RectangleGeometry(Bound);
            else if (ScanRegionShape == RegionShape.Ellipse)
                gm = new EllipseGeometry(Bound);
            else if (ScanRegionShape == RegionShape.Polygon)
            {
                gm = MakeGeometryFromPoints(Points);
            }
            else
                return;

            ScanFieldList.Clear();

            double incWidth = width + xInterval;
            double incHeight = height + yInterval;

            if (scanPathType == ScanPathType.Serpentine)
            {
                int row = 0;
                for (double y = Bound.Top; y < Bound.Bottom; y += incHeight, row++)
                {
                    if (row%2 == 0)
                    {
                        for (double x = Bound.Left; x < Bound.Right; x += incWidth)
                        {
                            Rect rect = new Rect(x, y, width, height);
                            Geometry rcGm = new RectangleGeometry(rect);
                            IntersectionDetail id = gm.FillContainsWithDetail(rcGm);
                            if (id != IntersectionDetail.Empty)
                            {
                                Scanfield field = new Scanfield(rect);
                                field.ScanFieldId = ScanFieldList.Count + 1;
                                ScanFieldList.Add(field);
                            }
                        }
                    }
                    else
                    {
                        for (double x = Bound.Right - incWidth; Math.Round(x, 6) >= Math.Round(Bound.Left, 6); x -= incWidth)
                        {
                            Rect rect = new Rect(x, y, width, height);
                            Geometry rcGm = new RectangleGeometry(rect);
                            IntersectionDetail id = gm.FillContainsWithDetail(rcGm);
                            if (id != IntersectionDetail.Empty)
                            {
                                Scanfield field = new Scanfield(rect);
                                field.ScanFieldId = ScanFieldList.Count + 1;
                                ScanFieldList.Add(field);
                            }
                        }
                    }

                }
            }
            else
            {
                for (double y = Bound.Top; y < Bound.Bottom; y += incHeight)
                {
                    for (double x = Bound.Left; x < Bound.Right; x += incWidth)
                    {
                        Rect rect = new Rect(x, y, width, height);
                        Geometry rcGm = new RectangleGeometry(rect);
                        IntersectionDetail id = gm.FillContainsWithDetail(rcGm);
                        if (id != IntersectionDetail.Empty)
                        {
                            Scanfield field = new Scanfield(rect);
                            field.ScanFieldId = ScanFieldList.Count + 1;
                            ScanFieldList.Add(field);
                        }
                    }
                }
            }

        }

        private PathGeometry MakeGeometryFromPoints(IList<Point> points)
        {
            if (points == null || points.Count <= 2)
            {

                throw new ArgumentException("points less than 2");
            }

            var figure = new PathFigure();

            if (points.Count >= 1)
            {
                figure.StartPoint = points[0];
            }

            for (var i = 1; i < points.Count; i++)
            {
                var segment = new LineSegment(points[i], true) {IsSmoothJoin = true};

                figure.Segments.Add(segment);
            }

            var pathGeometry = new PathGeometry();

            pathGeometry.Figures.Add(figure);

            return pathGeometry;
        }

    }

    public class Scanfield
    {
        #region Properties

        public int ScanFieldId { set; get; }
        public Rect SFRect { set; get; }

        #endregion

        #region Constructors
        public Scanfield(double x, double y, double width, double height)
        {
            SFRect = new Rect(x, y, width, height);
        }

        public Scanfield(Rect rc)
        {
            SFRect = rc;
        }

#if DEBUG
        public Scanfield()
        {
            
        }
#endif

        #endregion
    }
}
