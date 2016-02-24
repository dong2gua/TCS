using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels;

namespace ThorCyte.ProtocolModule.Views
{
    /// <summary>
    /// Interaction logic for ProtocolView.xaml
    /// </summary>
    public partial class MacroEditor
    {
        #region Events
        //  public static event MacroEditSizeChangedHandler MacroEditSizeChanged;
        public delegate void CreateModuleHandler(Point location);
        #endregion

        #region Properties and Fields
        private readonly List<GridLength> _recentGridLengths = new List<GridLength>();

        public MarcoEditorViewModel ViewModel
        {
            get { return (MarcoEditorViewModel)DataContext; }
        }

        private static MacroEditor _macroEdit;
        public static MacroEditor Instance
        {
            get { return _macroEdit ?? (_macroEdit = ServiceLocator.Current.GetInstance<MacroEditor>()); }
        }
        public CreateModuleHandler CreateModule;

        #endregion

        #region Constructors
        public MacroEditor()
        {
            InitializeComponent();
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance(this);
            DataContext = new MarcoEditorViewModel();
        }
        #endregion

        #region Methods

        private void OnCreateModule(Point location)
        {
            if (CreateModule != null)
            {
                CreateModule(location);
            }
        }
        /// <summary>
        /// Event raised when the user has started to drag out a _connection.
        /// </summary>
        private void networkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (PortModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(pannel);

            // Delegate the real work to the view model.
            var connection = ViewModel.ConnectionDragStarted(draggedOutConnector, curDragPoint);

            // Must return the view-model object that represents the _connection via the event args.
            // This is so that NetworkView can keep track of the object while it is being dragged.
            e.Connection = connection;
        }

        /// <summary>
        /// Event raised while the user is dragging a _connection.
        /// </summary>
        private void networkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var curDragPoint = Mouse.GetPosition(pannel);
            var connection = (ConnectorModel)e.Connection;
            ViewModel.ConnectionDragging(connection, curDragPoint);
        }

        /// <summary>
        /// Event raised when the user has finished dragging out a _connection.
        /// </summary>
        private void networkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOut = (PortModel)e.ConnectorDraggedOut;
            var connectorDraggedOver = (PortModel)e.ConnectorDraggedOver;
            var newConnection = (ConnectorModel)e.Connection;
            ViewModel.ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
        }

        /// <summary>
        /// Event raised to delete the selected _module.
        /// </summary>
        private void DeleteSelectedNodes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.DeleteSelectedModules();

            ViewModel.DeleteSelectedConnectors();


        }

        /// <summary>
        /// create new _module if no _module is selected
        /// </summary>
        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ViewModel.PannelVm.SelectedViewItem;
            if (selectedItem != null && selectedItem.ItemType != ModuleType.None)
            {
                var newNodeLocation = Mouse.GetPosition(pannel);
                OnCreateModule(newNodeLocation);
            }

            if (selectedItem != null)
            {
                MessageHelper.UnSelectItem(selectedItem);
                ViewModel.PannelVm.SelectedViewItem = null;
            }
        }


        private void PannelKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ViewModel.PannelVm.SelectedModuleViewModel = null;
            }
        }

        private void ToolboxClick(object sender, RoutedEventArgs e)
        {
            var isChecked = (bool)(sender as ToggleButton).IsChecked;
            if (isChecked)
            {
                treePannel.Visibility = Visibility.Collapsed;
                splitter1.Visibility = Visibility.Collapsed;
                Grid.SetColumn(PannelBorder, 0);
                Grid.SetColumnSpan(PannelBorder, 3);
            }
            else
            {
                treePannel.Visibility = Visibility.Visible;
                splitter1.Visibility = Visibility.Visible;
                Grid.SetColumn(PannelBorder, 2);
                Grid.SetColumnSpan(PannelBorder, 1);
            }
        }


        #endregion

    }
}
