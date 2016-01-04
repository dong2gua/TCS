using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.Controls
{
    /// <summary>
    /// This is the UI element for a connector.
    /// Each _nodes has multiple connectors that are used to connect it to other _nodes.
    /// </summary>
    public class Port : ContentControl
    {
        #region Events

        internal static readonly RoutedEvent ConnectorDragStartedEvent =
            EventManager.RegisterRoutedEvent("ConnectorDragStarted", RoutingStrategy.Bubble, typeof(ConnectorItemDragStartedEventHandler), typeof(Port));

        internal static readonly RoutedEvent ConnectorDraggingEvent =
            EventManager.RegisterRoutedEvent("ConnectorDragging", RoutingStrategy.Bubble, typeof(ConnectorItemDraggingEventHandler), typeof(Port));

        internal static readonly RoutedEvent ConnectorDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ConnectorDragCompleted", RoutingStrategy.Bubble, typeof(ConnectorItemDragCompletedEventHandler), typeof(Port));

        #endregion

        #region Dependency Property

        public static readonly DependencyProperty HotspotProperty =
            DependencyProperty.Register("Hotspot", typeof(Point), typeof(Port));

        public static readonly DependencyProperty PortTypeProperty =
            DependencyProperty.Register("PortType", typeof(PortType), typeof(Port));

        public static readonly DependencyProperty ParentPannelViewProperty =
            DependencyProperty.Register("ParentPannelView", typeof(PannelView), typeof(Port),
                new FrameworkPropertyMetadata(ParentPannelView_PropertyChanged));

        public static readonly DependencyProperty ParentModuleProperty =
            DependencyProperty.Register("ParentModule", typeof(Module), typeof(Port));

        #endregion

        #region Poperties and Fields

        /// <summary>
        /// The point the mouse was last at when dragging.
        /// </summary>
        private Point _lastMousePoint;

        /// <summary>
        /// Set to 'true' when left mouse button is held down.
        /// </summary>
        private bool _isLeftMouseDown;

        /// <summary>
        /// Set to 'true' when the user is dragging the connector.
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before dragging begins.
        /// </summary>
        private const double DragThreshold = 2;

        /// <summary>
        /// Automatically updated dependency property that specifies the hotspot (or center point) of the connector.
        /// Specified in content coordinate.
        /// </summary>
        public Point Hotspot
        {
            get { return (Point)GetValue(HotspotProperty); }
            set { SetValue(HotspotProperty, value); }
        }

        public PortType PortType
        {
            get { return (PortType)GetValue(PortTypeProperty); }
            set { SetValue(PortTypeProperty, value); }
        }

        /// <summary>
        /// Reference to the data-bound parent NetworkView.
        /// </summary>
        public PannelView ParentPannelView
        {
            get { return (PannelView)GetValue(ParentPannelViewProperty); }
            set { SetValue(ParentPannelViewProperty, value); }
        }

        /// <summary>
        /// Reference to the data-bound parent ModuleVmBase.
        /// </summary>
        public Module ParentModule
        {
            get { return (Module)GetValue(ParentModuleProperty); }
            set { SetValue(ParentModuleProperty, value); }
        }

        #endregion Private Data Members

        #region Constructors

        public Port()
        {
            // By default, we don't want a connector to be focusable.
            Focusable = false;

            // Hook layout update to recompute '_hotSpot' when the layout changes.
            LayoutUpdated += ConnectorItem_LayoutUpdated;
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Port()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Port), new FrameworkPropertyMetadata(typeof(Port)));
        }

        #endregion

        #region Methods

        /// <summary>
        /// A mouse button has been held down.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (ParentModule != null)
            {
                ParentModule.BringToFront();
            }

            if (ParentPannelView != null)
            {
                ParentPannelView.Focus();
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                //Drawing line only PortType is OutPort 
                if (PortType != PortType.OutPort)
                {
                    return;
                }
                if (ParentModule != null)
                {
                    // Delegate to parent _module to execute selection logic.
                    ParentModule.LeftMouseDownSelectionLogic();
                }

                _lastMousePoint = e.GetPosition(ParentPannelView);
                _isLeftMouseDown = true;
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (ParentModule != null)
                {
                    // Delegate to parent _module to execute selection logic.
                    ParentModule.RightMouseDownSelectionLogic();
                }
            }
        }

        /// <summary>
        /// The mouse cursor has been moved.
        /// </summary>        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                // Raise the event to notify that dragging is in progress.
                Point curMousePoint = e.GetPosition(ParentPannelView);
                Vector offset = curMousePoint - _lastMousePoint;
                if (offset.X != 0.0 && offset.Y != 0.0)
                {
                    _lastMousePoint = curMousePoint;
                    RaiseEvent(new ConnectorItemDraggingEventArgs(ConnectorDraggingEvent, this, offset.X, offset.Y));
                }
                e.Handled = true;
            }
            else if (_isLeftMouseDown)
            {
                if (ParentPannelView != null && ParentPannelView.EnableConnectionDragging)
                {
                    // The user is left-dragging the connector and _connection dragging is enabled,
                    // but don't initiate the drag operation until 
                    // the mouse cursor has moved more than the threshold distance.
                    Point curMousePoint = e.GetPosition(ParentPannelView);
                    var dragDelta = curMousePoint - _lastMousePoint;
                    double dragDistance = Math.Abs(dragDelta.Length);
                    if (dragDistance > DragThreshold)
                    {
                        // When the mouse has been dragged more than the threshold value commence dragging the _module.
                        // Raise an event to notify that that dragging has commenced.
                        var eventArgs = new ConnectorDragStartedEventArgs(ConnectorDragStartedEvent, this);
                        RaiseEvent(eventArgs);
                        if (eventArgs.Cancel)
                        {
                            // Handler of the event disallowed dragging of the _module.
                            _isLeftMouseDown = false;
                            return;
                        }
                        _isDragging = true;
                        CaptureMouse();
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// A mouse button has been released.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isLeftMouseDown)
                {
                    if (_isDragging)
                    {
                        RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, this));
                        ReleaseMouseCapture();
                        _isDragging = false;
                    }
                    else
                    {
                        // Execute mouse up selection logic only if there was no drag operation.
                        if (ParentModule != null)
                        {
                            // Delegate to parent _module to execute selection logic.
                            ParentModule.LeftMouseUpSelectionLogic();
                        }
                    }
                    _isLeftMouseDown = false;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Cancel _connection dragging for the connector that was dragged out.
        /// </summary>
        internal void CancelConnectionDragging()
        {
            if (_isLeftMouseDown)
            {
                // Raise ConnectorDragCompleted, with a null connector.
                RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, null));
                _isLeftMouseDown = false;
                ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Event raised when 'ParentPannelView' property has changed.
        /// </summary>
        private static void ParentPannelView_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (Port)d;
            c.UpdateHotspot();
        }

        /// <summary>
        /// Event raised when the layout of the connector has been updated.
        /// </summary>
        private void ConnectorItem_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateHotspot();
        }

        /// <summary>
        /// Update the connector hotspot.
        /// </summary>
        private void UpdateHotspot()
        {
            if (ParentPannelView == null)
            {
                // No parent NetworkView is set.
                return;
            }

            if (!ParentPannelView.IsAncestorOf(this))
            {
                // The parent NetworkView is no longer an ancestor of the connector.
                // This happens when the connector (and its parent _module) has been removed from the PannelVm.
                // Reset the property null so we don't attempt to check again.
                ParentPannelView = null;
                return;
            }
            // The parent NetworkView is still valid.Compute the center point of the connector.
            var centerPoint = new Point(ActualWidth / 2, ActualHeight / 2);

            // Transform the center point so that it is relative to the parent NetworkView.
            // Then assign it to _hotSpot.  Usually _hotSpot will be data-bound to the application
            // view-model using OneWayToSource so that the value of the hotspot is then pushed through
            // to the view-model.
            Hotspot = TransformToAncestor(ParentPannelView).Transform(centerPoint);
        }

        #endregion
    }
}
