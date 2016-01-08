using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using System;
namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolRuler:Tool
    {
        public GraphicsRuler Ruler { get; private set; }
        public ToolRuler()
        {
            Ruler = new GraphicsRuler();
        }
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            var point = position;
            Ruler.UpdatePoint(point, drawingCanvas);
            AddNewObject(drawingCanvas, Ruler);

        }

        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            var point = position;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!drawingCanvas.IsMouseCaptured) return;
                if (Ruler != null)
                {
                    Ruler.MoveHandleTo(point, 2);
                }
            }
        }

        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (drawingCanvas.Count > 0)
            {
                var obj = drawingCanvas[drawingCanvas.Count - 1];
                obj.Normalize();
            }

            //drawingCanvas.Tool = ToolType.Pointer;
            drawingCanvas.Cursor = Cursors.Arrow;
            drawingCanvas.ReleaseMouseCapture();
        }
        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase o)
        {
            drawingCanvas.UnselectAll();
            o.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth / drawingCanvas.ActualScale.Item1, drawingCanvas.ActualHeight / drawingCanvas.ActualScale.Item2));
            o.RefreshDrawing();
            if (!drawingCanvas.GraphicsList.Contains(o))
                drawingCanvas.GraphicsList.Add(o);
            drawingCanvas.CaptureMouse();
        }

        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = Cursors.IBeam;
        }

    }
}
