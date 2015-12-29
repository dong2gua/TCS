using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.ExperimentDetailModule.ViewModels
{
    public class ExperimentInfoViewModel :BindableBase
    {
        private IEventAggregator _eventAggregator;
        private IExperiment _experiment;

        private ExperimentInfo _experimentInfo;
        private bool _isInitialized;

        public ExperimentInfo CurrentExperiment
        {
            set { SetProperty(ref _experimentInfo, value); }
            get { return _experimentInfo; }
        }

        private ScanInfo _scanInfo;

        public ScanInfo CurrentScanInfo
        {
            set { SetProperty(ref _scanInfo, value); }
            get { return _scanInfo; }
        }

        private int _streamIndex;

        public int StreamIndex
        {
            set
            {
                SetProperty(ref _streamIndex, value);
                FrameChanged();
            }
            get { return _streamIndex; }
        }

        private int _timeIndex;

        public int TimeIndex
        {
            set
            {
                SetProperty(ref _timeIndex, value);
                FrameChanged();
            }
            get { return _timeIndex; }
        }

        private int _thirdIndex;

        public int ThirdIndex
        {
            set
            {
                SetProperty(ref _thirdIndex, value);
                FrameChanged();
            }
            get { return _thirdIndex; }
        }

        public ExperimentInfoViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(Loaded);
            StreamIndex = TimeIndex = ThirdIndex = 0;
            _isInitialized = false;
        }

        private void Loaded(int scanId)
        {
            _experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            CurrentExperiment = _experiment.GetExperimentInfo();
            CurrentScanInfo = _experiment.GetScanInfo(scanId);
            StreamIndex = TimeIndex = ThirdIndex = 1;
            _isInitialized = true;
        }

        private void FrameChanged()
        {
            if (_isInitialized)
            {
                _eventAggregator.GetEvent<FrameChangedEvent>().Publish(new FrameIndex()
                {
                    StreamId = StreamIndex,
                    ThirdStepId = ThirdIndex,
                    TimeId = TimeIndex
                });
            }
        }
    }
}
