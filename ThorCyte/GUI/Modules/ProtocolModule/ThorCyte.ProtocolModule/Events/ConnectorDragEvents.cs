using System.Windows;

namespace ThorCyte.ProtocolModule.Events
{
    #region Events

    /// <summary>
    /// Defines the event handler for ConnectorDragStarted events.
    /// </summary>
    internal delegate void ConnectorItemDragStartedEventHandler(object sender, ConnectorDragStartedEventArgs e);

    /// <summary>
    /// Defines the event handler for ConnectorDragCompleted events.
    /// </summary>
    internal delegate void ConnectorItemDragCompletedEventHandler(object sender, ConnectorItemDragCompletedEventArgs e);

    /// <summary>
    /// Defines the event handler for ConnectorDragStarted events.
    /// </summary>
    internal delegate void ConnectorItemDraggingEventHandler(object sender, ConnectorItemDraggingEventArgs e);

    #endregion

    /// <summary>
    /// Arguments for event raised when the user starts to drag a connector out from a _module.
    /// </summary>
    internal class ConnectorDragStartedEventArgs : RoutedEventArgs
    {
        #region Properties and Fields

        /// <summary>
        /// Cancel dragging out of the connector.
        /// </summary>
        public bool Cancel
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        internal ConnectorDragStartedEventArgs(RoutedEvent routedEvent, object source) :
            base(routedEvent, source)
        {
        }

        #endregion
    }

    /// <summary>
    /// Arguments for event raised while user is dragging a _module in the PannelVm.
    /// </summary>
    internal class ConnectorItemDraggingEventArgs : RoutedEventArgs
    {
        #region Properties and Fields

        /// <summary>
        /// The amount the connector has been dragged horizontally.
        /// </summary>
        private readonly double _horizontalChange;

        public double HorizontalChange
        {
            get { return _horizontalChange; }
        }

        /// <summary>
        /// The amount the connector has been dragged vertically.
        /// </summary>
        private readonly double _verticalChange;

        public double VerticalChange
        {
            get { return _verticalChange; }
        }

        #endregion

        #region Constructors

        public ConnectorItemDraggingEventArgs(RoutedEvent routedEvent, object source, double horizontalChange, double verticalChange) :
            base(routedEvent, source)
        {
            _horizontalChange = horizontalChange;
            _verticalChange = verticalChange;
        }

        #endregion
    }

    /// <summary>
    /// Arguments for event raised when the user has completed dragging a connector.
    /// </summary>
    internal class ConnectorItemDragCompletedEventArgs : RoutedEventArgs
    {
        #region Constructors

        public ConnectorItemDragCompletedEventArgs(RoutedEvent routedEvent, object source) :
            base(routedEvent, source)
        {
        }

        #endregion
    }
}
