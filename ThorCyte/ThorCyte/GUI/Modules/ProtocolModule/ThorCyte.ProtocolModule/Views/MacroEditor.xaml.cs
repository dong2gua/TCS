using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.ViewModels;

namespace ThorCyte.ProtocolModule.Views
{
    /// <summary>
    /// Interaction logic for ProtocolView.xaml
    /// </summary>
    public partial class MacroEditor : UserControl
    {
       #region Events

      //  public static event MacroEditSizeChangedHandler MacroEditSizeChanged;

        public delegate void CreateModuleHandler(Point location);

        #endregion

        #region Properties and Fields

        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private static MacroEditor _macroEdit = new MacroEditor();

        public static MacroEditor Instance
        {
            get { return _macroEdit; }
        }

        public CreateModuleHandler CreateModule;

        #endregion

        #region Constructors

        private MacroEditor()
        {
            InitializeComponent();
            DataContext = MainWindowViewModel.Instance;
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
                selectedItem.IsSelected = false;
            }
        }

        private void OnTreeviewSelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            if (tree != null)
            {
                ViewModel.PannelVm.SelectedViewItem = tree.SelectedItem as TreeViewItemModel;
            }
        }

        private bool IsChildInTree(DependencyObject child,Type parentType)
        {
            var parent = child;
            while (parent != null)
            {
                if (VisualTreeHelper.GetParent(parent) == null)
                {
                    return false;
                }
                if (parent.GetType() == parentType)
                {
                    return true;
                }
                parent = VisualTreeHelper.GetParent(parent);

                if (parent.GetType() == parentType)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            var isModule = IsChildInTree((DependencyObject) e.OriginalSource, typeof (Module));
            if (isModule) 
            {
                var vm = ViewModel.GetSelectedModule();
                ViewModel.PannelVm.SelectedModuleViewModel = vm;
             }
            else
            {
                ViewModel.PannelVm.UnSelectedAll();
                ViewModel.PannelVm.SelectedModuleViewModel = null;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ReAnalysisImage();
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
                treeview.Visibility = Visibility.Collapsed;
                splitter.Visibility = Visibility.Collapsed;
                Grid.SetColumn(pannelGrid, 0);
                Grid.SetColumnSpan(pannelGrid, 3);
            }
            else
            {
                treeview.Visibility = Visibility.Visible;
                splitter.Visibility = Visibility.Visible;
                Grid.SetColumn(pannelGrid, 2);
                Grid.SetColumnSpan(pannelGrid, 1);
            }
        }

        private void CollapseClick(object sender, RoutedEventArgs e)
        {
            var isChecked = (bool)(sender as ToggleButton).IsChecked;
            if (isChecked)
            {
                treeview.Visibility = Visibility.Collapsed;
                pannelGrid.Visibility = Visibility.Collapsed;
                toolPannel.Visibility = Visibility.Collapsed;
                splitter.Visibility = Visibility.Collapsed;
                expandcollapse.Source = (BitmapImage)Resources["expandImg"];
                Grid.SetColumn(contentGrid, 0);
                Grid.SetColumnSpan(contentGrid, 4);
                //OnSizeChanged(contentBorder.ActualWidth);
            }
            else
            {
                treeview.Visibility = Visibility.Visible;
                pannelGrid.Visibility = Visibility.Visible;
                toolPannel.Visibility = Visibility.Visible;
                splitter.Visibility = Visibility.Visible;
                expandcollapse.Source = (BitmapImage)Resources["collapseImg"];
                Grid.SetColumn(contentGrid, 3);
                Grid.SetColumnSpan(contentGrid, 1);
                //OnSizeChanged(880);
            }
        }

        //private void OnSizeChanged(double newWidth)
        //{
        //    if (MacroEditSizeChanged != null)
        //    {
        //        MacroEditSizeChanged(newWidth);
        //    }
        //}
        #endregion

    }
}
