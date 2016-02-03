using System.IO;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;
using ThorCyte.CarrierModule.Graphics;

namespace ThorCyte.CarrierModule.Tools
{
    /// <summary>
    /// Ellipse tool
    /// </summary>
    class ToolEllipse : ToolRectangleBase
    {
        public ToolEllipse()
        {
            var stream = new MemoryStream(Properties.Resources.Ellipse);
            ToolCursor = new Cursor(stream);
        }

        /// <summary>
        /// Create new rectangle
        /// </summary>
        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var p = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));

            AddNewObject(drawingCanvas,
                new GraphicsEllipse(
                p.X,
                p.Y,
                p.X + 1,
                p.Y + 1,
                drawingCanvas.LineWidth,
                drawingCanvas.ObjectColor,
                drawingCanvas.ActualScale,
                drawingCanvas.CurrentRoomNo));
        }
    }
}
