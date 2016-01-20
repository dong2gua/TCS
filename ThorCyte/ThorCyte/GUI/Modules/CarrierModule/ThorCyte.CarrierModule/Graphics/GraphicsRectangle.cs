using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ThorCyte.CarrierModule.Common;

namespace ThorCyte.CarrierModule.Graphics
{
    class GraphicsRectangle : GraphicsRectangleBase
    {
        #region Constructors

        public GraphicsRectangle(double left, double top, double right, double bottom,
            double lineWidth, Color objectColor, double actualScale, int roomNo)
        {
            RectangleLeft = left;
            RectangleTop = top;
            RectangleRight = right;
            RectangleBottom = bottom;
            GraphicsLineWidth = lineWidth;
            GraphicsObjectColor = objectColor;
            GraphicsActualScale = actualScale;
            RoomId = roomNo;
        }

        #endregion Constructors

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

            DrawFunction.DrawRectangle(drawingContext,
                FillObjectBrush,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                Rectangle);
            base.Draw(drawingContext);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return Rectangle.Contains(point);
        }

        public override string ToString()
        {
            return "Rectangle";
        }
        #endregion Overrides
    }
}
