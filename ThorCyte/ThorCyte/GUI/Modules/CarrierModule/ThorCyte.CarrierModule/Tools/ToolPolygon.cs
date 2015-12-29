using System.IO;
using System.Windows;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;
using ThorCyte.CarrierModule.Graphics;

namespace ThorCyte.CarrierModule.Tools
{
    class ToolPolygon : ToolObject
    {
        private bool _isNewPolygon;
        private GraphicsPolygon _newPolygon;

        public ToolPolygon()
        {
            var stream = new MemoryStream(Properties.Resources.Polygon);
            ToolCursor = new Cursor(stream);
        }

        /// <summary>
        /// Create new object
        /// </summary>
        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var p = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));
            if (!_isNewPolygon)
            {
                _newPolygon = new GraphicsPolygon(
                    new[]
                {
                    p,
                    new Point(p.X + 1, p.Y + 1)
                },
                    drawingCanvas.LineWidth,
                    drawingCanvas.ObjectColor,
                    drawingCanvas.ActualScale,
                    drawingCanvas.CurrentRoomNo);

                AddNewObject(drawingCanvas, _newPolygon);
                _isNewPolygon = true;
            }
            else
            {
                if (_newPolygon == null)
                    return;

                if (e.ChangedButton == MouseButton.Right)
                {
                    _isNewPolygon = false;
                }
                else
                {
                    _newPolygon.AddPoint(p);
                }
            }

        }

        /// <summary>
        /// Set cursor and resize new polyline
        /// </summary>
        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (_newPolygon == null)
            {
                return;         // precaution
            }

            var p = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));
            _newPolygon.MoveHandleTo(p, _newPolygon.HandleCount);
        }

        public override void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right) return;
            _newPolygon = null;
            _isNewPolygon = false;
            base.OnMouseUp(drawingCanvas, e);
        }
    }
}
