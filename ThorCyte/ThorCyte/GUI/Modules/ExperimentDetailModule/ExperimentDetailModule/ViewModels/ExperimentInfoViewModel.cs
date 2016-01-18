using System.Diagnostics;
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

        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { SetProperty(ref _isInitialized, value); }
        }

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

        private int _streamCount;
        public int StreamCount
        {
            set
            {
                SetProperty(ref _streamCount, value);
            }
            get { return _streamCount; }
        }

        private int _timeFrameCount;
        public int TimeFrameCount
        {
            set
            {
                SetProperty(ref _timeFrameCount, value);
            }
            get { return _timeIndex; }
        }

        private int _thirdFrameCount;
        public int ThirdFrameCount
        {
            set
            {
                SetProperty(ref _thirdFrameCount, value);
            }
            get { return _thirdFrameCount; }
        }

        public ExperimentInfoViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(Loaded);
            StreamIndex = TimeIndex = ThirdIndex = 0;
            StreamCount = ThirdFrameCount = TimeFrameCount = 0;
            IsInitialized = false;
        }

        private void Loaded(int scanId)
        {
            IsInitialized = false;
            _experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            CurrentExperiment = _experiment.GetExperimentInfo();
            CurrentScanInfo = _experiment.GetScanInfo(scanId);
            StreamIndex = TimeIndex = ThirdIndex = 1;
            StreamCount = CurrentScanInfo.StreamFrameCount;
            ThirdFrameCount = CurrentScanInfo.ThirdDimensionSteps;
            TimeFrameCount = CurrentScanInfo.TimingFrameCount;
            IsInitialized = true;
        }

        private void FrameChanged()
        {
            if (IsInitialized)
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
