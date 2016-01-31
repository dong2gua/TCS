using System.Diagnostics;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;
using ThorCyte.CarrierModule.Graphics;

namespace ThorCyte.CarrierModule.Tools
{
    class ToolPointer : Tool
    {
        private enum SelectionMode
        {
            None,
            Select,           // object(s) are moved
            GroupSelection
        }

        private SelectionMode _selectMode = SelectionMode.None;
        private GraphicsBase _resizedObject;
        private int _resizedObjectHandle;

        public override void OnMouseDown(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            var point = drawingCanvas.ClientToWorld(e.GetPosition(drawingCanvas));
            _selectMode = SelectionMode.None;

            GraphicsBase selectedObject = null;

            if (_selectMode == SelectionMode.None)
            {
                for (var i = drawingCanvas.GraphicsList.Count - 1; i >= 0; i--)
                {
                    var o = drawingCanvas[i];

                    if (o.MakeHitTest(point) == 0)
                    {
                        selectedObject = o;
                        break;
                    }
                }

                if (selectedObject != null)
                {
                    _selectMode = SelectionMode.Select;

                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        HelperFunctions.UnselectAll(drawingCanvas);
                    }

                    // Select clicked object
                    selectedObject.IsSelected = true;
                }
            }

            // Click on background
            if (_selectMode == SelectionMode.None)
            {
                // Unselect all if Ctrl is not pressed
                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    HelperFunctions.UnselectAll(drawingCanvas);
                }
            }
            // Capture mouse until MouseUp event is received
            drawingCanvas.CaptureMouse();
        }

        public override void OnMouseMove(SlideCanvas drawingCanvas, MouseEventArgs e)
        {

             drawingCanvas.Cursor = HelperFunctions.DefaultCursor;
                
        }

        /// <summary>
        /// Handle mouse up.
        /// Return to normal state.
        /// </summary>
        public override void OnMouseUp(SlideCanvas drawingCanvas, MouseButtonEventArgs e)
        {

            drawingCanvas.SetActiveRegions();
            drawingCanvas.ReleaseMouseCapture();

            drawingCanvas.Cursor = HelperFunctions.DefaultCursor;

            _selectMode = SelectionMode.None;
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
            _selectMode = SelectionMode.None;

            GraphicsBase selectedObject = null;

            if (_selectMode == SelectionMode.None)
            {
                for (var i = drawingCanvas.GraphicsList.Count - 1; i >= 0; i--)
                {
                    var o = drawingCanvas[i];

                    if (o.MakeHitTest(point) == 0)
                    {
                        selectedObject = o;
                        break;
                    }
                }

                if (selectedObject != null)
                {
                    _selectMode = SelectionMode.Select;

                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        PlateHelperFunctions.UnselectAll(drawingCanvas);
                    }

                    // Select clicked object
                    selectedObject.IsSelected = true;
                }
            }

            // Click on background
            if (_selectMode == SelectionMode.None)
            {
                // Unselect all if Ctrl is not pressed
                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    PlateHelperFunctions.UnselectAll(drawingCanvas);
                }
            }
            // Capture mouse until MouseUp event is received
            drawingCanvas.CaptureMouse();
        }

        public override void OnMouseMove(PlateCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;
        }

        public override void OnMouseUp(PlateCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            drawingCanvas.SetActiveRegions();
            drawingCanvas.ReleaseMouseCapture();

            drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;

            _selectMode = SelectionMode.None;
        }

        public override void SetCursor(PlateCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = PlateHelperFunctions.DefaultCursor;
        }
    }
}
