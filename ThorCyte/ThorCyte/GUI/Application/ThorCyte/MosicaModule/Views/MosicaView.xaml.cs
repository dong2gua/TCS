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
        private bool _isSelectChanged;
        private IEventAggregator _eventAggregator;
        public MosicaView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _isSelectChanged = false;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<MacroFinishEvent>().Subscribe(EndRun, ThreadOption.UIThread, true);
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExperimentLoaded);
            _eventAggregator.GetEvent<MacroRunEvent>().Subscribe(StartRun, ThreadOption.UIThread, true);
        }

        private void StartRun(int obj)
        {
            SetTabStatus(false);
        }

        private void ExperimentLoaded(int obj)
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

            if (tabControl.SelectedIndex == 0)
            {
                if (_isSelectChanged)
                {
                    _eventAggregator.GetEvent<ShowRegionEvent>().Publish("ReviewModule");
                    _isSelectChanged = false;
                }

            }
            else
            {
                if (!_isSelectChanged)
                {
                    _eventAggregator.GetEvent<ShowRegionEvent>().Publish("AnalysisModule");
                    _isSelectChanged = true; 
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
