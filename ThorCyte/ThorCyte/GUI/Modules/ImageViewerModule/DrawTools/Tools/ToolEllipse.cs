using System.IO;
using System.Windows;
using System.Windows.Input;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolEllipse : ToolObject
    {
        public ToolEllipse()
        {
            MemoryStream stream = new MemoryStream(Properties.Resources.CurEllipse);
            ToolCursor = new Cursor(stream);
        }
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            var point = position;
            AddNewObject(drawingCanvas, new GraphicsEllipse(point, drawingCanvas));
        }
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (!drawingCanvas.IsMouseCaptured) return;
                if (drawingCanvas.Count > 0)
                {
                    drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(position, 5);
                }
            }
        }
        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            base.OnMouseUp(drawingCanvas, e, position);
        }
        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = ToolCursor;
        }
    }
}
