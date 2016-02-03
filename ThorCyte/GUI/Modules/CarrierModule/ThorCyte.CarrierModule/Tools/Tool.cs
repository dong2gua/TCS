using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;

namespace ThorCyte.CarrierModule.Tools
{
    /// <summary>
    /// Base class for all drawing tools
    /// </summary>
    abstract class Tool
    {
        public abstract void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e);

        public abstract void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e);

        public abstract void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e);

        public abstract void SetCursor(SlideCanvas drawingCanvas);

        public abstract void OnMouseDown(PlateCanvas drawingCanvas, MouseButtonEventArgs e);

        public abstract void OnMouseMove(PlateCanvas drawingCanvas, MouseEventArgs e);

        public abstract void OnMouseUp(PlateCanvas drawingCanvas, MouseButtonEventArgs e);

        public abstract void SetCursor(PlateCanvas drawingCanvas);

    }
}
