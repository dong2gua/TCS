using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.CarrierModule.Canvases;
using ThorCyte.CarrierModule.Graphics;

namespace ThorCyte.CarrierModule.Tools
{
    class ToolCross : Tool
    {
        private readonly Cursor _toolCursor;

        public ToolCross()
        {
            var stream = new MemoryStream(Properties.Resources.CyteCross);
            _toolCursor = new Cursor(stream);
        }

        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var o = GraphicsCross.GetInstance();
            if (!drawingCanvas.GraphicsList.Contains(o))
            {
                o.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.Width, drawingCanvas.Height));
                o.SetCross(new Point(0, 0), drawingCanvas.ObjectColor, drawingCanvas.LineWidth, drawingCanvas.ActualScale);
                drawingCanvas.GraphicsList.Add(o);
            }
            var p = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));
            o.Move(p.X, p.Y);
            drawingCanvas.MovePostion(p.X, p.Y);
        }

        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = _toolCursor;
        }

        public override void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            if (drawingCanvas.IsStick) return;
            drawingCanvas.Tool = ToolType.Pointer;
            drawingCanvas.Cursor = HelperFunctions.DefaultCursor;
        }

        public override void SetCursor(SlideCanvas drawingCanvas)
        {

            drawingCanvas.Cursor = _toolCursor;
        }
    }
}
