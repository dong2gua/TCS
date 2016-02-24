﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.Controls
{
    /// <summary>
    /// Partial definition of the NetworkView class.
    /// This file only contains private members related to drag selection.
    /// </summary>
    public partial class PannelView
    {
        #region Properties and Fields

        /// <summary>
        /// Set to 'true' when the left mouse button is currently held down.
        /// </summary>
        private bool _isLeftMouseButtonDown;

        /// <summary>
        /// Set to 'true' when the user is dragging out the selection rectangle.
        /// </summary>
        private bool _isDraggingSelectionRect;

        /// <summary>
        /// Records the original mouse down point when the user is dragging out a selection rectangle.
        /// </summary>
        private Point _origMouseDownPoint;

        /// <summary>
        /// A reference to the canvas that contains the drag selection rectangle.
        /// </summary>
        private FrameworkElement _dragSelectionCanvas;

        /// <summary>
        /// The border that represents the drag selection rectangle.
        /// </summary>
        private FrameworkElement _dragSelectionBorder;

        /// <summary>
        /// Cached list of selected NodeItems, used while dragging _nodes.
        /// </summary>
        private List<Module> _cachedSelectedNodeItems;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private const double DragThreshold = 5;

        #endregion

        #region Methods

        /// <summary>
        /// Called when the user holds down the mouse.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (e.ChangedButton == MouseButton.Left)
            {
                //  Clear selection immediately when starting drag selection.
                _isLeftMouseButtonDown = true;
                _origMouseDownPoint = e.GetPosition(this);
                CaptureMouse();
                //e.Handled = true;
            }

        }

        /// <summary>
        /// Called when the user releases the mouse.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                bool wasDragSelectionApplied = false;

                if (_isDraggingSelectionRect)
                {
                    // Drag selection has ended, apply the 'selection rectangle'.
                    _isDraggingSelectionRect = false;
                    _dragSelectionCanvas.Visibility = Visibility.Collapsed;

                    //ApplyDragSelectionRect();
                    e.Handled = true;
                    wasDragSelectionApplied = true;
                }

                if (_isLeftMouseButtonDown)
                {
                    _isLeftMouseButtonDown = false;
                    ReleaseMouseCapture();
                    e.Handled = true;
                }

                if (!wasDragSelectionApplied && IsClearSelectionOnEmptySpaceClickEnabled)
                {
                    // A click and release in empty space clears the selection.
                    SelectedModules.Clear();

                    foreach (var con in _pannelControl.Items.OfType<ConnectorModel>())
                    {
                        con.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the user moves the mouse.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDraggingSelectionRect)
            {
                // Drag selection is in progress.
                var curMouseDownPoint = e.GetPosition(this);
                UpdateDragSelectionRect(_origMouseDownPoint, curMouseDownPoint);
                ApplyDragSelectionRect();
                e.Handled = true;
            }
            else if (_isLeftMouseButtonDown)
            {
                // The user is left-dragging the mouse,but don't initiate drag selection until
                // they have dragged past the threshold value.
                var curMouseDownPoint = e.GetPosition(this);
                var dragDelta = curMouseDownPoint - _origMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    // When the mouse has been dragged more than the threshold value commence drag selection.
                    _isDraggingSelectionRect = true;
                    // Clear the current selection.
                    _moduleItemsControl.SelectedItems.Clear();

                    InitDragSelectionRect(_origMouseDownPoint, curMouseDownPoint);
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            _dragSelectionCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x, y, width, height;
            // Determine x,y,width and height of the rect inverting the points if necessary.
            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            // Update the coordinates of the rectangle used for drag selection.
            Canvas.SetLeft(_dragSelectionBorder, x);
            Canvas.SetTop(_dragSelectionBorder, y);
            _dragSelectionBorder.Width = width;
            _dragSelectionBorder.Height = height;
        }

        /// <summary>
        /// Select all _nodes that are in the drag selection rectangle.
        /// </summary>
        private void ApplyDragSelectionRect()
        {
            var x = Canvas.GetLeft(_dragSelectionBorder);
            var y = Canvas.GetTop(_dragSelectionBorder);
            var width = _dragSelectionBorder.Width;
            var height = _dragSelectionBorder.Height;
            var dragRect = new Rect(x, y, width, height);

            // Inflate the drag selection-rectangle by 1/10 of its size to  make sure the intended item is selected.
            //dragRect.Inflate(width / 10, height / 10);


            // Find and select all the list box items.
            for (var nodeIndex = 0; nodeIndex < Modules.Count; ++nodeIndex)
            {
                var nodeItem = (Module)_moduleItemsControl.ItemContainerGenerator.ContainerFromIndex(nodeIndex);
                var transformToAncestor = nodeItem.TransformToAncestor(this);
                var itemPt1 = transformToAncestor.Transform(new Point(0, 0));
                var itemPt2 = transformToAncestor.Transform(new Point(nodeItem.ActualWidth, nodeItem.ActualHeight));
                var itemRect = new Rect(itemPt1, itemPt2);
                if (dragRect.IntersectsWith(itemRect))
                {
                    if (!nodeItem.IsSelected)
                        nodeItem.IsSelected = true;
                }
                else
                {
                    if (nodeItem.IsSelected)
                        nodeItem.IsSelected = false;
                }
            }

            foreach (ConnectorModel connItem in Connections)
            {

                var itemline = new LineGeometry
                {
                    StartPoint = connItem.SourcePortHotspot,
                    EndPoint = connItem.DestPortHotspot
                };

                if (IsIntersects(itemline, dragRect))
                {
                    if (!connItem.IsSelected)
                        connItem.IsSelected = true;
                }
                else
                {
                    if (connItem.IsSelected)
                        connItem.IsSelected = false;
                }
            }

        }

        private bool IsIntersects(LineGeometry l, Rect rect)
        {
            var ret = false;
            try
            {
                if (l == null || rect == null)
                {
                    ret = false;
                }
                else
                {
                    var r = new RectangleGeometry(rect);
                    var det = r.FillContainsWithDetail(l, 1.0, ToleranceType.Relative);

                    switch (det)
                    {
                        case IntersectionDetail.FullyContains:
                        case IntersectionDetail.Intersects:
                        case IntersectionDetail.FullyInside:
                            ret = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error occurred in IsIntersects " + ex.Message);
            }

            return ret;

        }



        #endregion
    }
}
