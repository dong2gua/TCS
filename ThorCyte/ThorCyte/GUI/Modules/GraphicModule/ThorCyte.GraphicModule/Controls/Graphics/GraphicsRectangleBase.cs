﻿using System.Linq;
using System.Windows;
using System.Windows.Input;
using ROIService;
using ROIService.Region;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    public abstract class GraphicsRectangleBase : GraphicsBase
    {
        #region Properties and FIelds

        protected double rectangleLeft;
        protected double rectangleTop;
        protected double rectangleRight;
        protected double rectangleBottom;


         //<summary>
         //Read-only property, returns Rect calculated on the fly from four _points.
         //Points can make inverted rectangle, fix this.
         //</summary>
        public Rect Rectangle
        {
            get
            {
                double l, t, w, h;
                GraphicsLeft = rectangleLeft;
                GraphicsRight = rectangleRight;
                GraphicsTop = rectangleTop;
                GraphicsBottom = rectangleBottom;
                if (rectangleLeft <= rectangleRight)
                {
                    l = rectangleLeft;
                    w = rectangleRight - rectangleLeft;
                }
                else
                {
                    l = rectangleRight;
                    w = rectangleLeft - rectangleRight;
                }

                if (rectangleTop <= rectangleBottom)
                {
                    t = rectangleTop;
                    h = rectangleBottom - rectangleTop;
                }
                else
                {
                    t = rectangleBottom;
                    h = rectangleTop - rectangleBottom;
                }

                return new Rect(l, t, w, h);
            }
            set
            {
                rectangleLeft = value.Left;
                rectangleRight = value.Right;
                rectangleTop = value.Top;
                rectangleBottom = value.Bottom;
                RefreshDrawing();
            }
        }

        public bool IsDrawTrackerAll { get; set; }

        public double Left
        {
            get { return rectangleLeft; }
            set { rectangleLeft = value;  }
        }

        public double Top
        {
            get { return rectangleTop; }
            set { rectangleTop = value; }
        }

        public double Right
        {
            get { return rectangleRight; }
            set { rectangleRight = value; }
        }

        public double Bottom
        {
            get { return rectangleBottom; }
            set { rectangleBottom = value; }
        }

        #endregion

        #region Overrides

         //<summary>
         //Get number of handles
         //</summary>
        public override int HandleCount
        {
            get { return 8; }
        }


        //<summary>
         //Get handle point by 1-based number
         //</summary>
        public override Point GetHandle(int handleNumber)
        {
            double xCenter = (rectangleRight + rectangleLeft) / 2;
            double yCenter = (rectangleBottom + rectangleTop) / 2;
            double x = rectangleLeft;
            double y = rectangleTop;

            switch (handleNumber)
            {
                case 1:
                    x = rectangleLeft;
                    y = rectangleTop;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangleTop;
                    break;
                case 3:
                    x = rectangleRight;
                    y = rectangleTop;
                    break;
                case 4:
                    x = rectangleRight;
                    y = yCenter;
                    break;
                case 5:
                    x = rectangleRight;
                    y = rectangleBottom;
                    break;
                case 6:
                    x = xCenter;
                    y = rectangleBottom;
                    break;
                case 7:
                    x = rectangleLeft;
                    y = rectangleBottom;
                    break;
                case 8:
                    x = rectangleLeft;
                    y = yCenter;
                    break;
            }

            return new Point(x, y);
        }

        //<summary>
         //Hit test.
         //Return value: -1 - no hit
         //               0 - hit anywhere
         //               > 1 - handle number
         //</summary>
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

         //<summary>
         //Get cursor for the handle
         //</summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            if (!IsDrawTrackerAll && handleNumber != 4 && handleNumber != 8)
            {
                return Cursors.None;
            }
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
                    return Cursors.Cross;
            }
        }

         //<summary>
         //Move handle to new point (resizing)
         //</summary>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            if (!IsDrawTrackerAll && handleNumber != 4 && handleNumber != 8)
            {
                return;
            }
            switch (handleNumber)
            {
                case 1:
                    rectangleLeft = point.X;
                    rectangleTop = point.Y;
                    break;
                case 2:
                    rectangleTop = point.Y;
                    break;
                case 3:
                    rectangleRight = point.X;
                    rectangleTop = point.Y;
                    break;
                case 4:
                    rectangleRight = point.X;
                    break;
                case 5:
                    rectangleRight = point.X;
                    rectangleBottom = point.Y;
                    break;
                case 6:
                    rectangleBottom = point.Y;
                    break;
                case 7:
                    rectangleLeft = point.X;
                    rectangleBottom = point.Y;
                    break;
                case 8:
                    rectangleLeft = point.X;
                    break;
            }

            RefreshDrawing();
        }

         //<summary>
         //Test whether object intersects with rectangle
         //</summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            return Rectangle.IntersectsWith(rectangle);
        }

         //<summary>
         //Move object
         //</summary>
        public override void Move(double deltaX, double deltaY)
        {
            rectangleLeft += deltaX;
            rectangleRight += deltaX;

            rectangleTop += deltaY;
            rectangleBottom += deltaY;

            RefreshDrawing();
        }

         //<summary>
         //Normalize rectangle
         //</summary>
        public override void Normalize()
        {
            if (rectangleLeft > rectangleRight)
            {
                double tmp = rectangleLeft;
                rectangleLeft = rectangleRight;
                rectangleRight = tmp;
            }

            if (rectangleTop > rectangleBottom)
            {
                double tmp = rectangleTop;
                rectangleTop = rectangleBottom;
                rectangleBottom = tmp;
            }
        }

        public override void XScaleChanged(double rate)
        {
            rectangleLeft *= rate;
            rectangleRight *= rate;
        }

        public override void YScaleChanged(double yRate)
        {
            rectangleTop *= yRate;
            rectangleBottom *= yRate;
        }


        #endregion Overrides
    }
}
