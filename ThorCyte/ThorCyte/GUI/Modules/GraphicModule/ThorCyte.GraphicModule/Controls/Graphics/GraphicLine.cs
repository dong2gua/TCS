using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    public class GraphicsLine : GraphicsBase
    {
        #region Properties and Fields

        public override int HandleCount
        {
            get { return 0; }
        }

        public override RegionType GraphicType
        {
            get { return RegionType.Line; }
        }
        
        protected Point lineStart;
        
        public Point Start
        {
            get { return lineStart; }
            set
            {
                lineStart = value;
                RefreshDrawing();
            }
        }

        protected Point lineEnd;
       
        public Point End
        {
            get { return lineEnd; }
            set
            {
                lineEnd = value;
                RefreshDrawing();
            }
        }

        public LineType LineType { get; set; }

        private bool _isShow ;
       
        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                RefreshDrawing();
            }
        }
        
        #endregion

        #region Constructor

        public GraphicsLine(Point start, Point end, double lineWidth, Color objectColor, double xScale,  LineType type)
        {
            lineStart = start;
            lineEnd = end;
            LineType = type;
            _graphicsObjectColor = objectColor;
            _xScale = xScale;
            _graphicsLineWidth = lineWidth;
        }

        #endregion

        #region Methods

        public override void Draw(DrawingContext dc)
        {
            if (!IsShow)
            {
                return;
            }
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            var pen = new Pen(new SolidColorBrush(_graphicsObjectColor), ActualLineWidth);
            DrawHelper.DrawLine(dc, pen, lineStart, lineEnd);
            base.Draw(dc);
        }

        public override void XScaleChanged(double xRate)
        {
            lineStart.X *= xRate;
            lineEnd.X *= xRate;
            RefreshDrawing();
        }

        public override void YScaleChanged(double yRate)
        {
            lineStart.Y *= yRate;
            lineEnd.Y *= yRate;
            RefreshDrawing();
        }

        public override Point GetHandle(int handleNumber)
        {
            return handleNumber == 1 ? lineStart : lineEnd;
        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
            lineEnd = point;
            RefreshDrawing();
        }

        public override void Move(double deltaX, double deltaY)
        {
        }

        public override bool Contains(Point point)
        {
            var g = new LineGeometry(lineStart, lineEnd);
            return g.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            return Cursors.SizeWE;
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            var rg = new RectangleGeometry(rectangle);
            var lg = new LineGeometry(lineStart, lineEnd);
            var widen = lg.GetWidenedPathGeometry(new Pen(Brushes.Black, LineHitTestWidth));
            var p = Geometry.Combine(rg, widen, GeometryCombineMode.Intersect, null);
            return (!p.IsEmpty());
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

        #endregion
    }
}
