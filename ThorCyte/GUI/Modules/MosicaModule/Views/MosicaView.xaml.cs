using System.Windows.Controls;
using Prism.Events;
using ThorCyte.Infrastructure.Events;

namespace MosicaModule.Views
{
    /// <summary>
    /// Interaction logic for MosicaView.xaml
    /// </summary>
    public partial class MosicaView : UserControl
    {
        private IEventAggregator _eventAggregator;
        public MosicaView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<MacroFinishEvent>().Subscribe(EndRun, ThreadOption.UIThread, true);
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExperimentLoaded);
            _eventAggregator.GetEvent<MacroRunEvent>().Subscribe(StartRun, ThreadOption.UIThread, true);
        }

        private void StartRun(int obj)
        {
            SetTabStatus(false);
        }

        private void ExperimentLoaded(int scanId)
        {
            if (TabControl.SelectedIndex != 0)
            {
                TabControl.SelectedIndex = 0;
            }
        }

        private void EndRun(int obj)
        {
            SetTabStatus(true);
            TabControl.SelectedIndex = 2;
        }

        private void OnSelectTabChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl.SelectedIndex > -1)
            {
                string tabName = "";
                switch (tabControl.SelectedIndex)
                {
                    case 0:
                        tabName = "ReviewModule";
                        break;
                    case 1:
                        tabName = "ProtocolModule";
                        break;
                    case 2:
                        tabName = "AnalysisModule";
                        break;
                }
                if (tabName != "")
                {
                    _eventAggregator.GetEvent<ShowRegionEvent>().Publish(tabName);
                }
            }
        }

        private void SetTabStatus(bool bEnable)
        {
            foreach (var item in TabControl.Items)
            {
                TabItem tabItem = item as TabItem;
                if (tabItem.Header.ToString() != "Protocol")
                    tabItem.IsEnabled = bEnable;
            }

        }
    }
}
