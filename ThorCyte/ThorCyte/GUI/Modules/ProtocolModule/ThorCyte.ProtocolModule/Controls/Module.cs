using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThorCyte.ProtocolModule.Events;

namespace ThorCyte.ProtocolModule.Controls
{

    
    /// <summary>
    /// This is a UI element that represents a PannelVm/flow-chart _module.
    /// </summary>
    public class Module : ListBoxItem
    {
        #region Dependency Property

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(Module),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(Module),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(Module),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ParentPannelViewProperty =
            DependencyProperty.Register("ParentPannelView", typeof(PannelView), typeof(Module),
                new FrameworkPropertyMetadata(ParentNetworkView_PropertyChanged));

        internal static readonly RoutedEvent ModuleDragStartedEvent =
            EventManager.RegisterRoutedEvent("ModuleDragStarted", RoutingStrategy.Bubble, typeof(NodeDragStartedEventHandler), typeof(Module));

        internal static readonly RoutedEvent ModuleDraggingEvent =
            EventManager.RegisterRoutedEvent("ModuleDragging", RoutingStrategy.Bubble, typeof(NodeDraggingEventHandler), typeof(Module));

        internal static readonly RoutedEvent ModuleDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ModuleDragCompleted", RoutingStrategy.Bubble, typeof(NodeDragCompletedEventHandler), typeof(Module));

        #endregion

        #region Constructors


        public Module()
        {
            // By default, we don't want this UI element to be focusable.
            Focusable = false;
            Loaded += Module_Loaded;
        }

        private void Module_Loaded(object sender, RoutedEventArgs e)
        {
            if(Resize != null) Resize(X + ActualWidth, Y + ActualHeight);
        }

        public delegate void ResizeHandler(double width, double height);
        public static ResizeHandler Resize;


