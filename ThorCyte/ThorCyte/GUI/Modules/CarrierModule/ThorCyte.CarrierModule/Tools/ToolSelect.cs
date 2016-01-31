using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.CarrierModule.Canvases;
using ThorCyte.CarrierModule.Graphics;

namespace ThorCyte.CarrierModule.Tools
{
    class ToolSelect : Tool
    {
        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var point = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));
            HelperFunctions.UnselectAll(drawingCanvas);
            drawingCanvas.Cursor = HelperFunctions.DefaultCursor;

            var r = new GraphicsSelectionRectangle(
                    point.X, point.Y,
                    point.X, point.Y,
                    drawingCanvas.ActualScale)
            {
                Clip =
                    new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth, drawingCanvas.ActualHeight))
            };

            drawingCanvas.GraphicsList.Add(r);

            // Capture mouse until MouseUp event is received
            drawingCanvas.CaptureMouse();

        }

        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {
            // Exclude all cases except left button on/off.
            if (e.MiddleButton == MouseButtonState.Pressed ||
                 e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            var point = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));

            if (e.LeftButton == MouseButtonState.Released)
            {
                drawingCanvas.Cursor = HelperFunctions.DefaultCursor;
            }

            if (!drawingCanvas.IsMouseCaptured)
            {
                return;
            }

            // Resize selection rectangle
            drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(
                point, 5);
        }

        /// <summary>
        /// Handle mouse up.
        /// Return to normal state.
        /// </summary>
        public override void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            if (!drawingCanvas.IsMouseCaptured)
            {
                drawingCanvas.Cursor = HelperFunctions.DefaultCursor;
                return;
            }

            var r = (GraphicsSelectionRectangle)drawingCanvas[drawingCanvas.Count - 1];
            r.Normalize();
            var rect = r.Rectangle;

            drawingCanvas.GraphicsList.Remove(r);

            foreach (var g in from GraphicsBase g in drawingCanvas.GraphicsList where g.IntersectsWith(rect) select g)
            {
                g.IsSelected = true;
            }

            drawingCanvas.SetActiveRegions();
            drawingCanvas.ReleaseMouseCapture();
            drawingCanvas.Tool = ToolType.Select;
            drawingCanvas.Cursor = HelperFunctions.DefaultCursor;
        }

        /// <summary>
        /// Set cursor
        /// </summary>
        public override void SetCursor(SlideCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = HelperFunctions.DefaultCursor;
        }

        public override void OnMouseDown(PlateCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var point = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));
            PlateHelperFunctions.UnselectAll(drawingCanvas);
            drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;

            var r = new GraphicsSelectionRectangle(
                    point.X, point.Y,
                    point.X, point.Y,
                    drawingCanvas.ActualScale)
            {
                Clip =
                    new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth, drawingCanvas.ActualHeight)),
            };

            drawingCanvas.GraphicsList.Add(r);

            // Capture mouse until MouseUp event is received
            drawingCanvas.CaptureMouse();

        }

        public override void OnMouseMove(PlateCanvas drawingCanvas, MouseEventArgs e)
        {
            // Exclude all cases except left button on/off.
            if (e.MiddleButton == MouseButtonState.Pressed ||
                 e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            var point = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));

            if (e.LeftButton == MouseButtonState.Released)
            {
                drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;
            }

            if (!drawingCanvas.IsMouseCaptured)
            {
                return;
            }

            // Resize selection rectangle
            drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(
                point, 5);
        }

        public override void OnMouseUp(PlateCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            if (!drawingCanvas.IsMouseCaptured)
            {
                drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;
                return;
            }


            var r = (GraphicsSelectionRectangle)drawingCanvas[drawingCanvas.Count - 1];
            r.Normalize();
            var rect = r.Rectangle;

            drawingCanvas.GraphicsList.Remove(r);

            foreach (var g in from GraphicsBase g in drawingCanvas.GraphicsList where g.IntersectsWith(rect) select g)
            {
                g.IsSelected = true;
            }

            drawingCanvas.SetActiveRegions();
            drawingCanvas.ReleaseMouseCapture();
            drawingCanvas.Tool = ToolType.Select;
            drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;
        }

        public override void SetCursor(PlateCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;
        }
    }
}
