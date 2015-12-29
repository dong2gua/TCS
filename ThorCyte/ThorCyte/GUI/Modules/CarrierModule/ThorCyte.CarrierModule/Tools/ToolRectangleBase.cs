using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;

namespace ThorCyte.CarrierModule.Tools
{
    /// <summary>
    /// Base class for rectangle-based tools
    /// </summary>
    abstract class ToolRectangleBase : ToolObject
    {
        /// <summary>
        /// Set cursor and resize new object.
        /// </summary>
        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!drawingCanvas.IsMouseCaptured) return;
            if (drawingCanvas.Count > 0)
            {
                drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(
                    drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas)), 5);
            }
        }

    }
}