        /// <summary>
        /// Static constructor.
        /// </summary>
        static Module()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Module), new FrameworkPropertyMetadata(typeof(Module)));

        }

        #endregion

        #region Properties and Fields

        /// <summary>
        /// The X coordinate of the _module.
        /// </summary>
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        /// <summary>
        /// The Y coordinate of the _module.
        /// </summary>
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        /// <summary>
        /// The Z index of the _module.
        /// </summary>
        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
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
        /// The point the mouse was last at when dragging.
        /// </summary>
        private Point _lastMousePoint;

        /// <summary>
        /// Set to 'true' when left mouse button is held down.
        /// </summary>
        private bool _isLeftMouseDown;

        /// <summary>
        /// Set to 'true' when left mouse button and the control key are held down.
        /// </summary>
        private bool _isLeftMouseAndControlDown;

        /// <summary>
        /// Set to 'true' when dragging has started.
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before dragging begins.
        /// </summary>
        private static readonly double DragThreshold = 5;

        #endregion

        #region Methods

        /// <summary>
        /// Bring the _module to the front of other elements.
        /// </summary>
        internal void BringToFront()
        {
            if (ParentPannelView == null)
            {
                return;
            }

            int maxZ = ParentPannelView.FindMaxZIndex();
            ZIndex = maxZ + 1;
        }

        /// <summary>
        /// Called when a mouse button is held down.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            BringToFront();

            if (ParentPannelView != null)
            {
                ParentPannelView.Focus();
            }

            if (e.ChangedButton == MouseButton.Left && ParentPannelView != null)
            {
                _lastMousePoint = e.GetPosition(ParentPannelView);
                _isLeftMouseDown = true;
                LeftMouseDownSelectionLogic();
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right && ParentPannelView != null)
            {
                RightMouseDownSelectionLogic();
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the left mouse button is pressed down.
        /// The reason this exists in its own method rather than being included in OnMouseDown is 
        /// so that Port can reuse this logic from its OnMouseDown.
        /// </summary>
        internal void LeftMouseDownSelectionLogic()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                // Control key was held down.
                // This means that the rectangle is being added to or removed from the existing selection.
                // Don't do anything yet, we will act on this later in the MouseUp event handler.
                _isLeftMouseAndControlDown = true;
            }
            else
            {
                // Control key is not held down.
                _isLeftMouseAndControlDown = false;

                if (ParentPannelView.SelectedModules.Count == 0)
                {
                    // Nothing already selected, select the item.
                    IsSelected = true;
                }
                else if (ParentPannelView.SelectedModules.Contains(this) ||
                         ParentPannelView.SelectedModules.Contains(DataContext))
                {
                    // Item is already selected, do nothing.
                    // We will act on this in the MouseUp if there was no drag operation.
                }
                else
                {
                    // Item is not selected.Deselect all, and select the item.
                    ParentPannelView.SelectedModules.Clear();
                    IsSelected = true;
                }
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the right mouse button is pressed down.
        /// The reason this exists in its own method rather than being included in OnMouseDown is 
        /// so that Port can reuse this logic from its OnMouseDown.
        /// </summary>
        internal void RightMouseDownSelectionLogic()
        {
            if (ParentPannelView.SelectedModules.Count == 0)
            {
                // Nothing already selected, select the item.
                IsSelected = true;
            }
            else if (ParentPannelView.SelectedModules.Contains(this) ||
                     ParentPannelView.SelectedModules.Contains(DataContext))
            {
                // Item is already selected, do nothing.
            }
            else
            {
                // Item is not selected. Deselect all, and select the item.
                ParentPannelView.SelectedModules.Clear();
                IsSelected = true;
            }
        }

        /// <summary>
        /// Called when the mouse cursor is moved.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                // Raise the event to notify that dragging is in progress.
                Point curMousePoint = e.GetPosition(ParentPannelView);
                object item = this;
                if (DataContext != null)
                {
                    item = DataContext;
                }

                Vector offset = curMousePoint - _lastMousePoint;
                if (offset.X != 0.0 || offset.Y != 0.0)
                {
                    _lastMousePoint = curMousePoint;
                    RaiseEvent(new ModuleDraggingEventArgs(ModuleDraggingEvent, this, new object[] { item }, offset.X, offset.Y));
                }
            }
            else if (_isLeftMouseDown && ParentPannelView.EnableNodeDragging)
            {
                // The user is left-dragging the _module, but don't initiate the drag operation until 
                // the mouse cursor has moved more than the threshold distance.
                Point curMousePoint = e.GetPosition(ParentPannelView);
                var dragDelta = curMousePoint - _lastMousePoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    // When the mouse has been dragged more than the threshold value commence dragging the _module.
                    // Raise an event to notify that that dragging has commenced.
                    var eventArgs = new ModuleDragStartedEventArgs(ModuleDragStartedEvent, this, new Module[] { this });
                    RaiseEvent(eventArgs);
                    if (eventArgs.Cancel)
                    {
                        // Handler of the event disallowed dragging of the _module.
                        _isLeftMouseDown = false;
                        _isLeftMouseAndControlDown = false;
                        return;
                    }
                    _isDragging = true;
                    CaptureMouse();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isLeftMouseDown)
            {
                if (_isDragging)
                {
                    // Raise an event to notify that _module dragging has finished.
                    RaiseEvent(new ModuleDragCompletedEventArgs(ModuleDragCompletedEvent, this, new Module[] { this }));
                    ReleaseMouseCapture();
                    _isDragging = false;
                }
                else
                {
                    // Execute mouse up selection logic only if there was no drag operation.
                    LeftMouseUpSelectionLogic();
                }

                _isLeftMouseDown = false;
                _isLeftMouseAndControlDown = false;
                e.Handled = true;
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the left mouse button is released.
        /// The reason this exists in its own method rather than being included in OnMouseUp is 
        /// so that Port can reuse this logic from its OnMouseUp.
        /// </summary>
        internal void LeftMouseUpSelectionLogic()
        {
            if (_isLeftMouseAndControlDown)
            {
                // Control key was held down.Toggle the selection.
                //IsSelected = !IsSelected;
            }
            else
            {
                // Control key was not held down.
                if (ParentPannelView.SelectedModules.Count == 1 &&
                    (ParentPannelView.SelectedModule == this ||
                     ParentPannelView.SelectedModule == DataContext))
                {
                    // The item that was clicked is already the only selected item.
                    // Don't need to do anything.
                }
                else
                {
                    // Clear the selection and select the clicked item as the only selected item.
                    ParentPannelView.SelectedModules.Clear();
                    IsSelected = true;
                }
            }
            _isLeftMouseAndControlDown = false;
        }

        /// <summary>
        /// Event raised when the ParentPannelView property has changed.
        /// </summary>
        private static void ParentNetworkView_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Bring new _nodes to the front of the z-order.
            var nodeItem = (Module)o;
            nodeItem.BringToFront();
        }



        #endregion
    }
}
