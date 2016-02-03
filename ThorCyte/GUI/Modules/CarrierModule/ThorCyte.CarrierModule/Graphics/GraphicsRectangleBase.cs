using System.Windows;
using System.Windows.Input;

namespace ThorCyte.CarrierModule.Graphics
{
    public abstract class GraphicsRectangleBase : GraphicsBase
    {
        #region Class Members

        protected double RectangleLeft;
        protected double RectangleTop;
        protected double RectangleRight;
        protected double RectangleBottom;

        #endregion Class Members

        #region Properties

        /// <summary>
        /// Read-only property, returns Rect calculated on the fly from four points.
        /// Points can make inverted rectangle, fix this.
        /// </summary>
        public Rect Rectangle
        {
            get
            {
                double l, t, w, h;
                GraphicsLeft = RectangleLeft;
                GraphicsRight = RectangleRight;
                GraphicsTop = RectangleTop;
                GraphicsBottom = RectangleBottom;
                if (RectangleLeft <= RectangleRight)
                {
                    l = RectangleLeft;
                    w = RectangleRight - RectangleLeft;
                }
                else
                {
                    l = RectangleRight;
                    w = RectangleLeft - RectangleRight;
                }

                if (RectangleTop <= RectangleBottom)
                {
                    t = RectangleTop;
                    h = RectangleBottom - RectangleTop;
                }
                else
                {
                    t = RectangleBottom;
                    h = RectangleTop - RectangleBottom;
                }

                return new Rect(l, t, w, h);
            }
        }

        public double Left
        {
            get { return RectangleLeft; }
            set { RectangleLeft = value; }
        }

        public double Top
        {
            get { return RectangleTop; }
            set { RectangleTop = value; }
        }

        public double Right
        {
            get { return RectangleRight; }
            set { RectangleRight = value; }
        }

        public double Bottom
        {
            get { return RectangleBottom; }
            set { RectangleBottom = value; }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        public override Point GetHandle(int handleNumber)
        {
            var xCenter = (RectangleRight + RectangleLeft) / 2;
            var yCenter = (RectangleBottom + RectangleTop) / 2;
            var x = RectangleLeft;
            var y = RectangleTop;

            switch (handleNumber)
            {
                case 1:
                    x = RectangleLeft;
                    y = RectangleTop;
                    break;
                case 2:
                    x = xCenter;
                    y = RectangleTop;
                    break;
                case 3:
                    x = RectangleRight;
                    y = RectangleTop;
                    break;
                case 4:
                    x = RectangleRight;
                    y = yCenter;
                    break;
                case 5:
                    x = RectangleRight;
                    y = RectangleBottom;
                    break;
                case 6:
                    x = xCenter;
                    y = RectangleBottom;
                    break;
                case 7:
                    x = RectangleLeft;
                    y = RectangleBottom;
                    break;
                case 8:
                    x = RectangleLeft;
                    y = yCenter;
                    break;
            }

            return new Point(x, y);
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



        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return HelperFunctions.DefaultCursor;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    RectangleLeft = point.X;
                    RectangleTop = point.Y;
                    break;
                case 2:
                    RectangleTop = point.Y;
                    break;
                case 3:
                    RectangleRight = point.X;
                    RectangleTop = point.Y;
                    break;
                case 4:
                    RectangleRight = point.X;
                    break;
                case 5:
                    RectangleRight = point.X;
                    RectangleBottom = point.Y;
                    break;
                case 6:
                    RectangleBottom = point.Y;
                    break;
                case 7:
                    RectangleLeft = point.X;
                    RectangleBottom = point.Y;
                    break;
                case 8:
                    RectangleLeft = point.X;
                    break;
            }

            RefreshDrawing();
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            return Rectangle.IntersectsWith(rectangle);
        }

        /// <summary>
        /// Move object
        /// </summary>
        public override void Move(double deltaX, double deltaY)
        {
            RectangleLeft += deltaX;
            RectangleRight += deltaX;

            RectangleTop += deltaY;
            RectangleBottom += deltaY;

            RefreshDrawing();
        }

        /// <summary>
        /// Normalize rectangle
        /// </summary>
        public override void Normalize()
        {
            if (RectangleLeft > RectangleRight)
            {
                var tmp = RectangleLeft;
                RectangleLeft = RectangleRight;
                RectangleRight = tmp;
            }

            if (RectangleTop > RectangleBottom)
            {
                var tmp = RectangleTop;
                RectangleTop = RectangleBottom;
                RectangleBottom = tmp;
            }
        }

        public override void Scale(double rate)
        {
            RectangleLeft *= rate;
            RectangleTop *= rate;
            RectangleRight *= rate;
            RectangleBottom *= rate;
        }

        #endregion Overrides
    }
}
