using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;

namespace ThorCyte.CarrierModule.Tools
{
    class ToolDrag : Tool
    {
        private readonly Cursor _toolCursor;

        private Point _lastPoint = new Point(0, 0);


        public ToolDrag()
        {
            var stream = new MemoryStream(Properties.Resources.Drag);
            _toolCursor = new Cursor(stream);
        }

        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            _lastPoint.X = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[0]);
            _lastPoint.Y = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[1]);
            drawingCanvas.CaptureMouse();
        }

        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = _toolCursor;

            if (e.LeftButton != MouseButtonState.Pressed) return;

            var thispoint = new Point()
            {
                X = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[0]),
                Y = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[1])
            };

            drawingCanvas.Drag(thispoint.X - _lastPoint.X, thispoint.Y - _lastPoint.Y);

        }

        public override void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            drawingCanvas.ReleaseMouseCapture();
        }

        public override void SetCursor(SlideCanvas drawingCanvas)
        {
        }

        public override void OnMouseDown(PlateCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            _lastPoint.X = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[0]);
            _lastPoint.Y = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[1]);
            drawingCanvas.CaptureMouse();
        }

        public override void OnMouseMove(PlateCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = _toolCursor;

            if (e.LeftButton != MouseButtonState.Pressed) return;

            var thispoint = new Point()
            {
                X = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[0]),
                Y = Convert.ToDouble(drawingCanvas.MousePosition.Split(',')[1])
            };

            drawingCanvas.Drag(thispoint.X - _lastPoint.X, thispoint.Y - _lastPoint.Y);

        }

        public override void OnMouseUp(PlateCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            drawingCanvas.ReleaseMouseCapture();
        }

        public override void SetCursor(PlateCanvas drawingCanvas)
        {
        }
    }
}
