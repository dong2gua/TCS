using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GraphicPanelView
    {
        #region Fields

        private int _gridItems;

        private int _row;

        private bool _isLoaded;

        private UniformGrid _uniformgrid;

        #endregion

        #region Constructor

        public GraphicPanelView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        #endregion

        #region Methods

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }
            var itemsPresenter = VisualHelper.GetVisualChild<ItemsPresenter>(GraphicViewList);
            _uniformgrid = VisualTreeHelper.GetChild(itemsPresenter, 0) as UniformGrid;
            if (_uniformgrid == null)
            {
                return;
            }

            var containerVm = (GraphicContainerVm)DataContext;
            var items = VisualHelper.GetChildObjects<GraphicUcBase>(GraphicViewList, "");
            foreach (var graphicview in items)
            {
                var vm = (GraphicVmBase)graphicview.DataContext;
                if (!containerVm.GraphicDictionary.ContainsKey(vm.Id))
                {
                    containerVm.GraphicDictionary.Add(vm.Id, new Tuple<GraphicUcBase, GraphicVmBase>(graphicview, vm));
                }
            }
            UpdateGridLayout();
            _isLoaded = true;
        }

        public void UpdateGridLayout()
        {
            if (_uniformgrid == null)
            {
                return;
            }
            var containerVm = (GraphicContainerVm)DataContext;
            if (containerVm == null)
            {
                return;
            }
            if (GraphicViewList.ActualWidth <= 900 )
            {
                _uniformgrid.Columns = 2;
            }
            else if (GraphicViewList.ActualWidth > 900 && GraphicViewList.ActualWidth < 1200)
            {
                _uniformgrid.Columns = 3;
            }
            else
            {
                _uniformgrid.Columns = 4;
            }
            if (containerVm.GraphicVmList.Count >= 3 * _uniformgrid.Columns)
            {
                if (containerVm.GraphicVmList.Count % _uniformgrid.Columns == 0)
                {
                    _uniformgrid.Rows = containerVm.GraphicVmList.Count / _uniformgrid.Columns;
                }
                else
                {
                    _uniformgrid.Rows = containerVm.GraphicVmList.Count / _uniformgrid.Columns + 1;
                }
            }
            else
            {
                _uniformgrid.Rows = 3;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_uniformgrid == null)
            {
                return;
            }
            UpdateGridLayout();
        }

        #endregion
    }
}
