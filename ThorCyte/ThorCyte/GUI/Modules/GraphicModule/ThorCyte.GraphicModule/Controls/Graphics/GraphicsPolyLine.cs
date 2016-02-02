using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    /// <summary>
    ///  PolyLine graphics object.
    /// </summary>
    public class GraphicsPolyLine : GraphicsBase
    {
        #region Properties and Fields

        // This class member contains all required geometry.
        // It is ready for drawing and hit testing.
        protected PathGeometry PathGeometry;

        // Points from _pathGeometry, including StartPoint
        private Point[] _points;

        public Point[] Points
        {
            get { return _points; }
        }

        public override RegionType GraphicType
        {
            get { return RegionType.PolyLine; }
        }

        static Cursor _handleCursor;

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get { return PathGeometry.Figures[0].Segments.Count + 1; }
        }

        #endregion Class Members

        #region Constructors

        public GraphicsPolyLine(Point[] points, double lineWidth, Color objectColor, Size parentSize, string name)
        {
            Fill(points, lineWidth, objectColor);
            CreatedCanvasSize = parentSize;
            _handleCursor = Cursors.Cross;
            Name = name;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convert geometry to array of _points.
        /// </summary>
        void MakePoints()
        {
            _points = new Point[PathGeometry.Figures[0].Segments.Count + 1];
            _points[0] = PathGeometry.Figures[0].StartPoint;

            for (var i = 0; i < PathGeometry.Figures[0].Segments.Count; i++)
            {
                _points[i + 1] = ((LineSegment)(PathGeometry.Figures[0].Segments[i])).Point;
            }

            GraphicsLeft = GraphicsRight = _points[0].X;
            GraphicsTop = GraphicsBottom = _points[0].Y;

            for (var i = 1; i < _points.Length; i++)
            {
                if (GraphicsLeft > _points[i].X)
                {
                    GraphicsLeft = _points[i].X;
                }

                if (GraphicsRight < _points[i].X)
                {
                    GraphicsRight = _points[i].X;
                }

                if (GraphicsTop > _points[i].Y)
                {
                    GraphicsTop = _points[i].Y;
                }

                if (GraphicsBottom < _points[i].Y)
                {
                    GraphicsBottom = _points[i].Y;
                }
            }

        }

        /// <summary>
        /// Convert array of _points to geometry.
        /// </summary>
        void MakeGeometryFromPoints(ref Point[] points)
        {
            if (points == null)
            {
                // This really sucks, XML file contains Points object,
                // but list of _points is empty. Do something to prevent program crush.
                points = new Point[2];
            }

            var figure = new PathFigure();
            if (points.Length >= 1)
            {
                figure.StartPoint = points[0];
            }

            for (int i = 1; i < points.Length; i++)
            {
                var segment = new LineSegment(points[i], true) { IsSmoothJoin = true };
                figure.Segments.Add(segment);
            }

            PathGeometry = new PathGeometry();
            PathGeometry.Figures.Add(figure);
            MakePoints();   // keep _points array up to date
        }

        // Called from constructors
        void Fill(Point[] points, double lineWidth, Color objectColor)
        {
            MakeGeometryFromPoints(ref points);
            _graphicsLineWidth = lineWidth;
            _graphicsObjectColor = objectColor;
        }


        /// <summary>
        /// Add new point (line segment)
        /// </summary>
        public void AddPoint(Point point)
        {
            var segment = new LineSegment(point, true) { IsSmoothJoin = true };
            PathGeometry.Figures[0].Segments.Add(segment);
            MakePoints();   // keep _points array up to date
        }

        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            var brush = new SolidColorBrush(ObjectColor);
            var formatText = new FormattedText(Name, new CultureInfo("en-US"), FlowDirection.LeftToRight,
               new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), FontSize, new SolidColorBrush(Colors.White));
            var xMax = _points[0].X;
            var xMin = _points[0].X;
            var yMax = _points[0].Y;
            var yMin = _points[0].Y;

            foreach (var point in _points)
            {
                if (point.X > xMax)
                {
                    xMax = point.X;
                }
                if (point.X < xMin)
                {
                    xMin = point.X;
                }
                if (point.Y > yMax)
                {
                    yMax = point.Y;
                }
                if (point.Y < yMin)
                {
                    yMin = point.Y;
                }
            }
            
            var center = new Point((xMax + xMin) / 2.0 - formatText.Width / 2.0, (yMax + yMin) / 2.0 - formatText.Height / 2.0);
            dc.DrawGeometry(_fillObjectBrush, new Pen(brush, ActualLineWidth), PathGeometry);
            if (_points.Length > 2)
            {
                dc.DrawText(formatText, center);
            }
            
            base.Draw(dc);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return PathGeometry.FillContains(point) || PathGeometry.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        public override Point GetHandle(int handleNumber)
        {
            if (handleNumber < 1)
                handleNumber = 1;

            if (handleNumber > _points.Length)
                handleNumber = _points.Length;

            return _points[handleNumber - 1];
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            return _handleCursor;
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
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i].X += deltaX;
                _points[i].Y += deltaY;
            }

            MakeGeometryFromPoints(ref _points);
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
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (Contains(point))
                return 0;

            return -1;
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            var rg = new RectangleGeometry(rectangle);
            var p = Geometry.Combine(rg, PathGeometry, GeometryCombineMode.Intersect, null);
            return (!p.IsEmpty());
        }

        public override void XScaleChanged(double rate)
        {
            for (var i = 0; i < _points.Length; i++)
            {
                _points[i].X *= rate;
            }
            MakeGeometryFromPoints(ref _points);
        }


        public override void YScaleChanged(double yRate)
        {
            for (var i = 0; i < _points.Length; i++)
            {
                _points[i].Y *= yRate;
            }
            MakeGeometryFromPoints(ref _points);
        }
        #endregion Other Functions
    }
}
