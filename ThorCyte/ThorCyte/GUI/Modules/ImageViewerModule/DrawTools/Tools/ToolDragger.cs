using System.Windows;
using System.Windows.Input;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolDragger : Tool
    {
        private Point? _lastPoint = null;
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed )
            {
                _lastPoint = position;
                drawingCanvas.CaptureMouse();
            }
        }
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed&&_lastPoint.HasValue)
            {
                var dx = position.X - _lastPoint.Value.X;
                var dy = position.Y - _lastPoint.Value.Y;
                _lastPoint = position;
                drawingCanvas.Drag(dx, dy);
            }
        }
        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            drawingCanvas.ReleaseMouseCapture();
            _lastPoint = null;
        }
        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = Cursors.Hand;
        }
    }
}
