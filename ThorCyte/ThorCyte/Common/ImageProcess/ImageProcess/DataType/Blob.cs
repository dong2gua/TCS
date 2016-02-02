using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ImageProcess.DataType
{
    public enum BlobType { Data, Contour, Background, Peripheral };
    public enum RegionShape { None, Range, Rectangle, Ellipse, Polygon, Contour, Ring, Quad }
    public class Blob : ICloneable
    {
        #region Fields

        private const double Tolerance = 1e-6;
        private Point _centroid = default(Point);
        private readonly List<Point> _points = new List<Point>();
        private readonly List<Point> _points2 = new List<Point>(); 
        private readonly List<VLine> _lines = new List<VLine>();
        private PathGeometry _pathGeometry;
        private bool _concave;
        private RegionShape _shape = RegionShape.Contour;
        #endregion

        #region Properties

        public int Id { get; set; }


        public Point[] PointsArray
        {
            get { return _points.ToArray(); }
        }

        public Int32Rect Bound { get; private set; }

        public VLine[] Lines
        {
            get { return _lines.ToArray(); }
        }

        public int PointCapacity
        {
            get { return _points.Capacity; }
        }

        public int Area { get; private set; }
        public int EventId { get; set; }
        //public double AreaByGeometry
        //{
        //    get { return _pathGeometry.GetArea(); }
        //}
        #endregion

        #region Constructors

        public Blob(IEnumerable<Point> points)
        {          
            AddContours(points);
        }

        public Blob(int capacity, int id)
        {
           
            Id = id;
            _points.Capacity = capacity;
        }

        public Blob()
        {
            
        }

        #endregion

        #region Methods

        #region Public

        public bool Contains(Blob blob)
        {
            return _pathGeometry.FillContains(blob._pathGeometry);
        }

        public void FlipH(int width)
        {
            int n = _points.Count;
            for(int i=0; i<n; i++)
            {
                _points[i] = new Point(width - _points[i].X - 1, _points[i].Y);
            }
        }
        public void AddContours(IEnumerable<Point> points)
        {
            _points.Clear();
            _points.AddRange(points);
            _points.Sort(SortCornersClockwise);
            _pathGeometry = MakeGeometryFromPoints();
            //Rect bound = _pathGeometry.Bounds;
            //Bound = new Int32Rect((int) bound.X, (int) bound.Y, (int) bound.Width, (int) bound.Height);
            Bound = RecalcPolygonBounds();
            InitContourLines();
            Area = ComputePolygonArea();
        }

        public Point Centroid()
        {
            if (_centroid.Equals(default(Point)))
                ComputeCentroid();
            return _centroid;
        }

        public bool TouchesEdge(int extent, int width, int height)
        {
            return (Bound.X < extent || Bound.X + Bound.Width + extent - 1 >= width ||
                    Bound.Y < extent || Bound.Y + Bound.Height + extent - 1 >= height);
        }	
        // create a blob expanded by the specified pixels, but clipped by the specified image size
        // if image size is 0, do not clip
        public Blob CreateExpanded(int addedPixels, int imgWidth, int imgHeight)
        {
            if (addedPixels < 0)
                throw new ArgumentOutOfRangeException("addedPixels", "addedPixels must be larger than 0");
            else if (addedPixels == 0)
                return Clone();
            else
            {
                int xStart = Bound.X - addedPixels;
                int width = Bound.Width + 2 * addedPixels;

                var yMin = new ushort[width];
                var yMax = new ushort[width];

                for (int i = 0; i < width; i++)
                    yMin[i] = ushort.MaxValue;

                foreach (VLine line in Lines)
                {
                    int index = line.X - xStart;
                    var yStart = (ushort)(line.Y1 - addedPixels);
                    var yEnd = (ushort)(line.Y2 + addedPixels);

                    // We enlarge by Y +/- nPixelsAdded at the index.	
                    if (yMin[index] > yStart)
                        yMin[index] = yStart;

                    if (yMax[index] < yEnd)
                        yMax[index] = yEnd;

                    int low = index - 1;
                    int high = index + 1;
                    for (int i = 1; i <= addedPixels; i++, low--, high++)
                    {
                        yStart++;
                        yEnd--;

                        if (yMin[low] > yStart)
                            yMin[low] = yStart;

                        if (yMax[low] < yEnd)
                            yMax[low] = yEnd;

                        if (yMin[high] > yStart)
                            yMin[high] = yStart;

                        if (yMax[high] < yEnd)
                            yMax[high] = yEnd;
                    }
                }

                // Now, small blobs with big slops can have gaps (think of a blob with one pixel and a slop > 2).
                // We work outward to find the missing spots.
                int half = width / 2;
                ushort nYMinLow = yMin[half];
                ushort nYMaxLow = yMax[half];
                ushort nYMinHigh = yMin[half];
                ushort nYMaxHigh = yMax[half];

                for (int i = 1; i < half; i++)
                {
                    // Note that the first and last indexes are always done.
                    int high = half + i;
                    if (yMax[high] == ushort.MaxValue)
                    {
                        yMin[high] = nYMinHigh;
                        yMax[high] = nYMaxHigh;
                    }
                    else
                    {
                        nYMinHigh = yMin[high];
                        nYMaxHigh = yMax[high];
                    }

                    int low = half - i;
                    if (yMax[low] == ushort.MaxValue)
                    {
                        yMin[low] = nYMinLow;
                        yMax[low] = nYMaxLow;
                    }
                    else
                    {
                        nYMinLow = yMin[low];
                        nYMaxLow = yMax[low];
                    }
                }

                // Now turn these into lines of new blob.
                var expBlob = new Blob();
              

                int sx = Bound.X - addedPixels;
                bool clipped = false;

                for (int i = 0; i < width; i++)
                {
                    int nX = sx + i;
                    if (imgWidth > 0 && imgHeight > 0)
                    {
                        if (nX < 0)
                        {
                            clipped = true;
                            continue;
                        }

                        if (nX >= imgWidth)
                        {
                            clipped = true;
                            continue;
                        }

                        if (yMax[i] >= imgHeight)
                        {
                            clipped = true;
                            yMax[i] = (ushort)(imgHeight - 1);
                        }
                    }

                    var line = new VLine(nX, yMin[i], yMax[i]);
                    expBlob._lines.Add(line);
                    expBlob.Area += line.Length;
                }

                if (clipped) // recompute bounds and centroid
                {
                    expBlob.ComputeCentroid();

                    VLine ln0 = expBlob._lines[0];
                    int top = ln0.Y1;
                    int bottom = ln0.Y2;
                    int left = ln0.X;
                    int right = ln0.X;

                    foreach (VLine line in expBlob._lines)
                    {
                        if (line.X > right)
                            right = line.X;
                        else if (line.X < left)
                            left = line.X;

                        if (line.Y2 > bottom)
                            bottom = line.Y2;

                        if (line.Y1 < top)
                            top = line.Y1;
                    }

                    int x = left;
                    int y = top;
                    int w = right - Bound.X + 1;
                    int h = bottom - top + 1;
                    expBlob.Bound = new Int32Rect(x, y, w, h);
                }
                else
                {
                    expBlob._centroid = _centroid;
                    int x = Bound.X - addedPixels;
                    int y = Bound.Y - addedPixels;
                    int w = Bound.Width + 2 * addedPixels;
                    int h = Bound.Height + 2 * addedPixels;
                    expBlob.Bound = new Int32Rect(x, y, w, h);
                }

                // create array of points describing blob as polygon
                int count = expBlob._lines.Count * 2 + 1;
                int up = 0;
                int down = count - 2;
                expBlob._points.Capacity = count;
                var points = new Point[count];
                foreach (VLine line in expBlob._lines)
                {
                    //expBlob._points[up++] = new Point(line.X, line.Y1);
                    //expBlob._points[down--] = new Point(line.X, line.Y2);
                    points[up++] = new Point(line.X, line.Y1);
                    points[down--] = new Point(line.X, line.Y2);
                }
                expBlob._points.AddRange(points);
                
                // repeat the first point
                VLine ln = expBlob._lines[0];
                expBlob._points[count - 1] = new Point(ln.X, ln.Y1);
                expBlob._pathGeometry = expBlob.MakeGeometryFromPoints(expBlob._points);
                return expBlob;
            }
           
        }

        public Blob CreateRing(int dist, int width, bool concave, int imgWidth, int imgHeight)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Cannot create ring with 0 or negative width.");
            if (dist <= 0)
                throw new ArgumentOutOfRangeException("dist");
            // We create two expanded blobs: they have the inner and outer contours.

            Blob blobInside = CreateExpanded(dist, imgWidth, imgHeight);
            Blob blobOutside = CreateExpanded(dist + width, imgWidth, imgHeight);
            var ring = new Blob {_shape = RegionShape.Ring, Bound = blobOutside.Bound};
            ring._points.Clear();
            ring._points.AddRange(blobInside._points);
            ring._points2.Clear();
            ring._points2.AddRange(blobInside._points); 
            //ring.m_area = blobOutside.Area - blobInside.Area;		// TODO: this should be the same as the recalculated area

            {
                // lines before the inner blob.
                int nXInsideStart = blobInside.Bound.X;
                int nx = 0;
                foreach (VLine line in blobOutside._lines)
                {
                    if (line.X == nXInsideStart)
                        break;

                    ring._lines.Add(line);
                    ring.Area += line.Length;
                    nx++;
                }

                // These are the lines that overlap the inner blob.
                foreach (VLine lineInside in blobInside._lines)
                {
                    VLine lineOutside = blobOutside._lines[nx++];

                    int nYStartTop = lineOutside.Y1;
                    int nYEndTop = lineInside.Y1 - 1;
                    int nYStartBottom = lineInside.Y2 + 1;
                    int nYEndBottom = lineOutside.Y2;

                    var line = new VLine(lineOutside.X, nYStartTop, nYEndTop);
                    ring._lines.Add(line);
                    ring.Area += line.Length;

                    line = new VLine(lineOutside.X, nYStartBottom, nYEndBottom);
                    ring._lines.Add(line);
                    ring.Area += line.Length;
                }

                // Here are the trailing lines.
                for (int i = nx; i < blobOutside._lines.Count; i++)
                {
                    VLine line = blobOutside._lines[i];
                    ring._lines.Add(line);
                    ring.Area += line.Length;
                }
            }
           
            return ring;
        }

        // This does the dynamic background method on the pixels within the (typically perimeter) blob.
        public int ComputeDynamicBackground(ImageData data, int lowPct, int highPct, int rejectPct)
        {
            //int xOffset = img.GetOffset(channel);
            //ushort[,] buf = img.GetBuffer(channel);

            // collect all of the pixels in the blob into the array.
            var pixels = new List<ushort>(Area);

            foreach (VLine line in _lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    //pixels[index++] = buf[xOffset + line.X, line.Y1 + i];
                    ushort value = data[(line.Y1 + i) * data.XSize + line.X];
                    pixels.Add(value);
                }
                   
                    
            }
            int pixelCount = pixels.Count;
            int index;
            pixels.Sort();
            //Array.Sort(pixels);
            int lowIndex = (pixelCount * lowPct + 50) / 100;
            int highIndex = (pixelCount * highPct + 50) / 100;

            if (highIndex < lowIndex)
                highIndex = lowIndex;

            if (highIndex >= pixelCount)
                highIndex = pixelCount - 1;

            if (lowIndex >= pixelCount)
                lowIndex = pixelCount - 1;

            int total = 0;
            for (index = lowIndex; index <= highIndex; index++)
                total += pixels[index];

            int size = highIndex - lowIndex + 1;
            total += size / 2;
            total /= size;

            int diff = pixels[highIndex] - pixels[lowIndex];
            int average = (pixels[highIndex] + pixels[lowIndex]) / 2 + 1;

            if (diff > (average * rejectPct / 100))	// background is rejected
                total = 0;

            return total;
        }

        public int Perimeter(double pixelWidth, double pixelHeight)
        {
            double peri = 0;

            if (_shape == RegionShape.Polygon || _shape == RegionShape.Contour)
            {
                Point pt1 = _points[0];
                foreach (Point pt2 in _points)
                {
                    double dx = (pt2.X - pt1.X) * pixelWidth;
                    double dy = (pt2.Y - pt1.Y) * pixelHeight;
                    peri += Math.Sqrt((dx * dx) + (dy * dy));
                    pt1 = pt2;
                }
            }
            else if (_shape == RegionShape.Rectangle)
                peri = (Bound.Width + Bound.Height) * 2;
            else if (_shape == RegionShape.Ellipse)	// Ellipse = (PI * long axis * short axis) / 4
                peri = Math.PI * Bound.Width * Bound.Height / 4;

            return (int)peri;
        }


        /// <summary>
        /// Computes the mean of X and the mean of Y for each coordinate within the boundaries of the blob.
        /// </summary>
        /// <param name="xMean"></param>
        /// <param name="yMean"></param>
        public void ComputeXyMean(out float xMean, out float yMean)
        {
            yMean = 0;
            xMean = 0;
            int count = 0;

            if (Area <= 0 ) return;

            float ySum = 0;
            float xSum = 0;

            foreach (VLine line in Lines) //step thru verticle lines of blob
            {
                for (int i = 0; i < line.Length; i++) //step down thru Y coords of line
                {
                    ySum += line.Y1 + i;
                    xSum += line.X; //could be calc. as (line.Length * Line.X) ?
                    count++;
                }
            }

            yMean = ySum / count;
            xMean = xSum / count;

        }

        public void ComputeCovarianceElements(float xMean, float yMean, out float oxx, out float oyy, out float oxy)
        {
            oxx = 0;
            oyy = 0;
            oxy = 0;

            if (Area <= 0 ) return;

            int count = 0;
            float xxSum = 0;
            float yySum = 0;
            float xySum = 0;

            foreach (VLine line in Lines) //step thru verticle lines of blob
            {
                for (int i = 0; i < line.Length; i++) //step down thru Y coords of line
                {
                    xxSum += (line.X - xMean) * (line.X - xMean);
                    yySum += (line.Y1 + i - yMean) * (line.Y1 + i - yMean);
                    xySum += (line.X - xMean) * (line.Y1 + i - yMean);
                    count++;
                }
            }

            if (count > 1) //prevent divide by 0
            {
                oxx = xxSum / (count - 1);
                oyy = yySum / (count - 1);
                oxy = xySum / (count - 1);
            }

        }

        //public bool IsVisible(Point pt)
        //{
        //    if (Bound.Contains(pt) == false) return false;
        //    if (_lines != null)
        //    {
        //        return
        //            _lines.Where(line => Math.Abs(pt.X - line.X) < Tolerance)
        //                .Any(line => pt.Y >= line.Y1 && pt.Y <= line.Y2);
        //    }
        //    return false;
        //}

        public bool IsVisible(Point pt)
        {
            return _pathGeometry.FillContains(pt);
        }
        #endregion

        #region Private

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

        private Int32Rect RecalcPolygonBounds()
        {
            // determine bounding box from boundary points         
            var right = (int)_points.Max(p => p.X);
            var left = (int) _points.Min(p => p.X);
            var top = (int) _points.Min(p => p.Y);
            var bottom = (int) _points.Max(p => p.Y);
            int x = left;
            int y = top;
            int w = right - left + 1;
            int h = bottom - top + 1;
            return new Int32Rect(x, y, w, h);

        }

        private PathGeometry MakeGeometryFromPoints()
        {
            return MakeGeometryFromPoints(_points);
        }



        private PathGeometry MakeGeometryFromPoints(List<Point> points)
        {
            if (points == null || points.Count <= 2)
            {
                throw new ArgumentException("points less than 2");
            }
            points.Sort(SortCornersClockwise);
            var figure = new PathFigure();
            if (points.Count >= 1)
            {
                figure.StartPoint = points[0];
            }

            for (var i = 1; i < points.Count; i++)
            {
                var segment = new LineSegment(points[i], true) { IsSmoothJoin = true };
                figure.Segments.Add(segment);
            }
            figure.IsClosed = true;
            var pathGeometry = new PathGeometry();

            pathGeometry.Figures.Add(figure);

            return pathGeometry;
        }


        private void InitContourLines()
        {
            _lines.Clear();
            _lines.Capacity = _points.Count;
            IEnumerable<Point> sortedPoints = _points.OrderBy(p => p.X).ThenBy(p => p.Y);
            Point pt1 = _points[0];
            Point pt2 = pt1;

            if (!_concave) // do not check concaveness
            {
                foreach (Point pt in sortedPoints)
                {
                    if (Math.Abs(pt.X - pt1.X) > Tolerance) //	start of new line
                    {
                        _lines.Add(new VLine(pt1.X, pt1.Y, pt2.Y));
                        pt1 = pt2 = pt;
                    }
                    else
                        pt2 = pt;
                }

                // last line
                _lines.Add(new VLine(pt1.X, pt1.Y, pt2.Y));
            }

        }

        private int ComputePolygonArea()
        {
         
            double area = 0;
            // create an array of n+2 vertices with V[n]=V[0] and V[n+1]=V[1]				
            int n = _points.Count;
            var pts = new Point[n + 2];

            for (int m = 0; m < n; m++)
                pts[m] = _points[m];

            pts[n] = _points[0];
            pts[n + 1] = _points[1];

            int i, j, k;

            for (i = 1, j = 2, k = 0; i <= n; i++, j++, k++)
                area += pts[i].X * (pts[j].Y - pts[k].Y);

            area /= 2;

            if (area < 0)
                area = -area;
            return (int) area;
        }

        private int SortCornersClockwise(Point a, Point b)
        {
            //  Variables to Store the atans

            //  Reference Point
            Point reference = Centroid();

            //  Fetch the atans
            double aTanA = Math.Atan2(a.Y - reference.Y, a.X - reference.X);
            double aTanB = Math.Atan2(b.Y - reference.Y, b.X - reference.X);

            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }
        #endregion

        #region Interface

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Blob Clone()
        {
            return new Blob(_points);
        }

        #endregion

        #endregion
    }
}
