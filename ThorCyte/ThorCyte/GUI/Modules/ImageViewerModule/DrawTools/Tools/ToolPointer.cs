using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using System;
namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolPointer : Tool
    {
        private enum SelectionMode
        {
            None,
            Move,           
            Size,           
            GroupSelection
        }
        private SelectionMode _selectMode = SelectionMode.None;
        private GraphicsBase _resizedObject;
        private int _resizedObjectHandle;

        private Point _lastPoint = new Point(0, 0);
        

        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            var point = position;
            _selectMode = SelectionMode.None;

            GraphicsBase graphics;
            GraphicsBase movedObject = null;
            for (var i = drawingCanvas.GraphicsList.Count - 1; i >= 0; i--)
            {
                graphics = drawingCanvas[i];

                if (!graphics.IsSelected) continue;
                var handleNumber = graphics.MakeHitTest(point);

                if (handleNumber <= 0) continue;
                _selectMode = SelectionMode.Size;

                _resizedObject = graphics;
                _resizedObjectHandle = handleNumber;

                drawingCanvas.UnselectAll();
                graphics.IsSelected = true;
                break;
            }

            if (_selectMode == SelectionMode.None)
            {
                for (var i = drawingCanvas.GraphicsList.Count - 1; i >= 0; i--)
                {
                   graphics = drawingCanvas[i];

                    if (graphics.MakeHitTest(point) != 0) continue;
                    movedObject = graphics;
                    _selectMode = SelectionMode.Move;

                    if (Keyboard.Modifiers != ModifierKeys.Control && !movedObject.IsSelected)
                    {
                        drawingCanvas.UnselectAll();
                    }

                    movedObject.IsSelected = true;

                    drawingCanvas.Cursor = Cursors.SizeAll;
                    break;
                }
            }

            if (_selectMode == SelectionMode.None)
            {
                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    drawingCanvas.UnselectAll();
                }

                var r = new GraphicsSelectionRectangle(point, drawingCanvas)
                {
                    Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth / drawingCanvas.ActualScale.Item1, drawingCanvas.ActualHeight / drawingCanvas.ActualScale.Item2))
                };

                drawingCanvas.GraphicsList.Add(r);

                _selectMode = SelectionMode.GroupSelection;
            }


            _lastPoint = point;

            drawingCanvas.CaptureMouse();


        }

        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            
            var point = position;

            if (e.LeftButton == MouseButtonState.Released&&e.MiddleButton== MouseButtonState.Released&& e.RightButton == MouseButtonState.Released)
            {
                Cursor cursor = null;

                for (var i = 0; i < drawingCanvas.GraphicsList.Count; i++)
                {
                    var n = drawingCanvas[i].MakeHitTest(point);

                    if (n <= 0) continue;
                    cursor = drawingCanvas[i].GetHandleCursor(n);
                    break;
                }

                if (cursor == null) cursor = Cursors.Arrow;

                drawingCanvas.Cursor = cursor;

                return;

            }

            if (!drawingCanvas.IsMouseCaptured)
            {
                return;
            }


            var dx = point.X - _lastPoint.X;
            var dy = point.Y - _lastPoint.Y;

            _lastPoint = point;

            if (_selectMode == SelectionMode.Size)
            {
                if (_resizedObject != null)
                {
                    _resizedObject.MoveHandleTo(point, _resizedObjectHandle);
                }
            }

            if (_selectMode == SelectionMode.Move)
            {
                drawingCanvas.Move(dx, dy);
            }

            if (_selectMode == SelectionMode.GroupSelection)
            {
                drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(position, 5);
            }
        }

        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (!drawingCanvas.IsMouseCaptured)
            {
                drawingCanvas.Cursor =Cursors.Arrow;
                _selectMode = SelectionMode.None;
                return;
            }
            if (_selectMode == SelectionMode.Size)
            {
                if (_resizedObject != null)
                {
                    _resizedObject.Normalize();
                }
            }

            if (_selectMode == SelectionMode.GroupSelection)
            {
                var r = (GraphicsSelectionRectangle)drawingCanvas[drawingCanvas.Count - 1];
                r.Normalize();
                var rect = r.Rectangle;

                drawingCanvas.GraphicsList.Remove(r);

                foreach (var g in from GraphicsBase g in drawingCanvas.GraphicsList where g.IntersectsWith(rect) select g)
                {
                    g.IsSelected = true;
                }
            }

            drawingCanvas.ReleaseMouseCapture();

            drawingCanvas.Cursor = Cursors.Arrow;

            _selectMode = SelectionMode.None;


        }

        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = Cursors.Arrow;
        }
    }

}
