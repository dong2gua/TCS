using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolDragger : Tool
    {
        private Point _lastPoint = new Point(0, 0);

        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            _lastPoint = position;
            drawingCanvas.CaptureMouse();
        }

        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = position;
                var dx = point.X - _lastPoint.X;
                var dy = point.Y - _lastPoint.Y;

                _lastPoint = point;
                drawingCanvas.Drag(dx, dy);
            }
        }

        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            drawingCanvas.ReleaseMouseCapture();
        }

        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = Cursors.Hand;
        }
    }
}
