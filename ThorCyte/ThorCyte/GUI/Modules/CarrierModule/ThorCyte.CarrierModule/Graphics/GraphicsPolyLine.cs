using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.CarrierModule.Graphics
{
    /// <summary>
    ///  PolyLine graphics object.
    /// </summary>
    public class GraphicsPolyLine : GraphicsBase
    {
        #region Class Members

        // This class member contains all required geometry.
        // It is ready for drawing and hit testing.
        protected PathGeometry PathGeometry;

        // Points from pathGeometry, including StartPoint
        protected Point[] Points;

        static readonly Cursor HandleCursor;

        #endregion Class Members

        #region Constructors

        public GraphicsPolyLine(Point[] points, double lineWidth, Color objectColor, double actualScale, int roomNo)
        {
            Fill(points, lineWidth, objectColor, actualScale);
            RoomId = roomNo;
            //RefreshDrawng();
        }

        static GraphicsPolyLine()
        {
            var stream = new MemoryStream();

            HandleCursor = new Cursor(stream);
        }

        #endregion Constructors

        #region Other Functions

        /// <summary>
        /// Convert geometry to array of points.
        /// </summary>
        void MakePoints()
        {
            Points = new Point[PathGeometry.Figures[0].Segments.Count + 1];

            Points[0] = PathGeometry.Figures[0].StartPoint;

            for (var i = 0; i < PathGeometry.Figures[0].Segments.Count; i++)
            {
                Points[i + 1] = ((LineSegment)(PathGeometry.Figures[0].Segments[i])).Point;
            }

            GraphicsLeft = GraphicsRight = Points[0].X;
            GraphicsTop = GraphicsBottom = Points[0].Y;

            for (var i = 1; i < Points.Length; i++)
            {
                if (GraphicsLeft > Points[i].X)
                {
                    GraphicsLeft = Points[i].X;
                }

                if (GraphicsRight < Points[i].X)
                {
                    GraphicsRight = Points[i].X;
                }

                if (GraphicsTop > Points[i].Y)
                {
                    GraphicsTop = Points[i].Y;
                }

                if (GraphicsBottom < Points[i].Y)
                {
                    GraphicsBottom = Points[i].Y;
                }
            }

        }

        /// <summary>
        /// Return array of points.
        /// </summary>
        public Point[] GetPoints()
        {
            return Points;
        }

        /// <summary>
        /// Convert array of points to geometry.
        /// </summary>
        void MakeGeometryFromPoints(ref Point[] points)
        {
            if (points == null)
            {
                // This really sucks, XML file contains Points object,
                // but list of points is empty. Do something to prevent program crush.

                points = new Point[2];
            }

            var figure = new PathFigure();

            if (points.Length >= 1)
            {
                figure.StartPoint = points[0];
            }

            for (var i = 1; i < points.Length; i++)
            {
                var segment = new LineSegment(points[i], true) { IsSmoothJoin = true };

                figure.Segments.Add(segment);
            }

            PathGeometry = new PathGeometry();

            PathGeometry.Figures.Add(figure);

            MakePoints();   // keep points array up to date
        }

        // Called from constructors
        void Fill(Point[] points, double lineWidth, Color objectColor, double actualScale)
        {
            MakeGeometryFromPoints(ref points);

            GraphicsLineWidth = lineWidth;
            GraphicsObjectColor = objectColor;
            GraphicsActualScale = actualScale;
        }


        /// <summary>
        /// Add new point (line segment)
        /// </summary>
        public void AddPoint(Point point)
        {
            var segment = new LineSegment(point, true) { IsSmoothJoin = true };

            PathGeometry.Figures[0].Segments.Add(segment);

            MakePoints();   // keep points array up to date
        }

        #endregion Other Functions

        #region Overrides

        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            drawingContext.DrawGeometry(
                FillObjectBrush,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                PathGeometry);

            base.Draw(drawingContext);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return PathGeometry.FillContains(point) ||
                PathGeometry.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get
            {
                return PathGeometry.Figures[0].Segments.Count + 1;
            }
        }


        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        public override Point GetHandle(int handleNumber)
        {
            if (handleNumber < 1)
                handleNumber = 1;

            if (handleNumber > Points.Length)
                handleNumber = Points.Length;

            return Points[handleNumber - 1];
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            return HandleCursor;
        }

        /// <summary>
        /// Move handle to new point (resizing).
        /// handleNumber is 1-based.
        /// </summary>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            if (handleNumber == 1)
            {
                PathGeometry.Figures[0].StartPoint = point;
            }
            else
            {
                ((LineSegment)(PathGeometry.Figures[0].Segments[handleNumber - 2])).Point = point;
            }

            MakePoints();

            RefreshDrawing();
        }


        /// <summary>
        /// Move object
        /// </summary>
        public override void Move(double deltaX, double deltaY)
        {
            for (var i = 0; i < Points.Length; i++)
            {
                Points[i].X += deltaX;
                Points[i].Y += deltaY;
            }

            MakeGeometryFromPoints(ref Points);

            RefreshDrawing();
        }


        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        public override int MakeHitTest(Point point)
        {
            if (IsSelected)
            {
                for (var i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (Contains(point))
                return 0;

            return -1;
        }


        #endregion Overrides

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            var rg = new RectangleGeometry(rectangle);

            var p = Geometry.Combine(rg, PathGeometry, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }

        public override void Scale(double rate)
        {
            for (var i = 0; i < Points.Length; i++)
            {
                Points[i].X *= rate;
                Points[i].Y *= rate;
            }
            MakeGeometryFromPoints(ref Points);
        }

    }
}
