using System.Windows;
using System.Windows.Input;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public abstract class GraphicsRectangleBase : GraphicsBase
    {
        public override int HandleCount
        {
            get { return 8; }
        }
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
            
            return ConvertToDisplayPoint(new Point(x, y));
        }
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
                    return Cursors.Arrow;
            }
        }
        public override void Move(double deltaX, double deltaY)
        {
            RectangleLeft += deltaX;
            RectangleRight += deltaX;
            RectangleTop += deltaY;
            RectangleBottom += deltaY;
            RefreshDrawing();
        }
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            point = VerifyPoint(point);
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
        public override int MakeHitTest(Point point)
        {
            if (IsSelected)
            {
                for (var i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(ConvertToDisplayPoint( point)))
                        return i;
                }
            }

            if (Contains(point))
                return 0;

            return -1;
        }
    }
}
