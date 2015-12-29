using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsImage : GraphicsBase
    {
        //public GraphicsImage(ImageSource imageSource, Size size)
        //{
        //    _imageSource = imageSource;
        //    _rect = new Rect(new Point(0, 0), size);
        //}
        public GraphicsImage()
        {
        }
        public void SetImage(ImageSource imageSource, Rect rect)
        {
            _imageSource = imageSource;
            RectangleLeft = rect.Left;
            RectangleTop = rect.Top;
            RectangleRight = rect.Right;
            RectangleBottom = rect.Bottom;

        }
        private ImageSource _imageSource;

        public ImageSource ImageSource
        {
            get { return _imageSource; }
        }


        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawImage(ImageSource, Rectangle);
        }
        public override int HandleCount { get { return 0; } }
        public override Point GetHandle(int handleNumber) { return new Point(0, 0); }
        public override Cursor GetHandleCursor(int handleNumber) { return Cursors.Arrow; }
        public override void Move(double deltaX, double deltaY)
        {
            RectangleLeft += deltaX;
            RectangleRight += deltaX;
            RectangleTop += deltaY;
            RectangleBottom += deltaY;
            RefreshDrawing();
        }
        public override void MoveHandleTo(Point point, int handleNumber) { }
        public override int MakeHitTest(Point point) { return 0; }
        public override bool Contains(Point point) { return false; }
        public override bool IntersectsWith(Rect rectangle) { return false; }

    }
}
