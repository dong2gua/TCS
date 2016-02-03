using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    class RegionToolPointer:RegionTool
    {
        private enum SelectionMode
        {
            None,
            Move,           // object(s) are moved
            Size,           // object is resized
            GroupSelection
        }

        #region Properties and Field

        private SelectionMode _selectMode = SelectionMode.None;
        private Point _lastPoint = new Point(0, 0);
        private GraphicsBase _resizedObject;
        private bool _isChanged;
        private int _resizedObjectHandle;

        #endregion

        #region Methods

        public override void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(graph);
            _selectMode = SelectionMode.None;
            GraphicsBase o;
            GraphicsBase movedObject = null;
            _isChanged = false;

            for (int i = graph.VisualList.Count - 1; i >= 0; i--)
            {
                if (!(graph.VisualList[i] is GraphicsBase))
                {
                    continue;
                }
                o = graph[i];
                if (o.IsSelected)
                {
                    int handleNumber = o.MakeHitTest(point);

                    if (handleNumber > 0)
                    {
                        _selectMode = SelectionMode.Size;
                        // keep resized object in class member
                        _resizedObject = o;
                        _resizedObjectHandle = handleNumber;
                        // Since we want to resize only one object, unselect all other objects
                        graph.UnSelectAll();
                        o.IsSelected = true;
                        break;
                    }
                }
            }

            if (_selectMode == SelectionMode.None)
            {
                for (int i = graph.VisualList.Count - 1; i >= 0; i--)
                {
                    if (!(graph.VisualList[i] is GraphicsBase))
                    {
                        continue;
                    }
                    o = graph[i];

                    if (o.MakeHitTest(point) == 0)
                    {
                        movedObject = o;
                        break;
                    }
                }

                if (movedObject != null)
                {
                    _selectMode = SelectionMode.Move;
                    graph.UnSelectAll();

                    // Select clicked object
                    movedObject.IsSelected = true;

                    // Set move cursor
                    var graphicsLine = movedObject as GraphicsLine;
                    if (graphicsLine != null)
                    {
                        var line = graphicsLine;
                        if (line.LineType == LineType.Horizon)
                        {
                            graph.Cursor = Cursors.SizeWE;
                        }
                        else if (line.LineType == LineType.Vertical)
                        {
                            graph.Cursor = Cursors.SizeNS;
                        }
                        else
                        {
                            graph.Cursor = Cursors.SizeNESW;
                        }
                    }
                    else
                    {
                        graph.Cursor = Cursors.SizeAll;
                    }
                }
            }

            // Click on background
            if (_selectMode == SelectionMode.None)
            {
                // Unselect all if Ctrl is not pressed
                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    graph.UnSelectAll();
                }
            }
            _lastPoint = point;

            // Capture mouse until MouseUp event is received
            graph.CaptureMouse();
        }

        public override void OnMouseMove(RegionCanvas graph, MouseEventArgs e)
        {
            // Exclude all cases except left button on/off.
            if (e.MiddleButton == MouseButtonState.Pressed ||
                 e.RightButton == MouseButtonState.Pressed)
            {
                graph.Cursor = Cursors.Arrow;
                return;
            }

            Point point = e.GetPosition(graph);

            // Set cursor when left button is not pressed
            if (e.LeftButton == MouseButtonState.Released)
            {
                Cursor cursor = null;

                for (int i = 0; i < graph.VisualList.Count; i++)
                {
                    if (graph[i] == null)
                    {
                        continue;
                    }
                    int n = graph[i].MakeHitTest(point);

                    if (n > 0)
                    {
                        cursor = graph[i].GetHandleCursor(n);
                        break;
                    }
                }

                if (cursor == null)
                    cursor = Cursors.Arrow;

                graph.Cursor = cursor;
                return;
            }


            //if (!graph.IsMouseCaptured)
            //{
            //    return;
            //}

            // Find difference between previous and current position
            double dx = point.X - _lastPoint.X;
            double dy = point.Y - _lastPoint.Y;

            _lastPoint = point;

            // Resize
            if (_selectMode == SelectionMode.Size)
            {
                if (_resizedObject != null)
                {
                    _isChanged = true;
                    var rc = new Rect(graph.EndYPoint, graph.EndXPoint);

                    if (point.Y < rc.Top)
                        point.Y = rc.Top;
                    if (point.Y > rc.Bottom)
                        point.Y = rc.Bottom;
                    if (point.X < rc.Left)
                        point.X = rc.Left;
                    if (point.X > rc.Right)
                        point.X = rc.Right;

                    _resizedObject.MoveHandleTo(point, _resizedObjectHandle);
                }
            }

            // Move
            if (_selectMode == SelectionMode.Move)
            {
                foreach (GraphicsBase o in graph.Selection)
                {
                    var rc = new Rect(graph.EndYPoint, graph.EndXPoint);
                    if (dx < 0)
                    {
                        if (dx + o.GraphicsLeft < rc.Left)
                        {
                            dx = rc.Left - o.GraphicsLeft;
                        }
                    }
                    else
                    {
                        if (dx + o.GraphicsRight > rc.Right)
                        {
                            dx = rc.Right - o.GraphicsRight;
                        }
                    }

                    if (dy < 0)
                    {
                        if (dy + o.GraphicsTop < rc.Top)
                        {
                            dy = rc.Top - o.GraphicsTop;
                        }
                    }
                    else
                    {
                        if (dy + o.GraphicsBottom > rc.Bottom)
                        {
                            dy = rc.Bottom - o.GraphicsBottom;
                        }
                    }

                    o.Move(dx, dy);
                    _isChanged = true;
                }
            }

            // Group selection
            if (_selectMode == SelectionMode.GroupSelection)
            {
                // Resize selection rectangle
                graph[graph.VisualList.Count - 1].MoveHandleTo(point, 5);
            }
          }

        /// <summary>
        /// Handle mouse up.
        /// Return to normal state.
        /// </summary>
        public override void OnMouseUp(RegionCanvas graph, MouseButtonEventArgs e)
        {
            //if (!graph.IsMouseCaptured)
            //{
            //    graph.Cursor = Cursors.Arrow;
            //    _selectMode = SelectionMode.None;
            //    graph.ReleaseMouseCapture();
            //    return;
            //}
            if (_isChanged)
            {
                if (_resizedObject != null)
                {
                    // after resizing
                    _resizedObject.Normalize();
                    UpdateRegion(new List<GraphicsBase> { _resizedObject }, graph);
                    _resizedObject = null;
                }

                if (_selectMode == SelectionMode.Move)
                {
                    MoveRegion(graph);
                    _isChanged = false;
                }
            }
            graph.ReleaseMouseCapture();
            graph.Cursor = Cursors.Arrow;
            _selectMode = SelectionMode.None;
        }

        private void MoveRegion(RegionCanvas graph)
        {
            var list = new List<GraphicsBase>();
            foreach (var g in graph.Selection)
            {
                if (g.GraphicType == RegionType.None || g.GraphicType == RegionType.Line)
                {
                    continue;
                }
                var scattergram = graph as Scattergram;
                if (scattergram != null)
                {
                    if (scattergram.IsSnap)
                    {
                   //     RegionSnapHelper.SnapEdges(scattergram, g);
                    }
                }
                list.Add(g);
            }
            UpdateRegion(list, graph);
        }
       
        private void UpdateRegion(List<GraphicsBase> graphicList, RegionCanvas graph)
        {
            var list = new List<MaskRegion>();
            foreach (var g in graphicList)
            {
                var region = ROIManager.Instance.GetRegion(g.Name);
                if (region == null)
                {
                    return;
                }
               RegionHelper.UpdateRegionLocation(region,g,graph,graph.Vm);
               list.Add(region);
            }

            if (list.Count > 0)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(graph.Id, list, RegionUpdateType.Update));
            }
        }

        /// <summary>
        /// Set cursor
        /// </summary>
        public override void SetCursor(RegionCanvas graph)
        {
            graph.Cursor = Cursors.Arrow;
        }

        #endregion
    }
}
