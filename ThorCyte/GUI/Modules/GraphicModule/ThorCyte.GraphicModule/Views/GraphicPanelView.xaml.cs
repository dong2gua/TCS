using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.ViewModels;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GraphicPanelView
    {
        #region Fields

        private bool _isLoaded;

        private UniformGrid _uniformgrid;

        public static List<GraphicDetailWnd> DetailWndList = new List<GraphicDetailWnd>();

        DragDropHelper<GraphicUcBase> _dragMgr;

        #endregion

        #region Constructor

        public GraphicPanelView()
        {
            InitializeComponent();
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<DelateGraphicEvent>()
                .Subscribe(DeleteGraphic);
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<ShowDetailGraphicEvent>()
                .Subscribe(OnShowDetail);
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<ShowRegionEvent>().Subscribe(OnSwitchTab);

            ServiceLocator.Current.GetInstance<IEventAggregator>().
                GetEvent<ExperimentLoadedEvent>().Subscribe(OnLoadExperiment);

            _dragMgr = new DragDropHelper<GraphicUcBase>(GraphicViewList)
            {
                ShowDragAdorner = true
            };
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
            UpdateGridLayout();
            _isLoaded = true;
        }

        private void UpdateGridLayout()
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
            if (GraphicViewList.ActualWidth <= 640)
            {
                _uniformgrid.Columns = 1;
            }
            else if (GraphicViewList.ActualWidth > 640 && GraphicViewList.ActualWidth <= 960)
            {
                _uniformgrid.Columns = 2;
            }
            else if (GraphicViewList.ActualWidth > 960 && GraphicViewList.ActualWidth < 1280)
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

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var newDataContext = e.NewValue as GraphicContainerVm;
            if (newDataContext == null)
            {
                return;
            }
            GraphicViewList.Items.Clear();

            foreach (var graphivm in newDataContext.GraphicVmList)
            {
                var scattervm = graphivm as ScattergramVm;
                if (scattervm != null)
                {
                    if (newDataContext.GraphicDictionary.ContainsKey(scattervm.Id))
                    {
                        var graphic = newDataContext.GraphicDictionary[scattervm.Id].Item1 as ScattergramView;
                        if (graphic != null && !GraphicViewList.Items.Contains(graphic))
                        {
                            GraphicViewList.Items.Add(graphic);
                        }
                    }
                }
                else
                {
                    if (newDataContext.GraphicDictionary.ContainsKey(graphivm.Id))
                    {
                        var graphic = newDataContext.GraphicDictionary[graphivm.Id].Item1 as HistogramView;
                        if (graphic != null && !GraphicViewList.Items.Contains(graphic))
                        {
                            GraphicViewList.Items.Add(graphic);
                        }
                    }
                }
            }

            var index = -1;
            if (newDataContext.SelectedGraphic != null)
            {
                index = newDataContext.GraphicVmList.IndexOf(newDataContext.SelectedGraphic);
            }
            if (index >= 0 && GraphicViewList.Items.Count > index)
            {
                GraphicViewList.SelectedIndex = index;
            }
        }

        public void AddScattergram()
        {
            var containerVm = DataContext as GraphicContainerVm;
            if (containerVm == null)
            {
                return;
            }
            var vm = containerVm.CreateScattergram();
            var scattergram = new ScattergramView(vm);
            vm.ViewDispatcher = Dispatcher;
            containerVm.GraphicDictionary.Add(vm.Id, new Tuple<GraphicUcBase, GraphicVmBase>(scattergram, vm));
            GraphicModule.GraphicManagerVmInstance.UpdateRegionList();
            GraphicViewList.Items.Add(scattergram);
            if (containerVm.GraphicVmList.Count == 1)
            {
                containerVm.SelectedGraphic = vm;
                GraphicViewList.SelectedIndex = 0;
            }
            UpdateGridLayout();
        }

        public void AddHistogram()
        {
            var containerVm = DataContext as GraphicContainerVm;
            if (containerVm == null)
            {
                return;
            }
            var vm = containerVm.CreateHistogram();
            var histogram = new HistogramView(vm);
            vm.ViewDispatcher = Dispatcher;
            containerVm.GraphicDictionary.Add(vm.Id, new Tuple<GraphicUcBase, GraphicVmBase>(histogram, vm));
            GraphicModule.GraphicManagerVmInstance.UpdateRegionList();
            GraphicViewList.Items.Add(histogram);

            if (containerVm.GraphicVmList.Count == 1)
            {
                containerVm.SelectedGraphic = vm;
                GraphicViewList.SelectedIndex = 0;
            }
            UpdateGridLayout();
        }

        private void OnSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            var containerVm = DataContext as GraphicContainerVm;
            if (containerVm == null || GraphicViewList.SelectedItem == null)
            {
                return;
            }
            containerVm.SelectedGraphic = null;
            containerVm.SelectedGraphic = (GraphicVmBase)((GraphicUcBase)GraphicViewList.SelectedItem).DataContext;
        }


        public void DeleteGraphic(int id)
        {
            var containerVm = DataContext as GraphicContainerVm;
            if (containerVm == null)
            {
                return;
            }
            var idStr = id.ToString(CultureInfo.InvariantCulture);
            containerVm.OnDeleteGraphic(idStr);
            var deleteIndex = -1;
            var selectedIndex = GraphicViewList.SelectedIndex;
            foreach (var item in GraphicViewList.Items)
            {
                var vm = item as GraphicUcBase;
                if (vm == null)
                {
                    continue;
                }
                if (vm.Id == idStr)
                {
                    deleteIndex = GraphicViewList.Items.IndexOf(item);
                    GraphicViewList.Items.Remove(item);
                    break;
                }
            }
            if (selectedIndex > deleteIndex)
            {
                selectedIndex = selectedIndex - 1;
            }
            if (GraphicViewList.Items.Count == 0)
            {
                return;
            }
            if (GraphicViewList.Items.Count > selectedIndex)
            {
                GraphicViewList.SelectedItem = selectedIndex >= 0 ? GraphicViewList.Items[selectedIndex] : null;
            }
            else
            {
                GraphicViewList.SelectedItem = GraphicViewList.Items[GraphicViewList.Items.Count - 1];
            }

            if (GraphicViewList.SelectedItem != null)
            {
                containerVm.SelectedGraphic = (GraphicVmBase)((GraphicUcBase)GraphicViewList.SelectedItem).DataContext;
            }
            UpdateGridLayout();
        }

        private void OnShowDetail(string id)
        {
            var graphcUc = GraphicViewList.SelectedItem as GraphicUcBase;
            if (graphcUc == null || graphcUc.Id != id)
            {
                return;
            }
            var wnd = new GraphicDetailWnd
            {
                Content = graphcUc,
            };
            DetailWndList.Add(wnd);
            graphcUc.SetCloseButtonState(true);
            graphcUc.ClearHeightBinding();
            GraphicViewList.Items.Remove(graphcUc);
            wnd.Show();
            UpdateGridLayout();
            wnd.MouseLeftButtonDown += delegate
            {
                var containerVm = DataContext as GraphicContainerVm;
                if (containerVm==null)
                {
                    return;
                }
                GraphicViewList.SelectedItem = null;
                containerVm.SelectedGraphic = ((GraphicUcBase) wnd.Content).DataContext as GraphicVmBase;
            };
            wnd.Closed += delegate
            {
                var content = wnd.Content as GraphicUcBase;
                if (content == null)
                {
                    return;
                }
                wnd.Content = null;
                var index = GetIndex(content);
                if (GraphicViewList.Items.Count > index)
                {
                    GraphicViewList.Items.Insert(index, content);
                }
                else
                {
                    GraphicViewList.Items.Add(content);
                }
                if (GraphicViewList.SelectedItem == null)
                {
                    GraphicViewList.SelectedIndex = GraphicViewList.Items.Count > index
                        ? index : GraphicViewList.Items.Count - 1;
                }
                graphcUc.SetCloseButtonState(false);
                graphcUc.SetHeightBinding();
                DetailWndList.Remove(wnd);
                wnd = null;
                UpdateGridLayout();
            };
        }

        private int GetIndex(GraphicUcBase graphic)
        {
            var index = 0;
            var containerVm = (GraphicContainerVm)DataContext;
            var baseIndex = containerVm.GraphicVmList.IndexOf(graphic.GraphicVm);
            if (baseIndex == 0)
            {
                index = 0;
            }
            else
            {
                var beginIndex = GraphicViewList.Items.Count - 1 >= baseIndex
                    ? baseIndex   : GraphicViewList.Items.Count - 1;
                for (var i = beginIndex;i>=0;i--)
                {
                    var graphicUc = GraphicViewList.Items[i] as GraphicUcBase;
                    if (graphicUc == null)
                    {
                        continue;
                    }
                    var currentIndex = containerVm.GraphicVmList.IndexOf(graphicUc.GraphicVm);
                    if (currentIndex < baseIndex)
                    {
                        index = i+1;
                        break;
                    }
                }
            }
            return index;
        }

        private void OnSwitchTab(string tabName)
        {
            if (DetailWndList == null || DetailWndList.Count == 0)
            {
                return;
            }
            foreach (var wnd in DetailWndList)
            {
                wnd.Visibility = tabName == "AnalysisModule" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OnLoadExperiment(int scanid)
        {
            while (DetailWndList.Count > 0)
            {
                DetailWndList[0].Close();
            }
        }

        #endregion
    }
}
