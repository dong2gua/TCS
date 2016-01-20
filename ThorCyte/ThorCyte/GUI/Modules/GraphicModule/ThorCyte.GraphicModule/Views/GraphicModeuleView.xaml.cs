using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ComponentDataService;
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

        private GraphicManagerVm _graphicManagerVm;

        #endregion

        #region Constructor

        public GraphicModeuleView()
        {
            InitializeComponent();
            DataContext = _graphicManagerVm = new GraphicManagerVm();
            var experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            GraphicModule.RegisterGaphicManager(_graphicManagerVm);
            var scaid = experiment.GetCurrentScanId();

            if (scaid > 0)
            {
                ComponentDataManager.Instance.Load(experiment);
                _graphicManagerVm.LoadXml(scaid);
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

        #endregion


    }
}
