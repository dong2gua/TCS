using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolRuler : Tool
    {
        public GraphicsRuler Ruler { get; private set; }
        public ToolRuler()
        {
            Ruler = new GraphicsRuler();
            MemoryStream stream = new MemoryStream(Properties.Resources.CurRuler);
            ToolCursor = new Cursor(stream);
        }
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Ruler.UpdatePoint(position, drawingCanvas);
                AddNewObject(drawingCanvas, Ruler);
            }
        }
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!drawingCanvas.IsMouseCaptured) return;
                if (Ruler != null)
                    Ruler.MoveHandleTo(position, 2);
            }
        }
        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            Ruler.Normalize();
            drawingCanvas.ReleaseMouseCapture();
        }
        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase o)
        {
            drawingCanvas.UnselectAll();
            o.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth , drawingCanvas.ActualHeight));
            o.RefreshDrawing();
            if (!drawingCanvas.GraphicsList.Contains(o))
                drawingCanvas.GraphicsList.Add(o);
            drawingCanvas.CaptureMouse();
        }
        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = ToolCursor;
        }
    }
}
