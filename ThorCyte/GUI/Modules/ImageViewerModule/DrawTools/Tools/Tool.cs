using System.Windows;
using System.Windows.Input;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    internal abstract class Tool
    {
        protected Cursor ToolCursor { get; set; }
        public abstract void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position);
        public abstract void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position);
        public abstract void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position);
        public abstract void SetCursor(DrawingCanvas drawingCanvas);
    }
}
