using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.ViewModels;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for GraphicModeuleView.xaml
    /// </summary>
    public partial class GraphicModeuleView
    {
        #region Fields

        private readonly IdManager _idManager = new IdManager();

        private GraphicPanelView _panel;

        #endregion

        #region Constructor

        public GraphicModeuleView()
        {
            GraphicManagerVm graphicManagerVm;
            InitializeComponent();
            DataContext = graphicManagerVm = new GraphicManagerVm();
            var experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            GraphicModule.RegisterGaphicManager(graphicManagerVm);
            var scaid = experiment.GetCurrentScanId();

            if (scaid > 0)
            {
                graphicManagerVm.LoadXml(scaid);
            }
            _idManager.InsertId(1);
        }

        #endregion

        #region Methods

        private void OnClickName(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            var containerVm = (GraphicContainerVm)GraphicTabControl.SelectedItem;
            if (tb == null)
            {
                return;
            }
            if (e.ClickCount > 1)
            {
                containerVm.IsEdit = true;
            }
        }

        private void OnTbIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if ((bool)e.NewValue)
            {
                if (textBox != null)
                {
                    textBox.SelectionLength = textBox.Text.Count();
                    textBox.Focus();

                    //In visiblechanged event call focus,sometime not works //So call it later.
                    Dispatcher.BeginInvoke((new Action(() => Keyboard.Focus(textBox))), DispatcherPriority.Render);
                }
            }
        }

        private void OnSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                var containervm = e.RemovedItems[0] as GraphicContainerVm;
                if (containervm != null)
                {
                    containervm.IsEdit = false;
                }
            }
        }

        private void UpdatePanelLayout()
        {
            if (_panel == null)
            {
                _panel = VisualHelper.GetVisualChild<GraphicPanelView>(GraphicTabControl);
                if (_panel == null)
                {
                    return;
                }
            }
            _panel.UpdateGridLayout();
        }

        private void OnAddScattergram(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var containerVm = (GraphicContainerVm) button.DataContext;
            
            containerVm.CreateScattergram();
            UpdatePanelLayout();
        }

        private void OnAddHistogram(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var containerVm = (GraphicContainerVm)button.DataContext;
            containerVm.CreateHistogram();
            UpdatePanelLayout();
        }

        #endregion

    }
}
