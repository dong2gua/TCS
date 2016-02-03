using System.IO;
using System.Windows;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;
using ThorCyte.CarrierModule.Graphics;

namespace ThorCyte.CarrierModule.Tools
{
    /// <summary>
    /// Polyline tool
    /// </summary>
    class ToolPolyLine : ToolObject
    {
        private double _lastX;
        private double _lastY;
        private GraphicsPolyLine _newPolyLine;
        private const double MinDistance = 15;


        public ToolPolyLine()
        {
            var stream = new MemoryStream(Properties.Resources.Pencil);
            ToolCursor = new Cursor(stream);
        }

        /// <summary>
        /// Create new object
        /// </summary>
        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var p = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));

            _newPolyLine = new GraphicsPolyLine(
                new[]
                {
                    p,
                    new Point(p.X + 1, p.Y + 1)
                },
                drawingCanvas.LineWidth,
                drawingCanvas.ObjectColor,
                drawingCanvas.ActualScale,
                drawingCanvas.CurrentRoomNo);

            AddNewObject(drawingCanvas, _newPolyLine);

            _lastX = p.X;
            _lastY = p.Y;
        }

        /// <summary>
        /// Set cursor and resize new polyline
        /// </summary>
        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (!drawingCanvas.IsMouseCaptured)
            {
                return;
            }

            if (_newPolyLine == null)
            {
                return;         // precaution
            }

            var p = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));

            var distance = (p.X - _lastX) * (p.X - _lastX) + (p.Y - _lastY) * (p.Y - _lastY);

            var d = drawingCanvas.ActualScale <= 0 ?
                MinDistance * MinDistance :
                MinDistance * MinDistance / drawingCanvas.ActualScale;

            if (distance < d)
            {
                // Distance between last two points is less than minimum -
                // move last point
                _newPolyLine.MoveHandleTo(p, _newPolyLine.HandleCount);
            }
            else
            {
                // Add new segment
                _newPolyLine.AddPoint(p);

                _lastX = p.X;
                _lastY = p.Y;
            }
        }

        public override void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            _newPolyLine = null;

            base.OnMouseUp(drawingCanvas, e);
        }
    }
}
