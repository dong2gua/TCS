using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Controls
{
    /// <summary>
    /// The main class that implements the PannelVm/flow-chart control.
    /// </summary>
    public partial class PannelView : Control
    {
        #region Events
        /// <summary>
        /// Event raised when the user starts dragging a _module in the PannelVm.
        /// </summary>
        public event NodeDragStartedEventHandler ModuleDragStarted
        {
            add { AddHandler(ModuleDragStartedEvent, value); }
            remove { RemoveHandler(ModuleDragStartedEvent, value); }
        }

        /// <summary>
        /// Event raised while user is dragging a _module in the PannelVm.
        /// </summary>
        public event NodeDraggingEventHandler ModuleDragging
        {
            add { AddHandler(ModuleDraggingEvent, value); }
            remove { RemoveHandler(ModuleDraggingEvent, value); }
        }

        /// <summary>
        /// Event raised when the user has completed dragging a _module in the PannelVm.
        /// </summary>
        public event NodeDragCompletedEventHandler ModuleDragCompleted
        {
            add { AddHandler(ModuleDragCompletedEvent, value); }
            remove { RemoveHandler(ModuleDragCompletedEvent, value); }
        }

        /// <summary>
        /// Event raised when the user starts dragging a connector in the PannelVm.
        /// </summary>
        public event ConnectionDragStartedEventHandler ConnectionDragStarted
        {
            add { AddHandler(ConnectionDragStartedEvent, value); }
            remove { RemoveHandler(ConnectionDragStartedEvent, value); }
        }

        /// <summary>
        /// Event raised while user drags a _connection over the connector of a _module in the PannelVm.
        /// The event handlers should supply a feedback objects and data-template that displays the 
        /// object as an appropriate graphic.
        /// </summary>
        public event QueryConnectionFeedbackEventHandler QueryConnectionFeedback
        {
            add { AddHandler(QueryConnectionFeedbackEvent, value); }
            remove { RemoveHandler(QueryConnectionFeedbackEvent, value); }
        }

        /// <summary>
        /// Event raised when a _connection is being dragged.
        /// </summary>
        public event ConnectionDraggingEventHandler ConnectionDragging
        {
            add { AddHandler(ConnectionDraggingEvent, value); }
            remove { RemoveHandler(ConnectionDraggingEvent, value); }
        }

        /// <summary>
        /// Event raised when the user has completed dragging a _connection in the PannelVm.
        /// </summary>
        public event ConnectionDragCompletedEventHandler ConnectionDragCompleted
        {
            add { AddHandler(ConnectionDragCompletedEvent, value); }
            remove { RemoveHandler(ConnectionDragCompletedEvent, value); }
        }

        /// <summary>
        /// An event raised when the modules selected in the NetworkView has changed.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        #endregion

        #region Dependency Property

        private static readonly DependencyPropertyKey NodesPropertyKey =
            DependencyProperty.RegisterReadOnly("Modules", typeof(ImpObservableCollection<object>), typeof(PannelView),
            new FrameworkPropertyMetadata());
        public static readonly DependencyProperty ModulesProperty = NodesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ConnectionsPropertyKey =
            DependencyProperty.RegisterReadOnly("Connections", typeof(ImpObservableCollection<object>), typeof(PannelView),
             new FrameworkPropertyMetadata());
        public static readonly DependencyProperty ConnectionsProperty = ConnectionsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ModuleSourceProperty =
            DependencyProperty.Register("ModuleSource", typeof(IEnumerable), typeof(PannelView),
                new FrameworkPropertyMetadata(NodesSource_PropertyChanged));

        public static readonly DependencyProperty ConnectionsSourceProperty =
            DependencyProperty.Register("ConnectionsSource", typeof(IEnumerable), typeof(PannelView),
                new FrameworkPropertyMetadata(ConnectionsSource_PropertyChanged));

        public static readonly DependencyProperty IsClearSelectionOnEmptySpaceClickEnabledProperty =
            DependencyProperty.Register("IsClearSelectionOnEmptySpaceClickEnabled", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty EnableConnectionDraggingProperty =
            DependencyProperty.Register("EnableConnectionDragging", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyPropertyKey IsDraggingConnectionPropertyKey =
            DependencyProperty.RegisterReadOnly("IsDraggingConnection", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingConnectionProperty = IsDraggingConnectionPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNotDraggingConnectionPropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDraggingConnection", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingConnectionProperty = IsNotDraggingConnectionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty EnableNodeDraggingProperty =
            DependencyProperty.Register("EnableNodeDragging", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyPropertyKey IsDraggingNodePropertyKey =
            DependencyProperty.RegisterReadOnly("IsDraggingNode", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingNodeProperty = IsDraggingNodePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNotDraggingNodePropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDraggingNode", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingNodeProperty = IsDraggingNodePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsDragging", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNotDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDragging", typeof(bool), typeof(PannelView),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingProperty = IsNotDraggingPropertyKey.DependencyProperty;

        public static readonly DependencyProperty NodeItemTemplateProperty =
            DependencyProperty.Register("NodeItemTemplate", typeof(DataTemplate), typeof(PannelView));

        public static readonly DependencyProperty NodeItemTemplateSelectorProperty =
            DependencyProperty.Register("NodeItemTemplateSelector", typeof(DataTemplateSelector), typeof(PannelView));

        public static readonly DependencyProperty NodeItemContainerStyleProperty =
            DependencyProperty.Register("NodeItemContainerStyle", typeof(Style), typeof(PannelView));

        public static readonly DependencyProperty ConnectionItemTemplateProperty =
            DependencyProperty.Register("ConnectionItemTemplate", typeof(DataTemplate), typeof(PannelView));

        public static readonly DependencyProperty ConnectionItemTemplateSelectorProperty =
            DependencyProperty.Register("ConnectionItemTemplateSelector", typeof(DataTemplateSelector), typeof(PannelView));

        public static readonly DependencyProperty ConnectionItemContainerStyleProperty =
            DependencyProperty.Register("ConnectionItemContainerStyle", typeof(Style), typeof(PannelView));

        public static readonly RoutedEvent ModuleDragStartedEvent =
            EventManager.RegisterRoutedEvent("ModuleDragStarted", RoutingStrategy.Bubble, typeof(NodeDragStartedEventHandler), typeof(PannelView));

        public static readonly RoutedEvent ModuleDraggingEvent =
            EventManager.RegisterRoutedEvent("ModuleDragging", RoutingStrategy.Bubble, typeof(NodeDraggingEventHandler), typeof(PannelView));

        public static readonly RoutedEvent ModuleDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ModuleDragCompleted", RoutingStrategy.Bubble, typeof(NodeDragCompletedEventHandler), typeof(PannelView));

        public static readonly RoutedEvent ConnectionDragStartedEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragStarted", RoutingStrategy.Bubble, typeof(ConnectionDragStartedEventHandler), typeof(PannelView));

        public static readonly RoutedEvent QueryConnectionFeedbackEvent =
            EventManager.RegisterRoutedEvent("QueryConnectionFeedback", RoutingStrategy.Bubble, typeof(QueryConnectionFeedbackEventHandler), typeof(PannelView));

        public static readonly RoutedEvent ConnectionDraggingEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragging", RoutingStrategy.Bubble, typeof(ConnectionDraggingEventHandler), typeof(PannelView));

        public static readonly RoutedEvent ConnectionDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragCompleted", RoutingStrategy.Bubble, typeof(ConnectionDragCompletedEventHandler), typeof(PannelView));

        #endregion

        #region Commands

        public static readonly RoutedCommand SelectAllCommand = null;
        public static readonly RoutedCommand SelectNoneCommand = null;
        public static readonly RoutedCommand InvertSelectionCommand = null;
        public static readonly RoutedCommand CancelConnectionDraggingCommand = null;

        #endregion

        #region Properties and Fields

        /// <summary>
        /// Cached reference to the ModuleItemsControl in the visual-tree.
        /// </summary>
        private ModuleItemsControl _moduleItemsControl;

        /// <summary>
        /// Cached reference to the ItemsControl for connections in the visual-tree.
        /// </summary>
        private ItemsControl _connectionItemsControl;

        /// <summary>
        /// Cached list of currently selected modules.
        /// </summary>
        private List<object> _initialSelectedNodes;

        /// <summary>
        /// Collection of modules in the PannelVm.
        /// </summary>
        public ImpObservableCollection<object> Modules
        {
            get { return (ImpObservableCollection<object>)GetValue(ModulesProperty); }
            private set { SetValue(NodesPropertyKey, value); }
        }

        /// <summary>
        /// Collection of connections in the PannelVm.
        /// </summary>
        public ImpObservableCollection<object> Connections
        {
            get { return (ImpObservableCollection<object>)GetValue(ConnectionsProperty); }
            private set { SetValue(ConnectionsPropertyKey, value); }
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Modules'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable ModuleSource
        {
            get { return (IEnumerable)GetValue(ModuleSourceProperty); }
            set { SetValue(ModuleSourceProperty, value); }
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Connections'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable ConnectionsSource
        {
            get { return (IEnumerable)GetValue(ConnectionsSourceProperty); }
            set { SetValue(ConnectionsSourceProperty, value); }
        }

        /// <summary>
        /// Set to 'true' to enable the clearing of selection when empty space is clicked.
        /// This is set to 'true' by default.
        /// </summary>
        public bool IsClearSelectionOnEmptySpaceClickEnabled
        {
            get { return (bool)GetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty); }
            set { SetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty, value); }
        }

        /// <summary>
        /// Set to 'true' to enable drag out of connectors to create new connections.
        /// </summary>
        public bool EnableConnectionDragging
        {
            get { return (bool)GetValue(EnableConnectionDraggingProperty); }
            set { SetValue(EnableConnectionDraggingProperty, value); }
        }

        /// <summary>
        /// Dependency property that is set to 'true' when the user is 
        /// dragging out a _connection.
        /// </summary>
        public bool IsDraggingConnection
        {
            get { return (bool)GetValue(IsDraggingConnectionProperty); }
            private set { SetValue(IsDraggingConnectionPropertyKey, value); }
        }

        /// <summary>
        /// Dependency property that is set to 'false' when the user is 
        /// dragging out a _connection.
        /// </summary>
        public bool IsNotDraggingConnection
        {
            get { return (bool)GetValue(IsNotDraggingConnectionProperty); }
            private set { SetValue(IsNotDraggingConnectionPropertyKey, value); }
        }

        /// <summary>
        /// Set to 'true' to enable dragging of modules.
        /// </summary>
        public bool EnableNodeDragging
        {
            get { return (bool)GetValue(EnableNodeDraggingProperty); }
            set { SetValue(EnableNodeDraggingProperty, value); }
        }

        /// <summary>
        /// Dependency property that is set to 'true' when the user is 
        /// dragging out a _connection.
        /// </summary>
        public bool IsDraggingNode
        {
            get { return (bool)GetValue(IsDraggingNodeProperty); }
            private set { SetValue(IsDraggingNodePropertyKey, value); }
        }

        /// <summary>
        /// Dependency property that is set to 'false' when the user is 
        /// dragging out a _connection.
        /// </summary>
        public bool IsNotDraggingNode
        {
            get { return (bool)GetValue(IsNotDraggingNodeProperty); }
            private set { SetValue(IsNotDraggingNodePropertyKey, value); }
        }

        /// <summary>
        /// Set to 'true' when the user is dragging either a _module or a _connection.
        /// </summary>
        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            private set { SetValue(IsDraggingPropertyKey, value); }
        }

        /// <summary>
        /// Set to 'true' when the user is not dragging anything.
        /// </summary>
        public bool IsNotDragging
        {
            get { return (bool)GetValue(IsNotDraggingProperty); }
            private set { SetValue(IsNotDraggingPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each _module item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate NodeItemTemplate
        {
            get { return (DataTemplate)GetValue(NodeItemTemplateProperty); }
            set { SetValue(NodeItemTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector NodeItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(NodeItemTemplateSelectorProperty); }
            set { SetValue(NodeItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each _module item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style NodeItemContainerStyle
        {
            get { return (Style)GetValue(NodeItemContainerStyleProperty); }
            set { SetValue(NodeItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each _connection item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate ConnectionItemTemplate
        {
            get { return (DataTemplate)GetValue(ConnectionItemTemplateProperty); }
            set { SetValue(ConnectionItemTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector ConnectionItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ConnectionItemTemplateSelectorProperty); }
            set { SetValue(ConnectionItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each _connection item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style ConnectionItemContainerStyle
        {
            get { return (Style)GetValue(ConnectionItemContainerStyleProperty); }
            set { SetValue(ConnectionItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// A reference to currently selected _module.
        /// </summary>
        public object SelectedModule
        {
            get
            {
                if (_moduleItemsControl != null)
                {
                    return _moduleItemsControl.SelectedItem;
                }
                if (_initialSelectedNodes == null || _initialSelectedNodes.Count != 1)
                {
                    return null;
                }
                return _initialSelectedNodes[0];
            }
            set
            {
                if (_moduleItemsControl != null)
                {
                    _moduleItemsControl.SelectedItem = value;
                }
                else
                {
                    if (_initialSelectedNodes == null)
                    {
                        _initialSelectedNodes = new List<object>();
                    }

                    _initialSelectedNodes.Clear();
                    _initialSelectedNodes.Add(value);
                }
            }
        }

        /// <summary>
        /// A list of selected modules.
        /// </summary>
        public IList SelectedModules
        {
            get
            {
                if (_moduleItemsControl != null)
                {
                    return _moduleItemsControl.SelectedItems;
                }
                return _initialSelectedNodes ?? (_initialSelectedNodes = new List<object>());
            }
        }

        #endregion

        #region Constructors

        public PannelView()
        {
            //Create a collection to contain modules.
            Modules = new ImpObservableCollection<object>();

            // Create a collection to contain connections.
            Connections = new ImpObservableCollection<object>();

            // Default background is white.
            Background = Brushes.White;

            // Add handlers for _module and connector drag events.
            AddHandler(Module.ModuleDragStartedEvent, new NodeDragStartedEventHandler(NodeItem_DragStarted));
            AddHandler(Module.ModuleDraggingEvent, new NodeDraggingEventHandler(NodeItem_Dragging));
            AddHandler(Module.ModuleDragCompletedEvent, new NodeDragCompletedEventHandler(NodeItem_DragCompleted));
            AddHandler(Port.ConnectorDragStartedEvent, new ConnectorItemDragStartedEventHandler(ConnectorItem_DragStarted));
            AddHandler(Port.ConnectorDraggingEvent, new ConnectorItemDraggingEventHandler(ConnectorItem_Dragging));
            AddHandler(Port.ConnectorDragCompletedEvent, new ConnectorItemDragCompletedEventHandler(ConnectorItem_DragCompleted));

            Module.Resize += Resize;
        }


        /// <summary>
        /// Static constructor.
        /// </summary>
        static PannelView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PannelView), new FrameworkPropertyMetadata(typeof(PannelView)));

            var inputs = new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Control) };
            SelectAllCommand = new RoutedCommand("SelectAll", typeof(PannelView), inputs);

            inputs = new InputGestureCollection { new KeyGesture(Key.Escape) };
            SelectNoneCommand = new RoutedCommand("SelectNone", typeof(PannelView), inputs);

            inputs = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            InvertSelectionCommand = new RoutedCommand("InvertSelection", typeof(PannelView), inputs);

            CancelConnectionDraggingCommand = new RoutedCommand("CancelConnectionDragging", typeof(PannelView));

            var binding = new CommandBinding { Command = SelectAllCommand };
            binding.Executed += SelectAll_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(PannelView), binding);

            binding = new CommandBinding { Command = SelectNoneCommand };
            binding.Executed += SelectNone_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(PannelView), binding);

            binding = new CommandBinding { Command = InvertSelectionCommand };
            binding.Executed += InvertSelection_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(PannelView), binding);

            binding = new CommandBinding { Command = CancelConnectionDraggingCommand };
            binding.Executed += CancelConnectionDragging_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(PannelView), binding);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bring the currently selected modules into view.
        /// This affects ContentViewportOffsetX/ContentViewportOffsetY, but doesn't affect 'ContentScale'.
        /// </summary>
        public void BringSelectedModulesIntoView()
        {
            BringModulesIntoView(SelectedModules);
        }

        /// <summary>
        /// Bring the collection of modules into view.
        /// This affects ContentViewportOffsetX/ContentViewportOffsetY, but doesn't affect 'ContentScale'.
        /// </summary>
        public void BringModulesIntoView(ICollection modules)
        {
            if (modules == null)
            {
                throw new ArgumentNullException("'modules' argument shouldn't be null.");
            }

            if (modules.Count == 0)
            {
                return;
            }

            var rect = Rect.Empty;

            foreach (var m in modules)
            {
                var module = FindAssociatedNodeItem(m);
                var modulerect = new Rect(module.X, module.Y, module.ActualWidth, module.ActualHeight);

                if (rect == Rect.Empty)
                {
                    rect = modulerect;
                }
                else
                {
                    rect.Intersect(modulerect);
                }
                

            }

            BringIntoView(rect);
        }

        /// <summary>
        /// Clear the selection.
        /// </summary>
        public void SelectNone()
        {
            SelectedModules.Clear();
        }

        /// <summary>
        /// Selects all of the modules.
        /// </summary>
        public void SelectAll()
        {
            if (SelectedModules.Count != Modules.Count)
            {
                SelectedModules.Clear();
                foreach (var node in Modules)
                {
                    SelectedModules.Add(node);
                }
            }
        }

        /// <summary>
        /// Inverts the current selection.
        /// </summary>
        public void InvertSelection()
        {
            var selectedNodesCopy = new ArrayList(SelectedModules);
            SelectedModules.Clear();

            foreach (var node in Modules)
            {
                if (!selectedNodesCopy.Contains(node))
                {
                    SelectedModules.Add(node);
                }
            }
        }

        /// <summary>
        /// When _connection dragging is progress this function cancels it.
        /// </summary>
        public void CancelConnectionDragging()
        {
            if (!IsDraggingConnection)
            {
                return;
            }

            _draggedOutPort.CancelConnectionDragging();
            IsDragging = false;
            IsNotDragging = true;
            IsDraggingConnection = false;
            IsNotDraggingConnection = true;
            _draggedOutPort = null;
            _draggedOutNodeDataContext = null;
            _draggedOutConnectorDataContext = null;
            _draggingConnectionDataContext = null;
        }

        /// <summary>
        /// Executes the 'SelectAll' command.
        /// </summary>
        private static void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (PannelView)sender;
            c.SelectAll();
        }

        /// <summary>
        /// Executes the 'SelectNone' command.
        /// </summary>
        private static void SelectNone_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (PannelView)sender;
            c.SelectNone();
        }

        /// <summary>
        /// Executes the 'InvertSelection' command.
        /// </summary>
        private static void InvertSelection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (PannelView)sender;
            c.InvertSelection();
        }

        /// <summary>
        /// Executes the 'CancelConnectionDragging' command.
        /// </summary>
        private static void CancelConnectionDragging_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (PannelView)sender;
            c.CancelConnectionDragging();
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'ModuleSource' property.
        /// </summary>
        private static void NodesSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (PannelView)d;
            c.Modules.Clear();

            if (e.OldValue != null)
            {
                var notifyCollectionChanged = e.OldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    // Unhook events from previous collection.
                    notifyCollectionChanged.CollectionChanged -= c.NodesSource_CollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                var enumerable = e.NewValue as IEnumerable;
                if (enumerable != null)
                {
                    // Populate 'Modules' from 'ModuleSource'.
                    foreach (object obj in enumerable)
                    {

                        c.Modules.Add(obj);
                    }
                }

                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    // Hook events in new collection.
                    notifyCollectionChanged.CollectionChanged += c.NodesSource_CollectionChanged;
                }
            }


        }

        /// <summary>
        /// Event raised when a _module has been added to or removed from the collection assigned to 'ModuleSource'.
        /// </summary>
        private void NodesSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // 'ModuleSource' has been cleared, also clear 'Modules'.
                Modules.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    // For each item that has been removed from 'ModuleSource' also remove it from 'Modules'.
                    foreach (object obj in e.OldItems)
                    {
                        Modules.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    // For each item that has been added to 'ModuleSource' also add it to 'Modules'.
                    foreach (object obj in e.NewItems)
                    {
                        Modules.Add(obj);
                    }
                }
            }
        }


        /// <summary>
        /// Event raised when a new collection has been assigned to the 'ConnectionsSource' property.
        /// </summary>
        private static void ConnectionsSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (PannelView)d;
            c.Connections.Clear();

            if (e.OldValue != null)
            {
                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    // Unhook events from previous collection.
                    notifyCollectionChanged.CollectionChanged -= c.ConnectionsSource_CollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                var enumerable = e.NewValue as IEnumerable;
                if (enumerable != null)
                {
                    // Populate 'Connections' from 'ConnectionsSource'.
                    foreach (object obj in enumerable)
                    {
                        c.Connections.Add(obj);
                    }
                }

                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    // Hook events in new collection.
                    notifyCollectionChanged.CollectionChanged += c.ConnectionsSource_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Event raised when a _connection has been added to or removed from the collection assigned to 'ConnectionsSource'.
        /// </summary>
        private void ConnectionsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // 'ConnectionsSource' has been cleared, also clear 'Connections'.
                Connections.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    // For each item that has been removed from 'ConnectionsSource' also remove it from 'Connections'.
                    foreach (object obj in e.OldItems)
                    {
                        Connections.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    // For each item that has been added to 'ConnectionsSource' also add it to 'Connections'.
                    foreach (object obj in e.NewItems)
                    {
                        Connections.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Called after the visual tree of the control has been built.
        /// Search for and cache references to named parts defined in the XAML control template for NetworkView.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Cache the parts of the visual tree that we need access to later.
            _moduleItemsControl = (ModuleItemsControl)Template.FindName("PART_NodeItemsControl", this);
            if (_moduleItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_NodeItemsControl' in the visual tree for 'NetworkView'.");
            }

            // Synchronize initial selected modules to the ModuleItemsControl.
            if (_initialSelectedNodes != null && _initialSelectedNodes.Count > 0)
            {
                foreach (var node in _initialSelectedNodes)
                {
                    _moduleItemsControl.SelectedItems.Add(node);
                }
            }

            _initialSelectedNodes = null; // Don't need this any more.

            _moduleItemsControl.SelectionChanged += nodeItemsControl_SelectionChanged;

            _connectionItemsControl = (ItemsControl)Template.FindName("PART_ConnectionItemsControl", this);
            if (_connectionItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_ConnectionItemsControl' in the visual tree for 'NetworkView'.");
            }

            _dragSelectionCanvas = (FrameworkElement)Template.FindName("PART_DragSelectionCanvas", this);
            if (_dragSelectionCanvas == null)
            {
                throw new ApplicationException("Failed to find 'PART_DragSelectionCanvas' in the visual tree for 'NetworkView'.");
            }

            _dragSelectionBorder = (FrameworkElement)Template.FindName("PART_DragSelectionBorder", this);
            if (_dragSelectionBorder == null)
            {
                throw new ApplicationException("Failed to find 'PART_dragSelectionBorder' in the visual tree for 'NetworkView'.");
            }

        }

        /// <summary>
        /// Event raised when the selection in 'ModuleItemsControl' changes.
        /// </summary>
        private void nodeItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
            }
        }

        /// <summary>
        /// Find the max ZIndex of all the modules.
        /// </summary>
        internal int FindMaxZIndex()
        {
            if (_moduleItemsControl == null)
            {
                return 0;
            }

            int maxZ = 0;

            for (int nodeIndex = 0; ; ++nodeIndex)
            {
                var module = (Module)_moduleItemsControl.ItemContainerGenerator.ContainerFromIndex(nodeIndex);
                if (module == null)
                {
                    break;
                }

                if (module.ZIndex > maxZ)
                {
                    maxZ = module.ZIndex;
                }
            }

            return maxZ;
        }

        /// <summary>
        /// Find the ModuleVmBase UI element that is associated with '_module'.
        /// '_module' can be a view-model object, in which case the visual-tree
        /// is searched for the associated ModuleVmBase.
        /// Otherwise '_module' can actually be a 'ModuleVmBase' in which case it is 
        /// simply returned.
        /// </summary>
        public Module FindAssociatedNodeItem(object node)
        {
            var module = node as Module ?? _moduleItemsControl.FindAssociatedModule(node);
            return module;
        }


        /// <summary>
        /// Resize pannel min width/height
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        private void Resize(double w, double h)
        {
            if (double.IsNaN(MinWidth))
            {
                MinWidth = ActualWidth;
            }

            if (double.IsNaN(MinHeight))
            {
                MinHeight = ActualHeight;
            }

            
            if (w > MinWidth)
            {
                MinWidth = w;
            }

            if (h > MinHeight)
            {
                MinHeight = h;
            }

        }

        #endregion
    }
}
