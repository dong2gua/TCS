using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Controls
{
    public partial class PannelView
    {
        #region Properties and Fields

        /// <summary>
        /// When dragging a _connection, this is set to the Port that was initially dragged out.
        /// </summary>
        private Port _draggedOutPort;

        /// <summary>
        /// The view-model object for the connector that has been dragged out.
        /// </summary>
        private object _draggedOutConnectorDataContext;

        /// <summary>
        /// The view-model object for the _module whose connector was dragged out.
        /// </summary>
        private object _draggedOutNodeDataContext;

        /// <summary>
        /// The view-model object for the _connection that is currently being dragged, or null if none being dragged.
        /// </summary>
        private object _draggingConnectionDataContext;

        #endregion

        #region Methods

        /// <summary>
        /// Event raised when the user starts to drag a connector.
        /// </summary>
        private void ConnectorItem_DragStarted(object source, ConnectorDragStartedEventArgs e)
        {
            Focus();
            e.Handled = true;
            IsDragging = true;
            IsNotDragging = false;
            IsDraggingConnection = true;
            IsNotDraggingConnection = false;
            _draggedOutPort = (Port)e.OriginalSource;
            var nodeItem = _draggedOutPort.ParentModule;
            _draggedOutNodeDataContext = nodeItem.DataContext ?? nodeItem;
            _draggedOutConnectorDataContext = _draggedOutPort.DataContext ?? _draggedOutPort;

            // Raise an event so that application code can create a _connection and
            // add it to the view-model.
            var eventArgs = new ConnectionDragStartedEventArgs(ConnectionDragStartedEvent, this, _draggedOutNodeDataContext, _draggedOutConnectorDataContext);
            RaiseEvent(eventArgs);
            // Retrieve the the view-model object for the _connection was created by application code.
            _draggingConnectionDataContext = eventArgs.Connection;

            if (_draggingConnectionDataContext == null)
            {
                // Application code didn't create any _connection.
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Event raised while the user is dragging a connector.
        /// </summary>
        private void ConnectorItem_Dragging(object source, ConnectorItemDraggingEventArgs e)
        {
            e.Handled = true;

            try
            {
                if (!Equals(e.OriginalSource, _draggedOutPort))
                {
                    throw new ArgumentException("original source port not equals to outport.");
                }

                var mousePoint = Mouse.GetPosition(this);
                // Raise an event so that application code can compute intermediate _connection points.
                var connectionDraggingEventArgs =
                    new ConnectionDraggingEventArgs(ConnectionDraggingEvent, this,
                            _draggedOutNodeDataContext, _draggingConnectionDataContext,
                            _draggedOutConnectorDataContext);

                RaiseEvent(connectionDraggingEventArgs);

                // Figure out if the _connection has been dragged over a connector.
                Port portDraggedOver;
                object connectorDataContextDraggedOver;
                DetermineConnectorItemDraggedOver(mousePoint, out portDraggedOver, out connectorDataContextDraggedOver);
                if (portDraggedOver != null)
                {
                    // Raise an event so that application code can specify if the connector
                    // that was dragged over is valid or not.
                    var queryFeedbackEventArgs =
                        new QueryConnectionFeedbackEventArgs(QueryConnectionFeedbackEvent, this, _draggedOutNodeDataContext, _draggingConnectionDataContext,
                                _draggedOutConnectorDataContext, connectorDataContextDraggedOver);

                    RaiseEvent(queryFeedbackEventArgs);
                }
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("PannelView_ConnectionDragging.ConnectorItem_Dragging Error :", ex);
            }
        }

        /// <summary>
        /// Event raised when the user has finished dragging a connector.
        /// </summary>
        private void ConnectorItem_DragCompleted(object source, ConnectorItemDragCompletedEventArgs e)
        {
            e.Handled = true;

            try
            {
                if (!Equals(e.OriginalSource, _draggedOutPort))
                {
                    throw new ArgumentException("original source port not equals to outport.");
                }
                var mousePoint = Mouse.GetPosition(this);

                // Figure out if the end of the _connection was dropped on a connector.
                Port portDraggedOver;
                object connectorDataContextDraggedOver;
                DetermineConnectorItemDraggedOver(mousePoint, out portDraggedOver, out connectorDataContextDraggedOver);

                // Raise an event to inform application code that _connection dragging is complete.
                // The application code can determine if the _connection between the two connectors
                // is valid and if so it is free to make the appropriate _connection in the view-model.
                RaiseEvent(new ConnectionDragCompletedEventArgs(ConnectionDragCompletedEvent, this, _draggedOutNodeDataContext, _draggingConnectionDataContext, _draggedOutConnectorDataContext, connectorDataContextDraggedOver));

                IsDragging = false;
                IsNotDragging = true;
                IsDraggingConnection = false;
                IsNotDraggingConnection = true;
                _draggedOutConnectorDataContext = null;
                _draggedOutNodeDataContext = null;
                _draggedOutPort = null;
                _draggingConnectionDataContext = null;
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("PannelView_ConnectionDragging.ConnectorItem_Dragging Error :", ex);
            }
        }

        /// <summary>
        /// This function does a hit test to determine which connector, if any, is under 'hitPoint'.
        /// </summary>
        private bool DetermineConnectorItemDraggedOver(Point hitPoint, out Port portDraggedOver, out object connectorDataContextDraggedOver)
        {
            portDraggedOver = null;
            connectorDataContextDraggedOver = null;

            // Run a hit test 
            HitTestResult result = null;
            VisualTreeHelper.HitTest(_moduleItemsControl, null,
                // Result callback delegate.
                // This method is called when we have a result.
                delegate(HitTestResult hitTestResult)
                {
                    result = hitTestResult;

                    return HitTestResultBehavior.Stop;
                },
                new PointHitTestParameters(hitPoint));

            if (result == null || result.VisualHit == null)
            {
                // Hit test failed.
                return false;
            }

            // Actually want a reference to a 'Port'. The hit test may have hit a UI element that is below 'Port' so
            // search up the tree.
            var hitItem = result.VisualHit as FrameworkElement;
            if (hitItem == null)
            {
                return false;
            }
            var connectorItem = VisualUtils.FindVisualParentWithType<Port>(hitItem);
            if (connectorItem == null)
            {
                return false;
            }

            var networkView = connectorItem.ParentPannelView;
            if (networkView != this)
            {
                // Ensure that dragging over a connector in another NetworkView doesn't
                // return a positive result.
                return false;
            }

            object connectorDataContext = connectorItem;
            if (connectorItem.DataContext != null)
            {
                // If there is a data-context then grab it.When we are using a view-model then it is the view-model
                // object we are interested in.
                connectorDataContext = connectorItem.DataContext;
            }

            portDraggedOver = connectorItem;
            connectorDataContextDraggedOver = connectorDataContext;

            return true;
        }
        #endregion
    }
}
