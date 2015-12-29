using System.Windows.Input;
using System.Windows;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    internal abstract class Tool
    {
        public abstract void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position);

        public abstract void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position);

        public abstract void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position);

        public abstract void SetCursor(DrawingCanvas drawingCanvas);
    }
}
