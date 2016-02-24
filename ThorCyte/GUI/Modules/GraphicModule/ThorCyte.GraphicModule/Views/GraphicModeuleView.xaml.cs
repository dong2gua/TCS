using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
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

        private GraphicManagerVm _graphicManagerVm;

        #endregion

        #region Constructor

        public GraphicModeuleView()
        {
            InitializeComponent();
            DataContext = _graphicManagerVm = new GraphicManagerVm();
            var experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            GraphicModule.RegisterGaphicManager(_graphicManagerVm);
            if (experiment != null)
            {
                var scanid = experiment.GetCurrentScanId();

                if (scanid > 0)
                {
                    _graphicManagerVm.LoadXml(scanid);
                }
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
            e.Handled = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as GraphicPanelView;
            if (panel == null)
            {
                return;
            }
            _panel = panel;
        }

        private void OnAddScattergram(object sender, RoutedEventArgs e)
        {
            _panel.AddScattergram();
        }

        private void OnAddHistogram(object sender, RoutedEventArgs e)
        {
            _panel.AddHistogram();
        }

        private void OnDeleteGraphic(object sender, RoutedEventArgs e)
        {
            _panel.DeleteGraphic();
        }

        #endregion


    }
}
