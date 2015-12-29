using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolLine// : ToolObject
    {
        //public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        //{
        //    var point = position;
        //    var line = new GraphicsLine(point, new Point(point.X + 1, point.Y + 1), drawingCanvas.ActualScale);
        //    AddNewObject(drawingCanvas, line);
        //}

        //public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        //{
        //    var point = position;
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        if (!drawingCanvas.IsMouseCaptured) return;
        //        if (drawingCanvas.Count > 0)
        //        {
        //            drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(point, 2);
        //        }
        //    }
        //}

        //public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        //{
        //    base.OnMouseUp(drawingCanvas, e, position);
        //}
    }

}
