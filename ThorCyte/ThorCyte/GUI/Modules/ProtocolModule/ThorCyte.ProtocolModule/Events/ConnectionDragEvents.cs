using System.Windows;

namespace ThorCyte.ProtocolModule.Events
{
    #region Events
    /// <summary>
    /// Defines the event handler for the ConnectionDragging event.
    /// </summary>
    public delegate void ConnectionDraggingEventHandler(object sender, ConnectionDraggingEventArgs e);

    /// <summary>
    /// Defines the event handler for the ConnectionDragCompleted event.
    /// </summary>
    public delegate void ConnectionDragCompletedEventHandler(object sender, ConnectionDragCompletedEventArgs e);

    /// <summary>
    /// Defines the event handler for the QueryConnectionFeedback event.
    /// </summary>
    public delegate void QueryConnectionFeedbackEventHandler(object sender, QueryConnectionFeedbackEventArgs e);

    /// <summary>
    /// Defines the event handler for the ConnectionDragStarted event.
    /// </summary>
    public delegate void ConnectionDragStartedEventHandler(object sender, ConnectionDragStartedEventArgs e);

    #endregion

    /// <summary>
    /// Base class for _connection dragging event args.
    /// </summary>
    public class ConnectionDragEventArgs : RoutedEventArgs
    {
        #region Prpperties and Fields

        protected object _connection;

        /// <summary>
        /// The ModuleVmBase or it's DataContext (when non-NULL).
        /// </summary>
        private readonly object _module;

        public object Module
        {
            get { return _module; }
        }

        /// <summary>
        /// The Port or it's DataContext (when non-NULL).
        /// </summary>
        private readonly object _draggedOutConnector;

        public object ConnectorDraggedOut
        {
            get { return _draggedOutConnector; }
        }

        #endregion

        #region Methods

        protected ConnectionDragEventArgs(RoutedEvent routedEvent, object source, object module, object connection, object connector) :
            base(routedEvent, source)
        {
            _module = module;
            _draggedOutConnector = connector;
            _connection = connection;
        }

        #endregion
    }

    /// <summary>
    /// Arguments for event raised when the user starts to drag a _connection out from a _module.
    /// </summary>
    public class ConnectionDragStartedEventArgs : ConnectionDragEventArgs
    {
        #region Properties and Fields

        /// <summary>
        /// The _connection that will be dragged out.
        /// </summary>
        public object Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        #endregion

        #region Methods

        internal ConnectionDragStartedEventArgs(RoutedEvent routedEvent, object source, object module, object connector) :
            base(routedEvent, source, module, null, connector)
        {
        }

        #endregion
    }

    /// <summary>
    /// Arguments for event raised while user is dragging a _module in the PannelVm.
    /// </summary>
    public class QueryConnectionFeedbackEventArgs : ConnectionDragEventArgs
    {
        #region Properties and Fields
        /// <summary>
        /// The Port or it's DataContext (when non-NULL).
        /// </summary>
        private readonly object _draggedOverConnector;

        public object DraggedOverConnector
        {
            get { return _draggedOverConnector; }
        }

        /// <summary>
        /// The _connection that will be dragged out.
        /// </summary>
        public object Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Set to 'true' / 'false' to indicate that the _connection from the dragged out _connection to the dragged over connector is valid.
        /// </summary>
        private bool _connectionOk = true;

        public bool ConnectionOk
        {
            get { return _connectionOk; }
            set { _connectionOk = value; }
        }


        public object FeedbackIndicator { get; set; }

        #endregion

        #region Methods

        internal QueryConnectionFeedbackEventArgs(RoutedEvent routedEvent, object source,
            object module, object connection, object connector, object draggedOverConnector) :
            base(routedEvent, source, module, connection, connector)
        {
            _draggedOverConnector = draggedOverConnector;
        }

        #endregion
    }

    /// <summary>
    /// Arguments for event raised while user is dragging a _module in the PannelVm.
    /// </summary>
    public class ConnectionDraggingEventArgs : ConnectionDragEventArgs
    {
        #region Properties and Fields

        /// <summary>
        /// The _connection being dragged out.
        /// </summary>
        public object Connection
        {
            get { return _connection; }
        }

        #endregion

        #region Methods

        internal ConnectionDraggingEventArgs(RoutedEvent routedEvent, object source,
                object module, object connection, object connector) :
            base(routedEvent, source, module, connection, connector)
        {
        }

        #endregion
    }

    /// <summary>
    /// Arguments for event raised when the user has completed dragging a connector.
    /// </summary>
    public class ConnectionDragCompletedEventArgs : ConnectionDragEventArgs
    {
        #region Properties and Fields
        /// <summary>
        /// The Port or it's DataContext (when non-NULL).
        /// </summary>
        private readonly object _connectorDraggedOver;

        public object ConnectorDraggedOver
        {
            get { return _connectorDraggedOver; }
        }

        /// <summary>
        /// The _connection that will be dragged out.
        /// </summary>
        public object Connection
        {
            get { return _connection; }
        }

        #endregion

        #region Methods

        internal ConnectionDragCompletedEventArgs(RoutedEvent routedEvent, object source, object module, object connection, object connector, object connectorDraggedOver) :
            base(routedEvent, source, module, connection, connector)
        {
            _connectorDraggedOver = connectorDraggedOver;
        }

        #endregion
    }
}
