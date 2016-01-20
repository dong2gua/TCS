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
            if (_gridItems == 0)
            {
                var itemsPresenter = VisualHelper.GetVisualChild<ItemsPresenter>(GraphicViewList);
                var uniformGrid = VisualTreeHelper.GetChild(itemsPresenter, 0) as UniformGrid;
                if (uniformGrid == null)
                {
                    return;
                }
                uniformGrid.Columns = 4;
                uniformGrid.Rows = 4;
                _row = 4;
                _gridItems = uniformGrid.Columns * uniformGrid.Rows;
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
            _isLoaded = true;
        }

        private void UpdatePanelLayout()
        {
            if (GraphicViewList.Items.Count >= _gridItems && GraphicViewList.Items.Count % _row == 0)
            {
                var itemsPresenter = VisualHelper.GetVisualChild<ItemsPresenter>(GraphicViewList);
                var uniformGrid = VisualTreeHelper.GetChild(itemsPresenter, 0) as UniformGrid;
                if (uniformGrid != null)
                {
                    uniformGrid.Rows = uniformGrid.Rows + 1;
                }
            }  
        }

        private void  OnAddScattergram(object sender, RoutedEventArgs e)
        {
            var containerVm = (GraphicContainerVm)DataContext;
            UpdatePanelLayout();
            containerVm.CreateScattergram();
        }

        private void OnAddHistogram(object sender, RoutedEventArgs e)
        {
            var containerVm = (GraphicContainerVm)DataContext;
            UpdatePanelLayout();
            containerVm.CreateHistogram();
        }

        #endregion
    }
}
