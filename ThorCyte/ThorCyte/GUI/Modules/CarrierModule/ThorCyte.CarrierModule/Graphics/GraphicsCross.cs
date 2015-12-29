using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.CarrierModule.Common;

namespace ThorCyte.CarrierModule.Graphics
{
    public class GraphicsCross : GraphicsBase
    {
        #region Fields

        private double _xPos;
        private double _yPos;
        private static GraphicsCross _uniqueInstance;

        #endregion


        private GraphicsCross() { }

        public static GraphicsCross GetInstance()
        {
            return _uniqueInstance ?? (_uniqueInstance = new GraphicsCross());
        }

        public override int HandleCount
        {
            get
            {
                return 0;
            }
        }

        public void SetCross(Point pt, Color objectColor, double lineWidth, double actualScale)
        {
            _xPos = pt.X;
            _yPos = pt.Y;
            GraphicsActualScale = actualScale;
            GraphicsLineWidth = lineWidth;
            GraphicsObjectColor = objectColor;

        }


        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            double size = 12 * ActualScale;
            var start = new Point(_xPos - size, _yPos);
            var end = new Point(_xPos + size, _yPos);

            var pen = new Pen(new SolidColorBrush(GraphicsObjectColor), ActualLineWidth);

            DrawFunction.DrawLine(drawingContext, pen, start, end);

            start.X = end.X = _xPos;
            start.Y = _yPos - size;
            end.Y = _yPos + size;

            DrawFunction.DrawLine(drawingContext, pen, start, end);

        }

        public override Point GetHandle(int handleNumber)
        {
            return new Point(0, 0);
        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
        }

        public override void Move(double deltaX, double deltaY)
        {
            _xPos = deltaX;
            _yPos = deltaY;
            RefreshDrawing();
        }

        public override bool Contains(Point point)
        {
            return false;
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            return Cursors.None;
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            return false;
        }

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        public override int MakeHitTest(Point point)
        {
            return -1;
        }

        public override void Scale(double rate)
        {
            _xPos *= rate;
            _yPos *= rate;
        }
    }
}
