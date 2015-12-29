using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.ReviewModule.ViewModels
{
    public class ReviewViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;
        private IExperiment _experiment;
        private IData _data;

        public ReviewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(Loaded);
        }

        private void Loaded(int scanId)
        {
            _experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            _data = ServiceLocator.Current.GetInstance<IData>();
        }
    }
}
