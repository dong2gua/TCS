using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.HeaderModule.ViewModels
{
    public class TabViewModel: BindableBase
    {
        private string _currentTab;
        private IEventAggregator _eventAggregator;

        private bool _isLoaded;

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public ICommand TabCommand { get; set; }

        public TabViewModel(IEventAggregator eventAggregator)
        {
            TabCommand = new DelegateCommand<object>(SelectTab);
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(EventHandler);
        }

        private void EventHandler(int id)
        {
            IsLoaded = true;
            SelectTab("ReviewModule");
        }

        private void SelectTab(object obj)
        {
            var str = obj as string;
            if (_currentTab != str)
            {
                _currentTab = str;
                _eventAggregator.GetEvent<ShowRegionEvent>().Publish(str);
            }
        }
    }
}
