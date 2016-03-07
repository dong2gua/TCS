using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

       
        public static Blob CreatePhantomBlobs(Point ptCenter, double radius, Size imageSize, Size pixelSize) // jcl-5508
        {
            double pixelWidth = pixelSize.Width;
            double pixelHeight = pixelSize.Height;
            int imgWidth = (int)imageSize.Width;
            int imgHeight = (int) imageSize.Height;

            // convert micron to pixel
            int nXPixels = (int)(radius / pixelWidth);
            int nYPixels = (int)(radius / pixelHeight);

            double dR2 = radius * radius;
            double pixelWidth2 = pixelWidth * pixelWidth;
            int x = (int)ptCenter.X - nXPixels - 1;
            int xEnd = (int)ptCenter.X + nXPixels;

            if (TouchesEdge(x, (int)ptCenter.Y - nYPixels, 2 * nXPixels + 1, 2 * nYPixels, imageSize))
                return null;

            Blob blob = new Blob {_shape = RegionShape.Ellipse};
            int nYMax = 0;
            for (int j = x + 1; j <= xEnd; j++)
            {
                if (j >= imgWidth) break;
                double dX2 = (ptCenter.X - j) * (ptCenter.X - j) * pixelWidth2;
                double dYTmp = (dR2 - dX2) > 0 ? Math.Sqrt(dR2 - dX2) : 0;
                int y = (int)(dYTmp / pixelHeight + 0.1);
                if (nYMax < y) nYMax = y;
                int y1 = (int)ptCenter.Y - y;
                if (y1 < 0) y1 = 0;
                int y2 = (int)ptCenter.Y + y;
                if (y2 >= imgHeight)
                    y2 = imgHeight - 1;
                VLine line = new VLine(j, y1, y2);
                blob._lines.Add(line);
                blob.Area += line.Length;
            }

            blob._centroid = ptCenter;
            blob.Bound = new Int32Rect((int) ptCenter.X - nXPixels, (int) ptCenter.Y - nYMax, 2*nXPixels + 1,
                2*nYMax + 1);

            // create array of points describing blob as polygon
            int count = blob._lines.Count * 2 + 1;
            int up = 0;
            int down = count - 2;
           
            var points = new Point[count];
            foreach (VLine line in blob._lines)
            {
                points[up++] = new Point(line.X, line.Y1);
                points[down--] = new Point(line.X, line.Y2);
            }

            // repeat the first point
            VLine ln = blob._lines[0];
            points[count - 1] = new Point(ln.X, ln.Y1);
            blob.AddContours(points);
            return blob;
        }

        public bool Contains(Blob blob)
        {
            return Contains(this, blob);
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
            //_points.Sort(SortCornersClockwise);
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
                //expBlob._points.AddRange(points);
                
                // repeat the first point
                VLine ln = expBlob._lines[0];
                points[count - 1] = new Point(ln.X, ln.Y1);
                //expBlob._points[count - 1] = new Point(ln.X, ln.Y1);
                expBlob.AddContours(points);
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
        public int ComputeDynamicBackground(short[] data, int width, int lowPct, int highPct, int rejectPct)
        {
            // collect all of the pixels in the blob into the array.
            var pixels = new List<ushort>(Area);

            foreach (VLine line in _lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    //pixels[index++] = buf[xOffset + line.X, line.Y1 + i];
                    ushort value = (ushort) data[(line.Y1 + i)*width + line.X];
                    pixels.Add(value);
                }


            }
            int pixelCount = pixels.Count;
            int index;
            pixels.Sort();
            int lowIndex = (pixelCount*lowPct + 50)/100;
            int highIndex = (pixelCount*highPct + 50)/100;

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
            total += size/2;
            total /= size;

            int diff = pixels[highIndex] - pixels[lowIndex];
            int average = (pixels[highIndex] + pixels[lowIndex])/2 + 1;

            if (diff > (average*rejectPct/100)) // background is rejected
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

        public bool IsVisible(Point pt)
        {
            if (Bound.Contains(pt) == false) return false;
            if (_lines != null)
            {
                return
                    _lines.Where(line => Math.Abs(pt.X - line.X) < Tolerance)
                        .Any(line => pt.Y >= line.Y1 && pt.Y <= line.Y2);
            }
            return false;
        }

       
        #endregion

        #region Private

        private static bool TouchesEdge(int x, int y, int w, int h, Size imageSize)
        {
            int imageWidth = (int) imageSize.Width;
            int imageHeight = (int) imageSize.Height;
            return x < 0 || x + w >= imageWidth || y < 0 || y + h >= imageHeight;            
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


        private void InitContourLines()
        {
            _lines.Clear();
            //_lines.Capacity = _points.Count;
            IEnumerable<Point> sortedPoints = _points.OrderBy(p => p.X).ThenBy(p => p.Y);
            Point pt1 = sortedPoints.First();
            Point pt2 = pt1;
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


        private int ComputePolygonArea()
        {
            //double area = 0;
            ////create an array of n+2 vertices with V[n]=V[0] and V[n+1]=V[1]				
            //int n = _points.Count;
            //var pts = new Point[n + 2];

            //for (int m = 0; m < n; m++)
            //    pts[m] = _points[m];

            //pts[n] = _points[0];
            //pts[n + 1] = _points[1];

            //int i, j, k;

            //for (i = 1, j = 2, k = 0; i <= n; i++, j++, k++)
            //    area += pts[i].X*(pts[j].Y - pts[k].Y);

            //area /= 2;

            //if (area < 0)
            //    area = -area;
            //return (int) area;
            return _lines.Sum(vl => vl.Length);
        }


/*
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
*/

        private static bool PointInsidePolygon(Blob blob, Point point)
        {
            int n = blob._points.Count;
            List<Point> vertex = blob._points;
            if (n <= 2)
                return false;
            bool isInside = false;
            double x = point.X;
            double y = point.Y;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((vertex[i].Y > y) != (vertex[j].Y > y))
                    &&
                    (x <
                     (vertex[j].X - vertex[i].X) * (y - vertex[i].Y) /
                     (vertex[j].Y - vertex[i].Y) + vertex[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        // see: 1. http://gamedev.stackexchange.com/questions/30534/how-do-i-determine-if-one-polygon-completely-contains-another
        //      2. https://github.com/tmpvar/2d-polygon-contains-polygon
        private static bool Contains(Blob container, Blob contained)
        {
            if(container.Area<contained.Area) return false;
            else
            {
                List<Point> containerPoly = container._points;
                List<Point> containedPoly = contained._points;
                bool isInside = containedPoly.Any(point => PointInsidePolygon(container, point));
                if (isInside == false) return false;
                int n = containedPoly.Count;
                int m = containerPoly.Count;
                for (int i = 0; i < n; i++)
                {
                    Point containedStart = containedPoly[i];
                    Point containedEnd = containedPoly[(i + 1)%n];
                    for (int j = 0; j < m; j++)
                    {
                        Point containerStart = containerPoly[j];
                        Point containerEnd = containerPoly[(j + 1)%m];
                        if (DoIntersect(containedStart, containedEnd, containerStart, containerEnd))
                        {
                            return false;
                        }
                    }

                }
                return true;
            }
        }
        
        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        private static bool OnSegment(Point p, Point q, Point r)
        {
            return q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                   q.Y <= Math.Max(p.Y, r.Y) && q.X >= Math.Min(p.Y, r.Y);
        }

        // To find Orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are colinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        private static int Orientation(Point p, Point q, Point r)
        {
            // See http://www.geeksforgeeks.org/Orientation-3-ordered-points/
            // for details of below formula.
            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (Math.Abs(val) < Tolerance) return 0; // colinear
            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // see http://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        // The main function that returns true if line segment 'start1-end1'
        // and 'start2-end2' intersect.
        private static bool DoIntersect(Point start1, Point end1, Point start2, Point end2)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = Orientation(start1, end1, start2);
            int o2 = Orientation(start1, end1, end2);
            int o3 = Orientation(start2, end2, start1);
            int o4 = Orientation(start2, end2, end1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // start1, end1 and start2 are colinear and start2 lies on segment p1q1
            if (o1 == 0 && OnSegment(start1, start2, end1)) return true;

            // start1, end1 and start2 are colinear and end2 lies on segment p1q1
            if (o2 == 0 && OnSegment(start1, end2, end1)) return true;

            // start2, end2 and start1 are colinear and start1 lies on segment p2q2
            if (o3 == 0 && OnSegment(start2, start1, end2)) return true;

            // start2, end2 and end1 are colinear and end1 lies on segment p2q2
            if (o4 == 0 && OnSegment(start2, end1, end2)) return true;

            return false; // Doesn't fall in any of the above cases
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
