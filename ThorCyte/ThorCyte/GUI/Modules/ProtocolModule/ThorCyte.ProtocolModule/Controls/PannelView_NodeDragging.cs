using System;
using System.Collections.Generic;
using ThorCyte.ProtocolModule.Events;

namespace ThorCyte.ProtocolModule.Controls
{
    /// <summary>
    /// Partial definition of the PannelView class.
    /// This file only contains private members related to dragging _nodes.
    /// </summary>
    public partial class PannelView
    {
        #region Methods

        /// <summary>
        /// Event raised when the user starts to drag a _module.
        /// </summary>
        private void NodeItem_DragStarted(object source, ModuleDragStartedEventArgs e)
        {
            e.Handled = true;
            IsDragging = true;
            IsNotDragging = false;
            IsDraggingNode = true;
            IsNotDraggingNode = false;
            var eventArgs = new ModuleDragStartedEventArgs(ModuleDragStartedEvent, this, SelectedModules);
            RaiseEvent(eventArgs);
            e.Cancel = eventArgs.Cancel;
        }

        /// <summary>
        /// Event raised while the user is dragging a _module.
        /// </summary>
        private void NodeItem_Dragging(object source, ModuleDraggingEventArgs e)
        {
            e.Handled = true;

            // Cache the ModuleVmBase for each selected _module whilst dragging is in progress.
            if (_cachedSelectedNodeItems == null)
            {
                _cachedSelectedNodeItems = new List<Module>();

                foreach (var selectedNode in SelectedModules)
                {
                    Module module = FindAssociatedNodeItem(selectedNode);
                    if (module == null)
                    {
                        throw new ApplicationException("Unexpected code path!");
                    }
                    _cachedSelectedNodeItems.Add(module);
                }
            }

            // Update the position of the _module within the Canvas.
            foreach (var nodeItem in _cachedSelectedNodeItems)
            {
                var temp = nodeItem.X + e.HorizontalChange;

                if (temp >= 0 && (temp + nodeItem.ActualWidth) <= ActualWidth)
                {
                    nodeItem.X = temp;
                }
                else if (temp >= 0)
                {
                    Width = (temp + nodeItem.ActualWidth);
                    nodeItem.X = temp;
                }
                temp = nodeItem.Y + e.VerticalChange;

                if (temp >= 0 && (temp + nodeItem.ActualHeight) <= ActualHeight)
                {
                    nodeItem.Y = temp;
                }
                else if (temp >= 0)
                {
                    Height = (temp + nodeItem.ActualHeight);
                    nodeItem.Y = temp;
                }
            }

            var eventArgs = new ModuleDraggingEventArgs(ModuleDraggingEvent, this, SelectedModules, e.HorizontalChange, e.VerticalChange);
            RaiseEvent(eventArgs);
        }

        /// <summary>
        /// Event raised when the user has finished dragging a _module.
        /// </summary>
        private void NodeItem_DragCompleted(object source, ModuleDragCompletedEventArgs e)
        {
            e.Handled = true;
            var eventArgs = new ModuleDragCompletedEventArgs(ModuleDragCompletedEvent, this, SelectedModules);
            RaiseEvent(eventArgs);
            _cachedSelectedNodeItems = null;
            IsDragging = false;
            IsNotDragging = true;
            IsDraggingNode = false;
            IsNotDraggingNode = true;
        }

        #endregion
    }
}
