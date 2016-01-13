using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ImageProcess.DataType
{
    public enum BlobType { Data, Contour, Background, Peripheral };
   
    public class Blob:ICloneable
    {
        #region Field

        private const double Tolerance = 1e-6;
        private Point _centroid = default(Point);
        private readonly List<Point> _points = new List<Point>();
        private readonly List<VLine> _lines;
        private readonly PathGeometry _pathGeometry;
        private double _area = -1;
        private Rect _bound;
        private bool _concave;
        #endregion

        public int Id { get; set; }

     

        public Point[] PointsArray
        {
            get { return _points.ToArray(); }
        }

        public Int32Rect Bound
        {
            get { return new Int32Rect((int) _bound.X, (int) _bound.Y, (int) _bound.Width, (int) _bound.Height); }
        }

        public VLine[] Lines
        {
            get { return _lines.ToArray(); }
        }


        public Blob(IList<Point> points)
        {
            _points.AddRange(points);
            _lines = new List<VLine>(points.Count);
            _pathGeometry = MakeGeometryFromPoints(_points);
            _bound = _pathGeometry.Bounds;
            InitContourLines();
        }

        public Blob()
        {
            
        }

        public Point Centroid()
        {
            if (_centroid.Equals(default(Point)))
                ComputeCentroid();
            return _centroid;
        }

        public double Area()
        {
            if (_area < 0)
            {
                _area = _pathGeometry.GetArea();
            }
            return _area;
        }

        public bool IsVisible(Point point)
        {
            return _pathGeometry.FillContains(point);
        }

       
        private void ComputeCentroid()
        {
            int count = _points.Count;
            if (count <= 0) return;
            for (var i = 0; i < count; i++)
            {
                _centroid.X += _points[i].X;
                _centroid.Y += _points[i].Y;
            }
            _centroid.X /= count;
            _centroid.Y /= count;
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
            figure.IsClosed = true;
            var pathGeometry = new PathGeometry();

            pathGeometry.Figures.Add(figure);

            return pathGeometry;
        }


      

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Blob Clone()
        {
            return new Blob(_points);
        }

        private void InitContourLines()
        {
            _lines.Clear();
            Point[] sortedPoints = _points.OrderBy(pt => pt.X).ThenBy(pt => pt.Y).ToArray();
            Point pt1 = sortedPoints[0];
            Point pt2 = pt1;

            if (!_concave)	// do not check concaveness
            {
                foreach (Point pt in sortedPoints)
                {
                    if (Math.Abs(pt.X - pt1.X) > Tolerance)	//	start of new line
                    {
                        _lines.Add(new VLine((int) pt1.X, (int) pt1.Y, (int) pt2.Y));
                        pt1 = pt2 = pt;
                    }
                    else
                        pt2 = pt;
                }

                // last line
                _lines.Add(new VLine((int)pt1.X, (int)pt1.Y, (int)pt2.Y));
            }
          
        }
    }
}
