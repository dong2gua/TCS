using System.Windows.Forms;
using System.Windows.Input;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace TestProtocol.ViewModels
{
    public class MainWndVm:BindableBase
    {
        public ICommand LoadExpCommand { get; private set; }

        private string _captionString;

        public string CaptionString
        {
            get { return _captionString; }
            set { SetProperty(ref _captionString, value); }
        }

        public MainWndVm()
        {
            LoadExpCommand = new DelegateCommand(LoadExp);
            CaptionString = "Protocol Test View";
        }


        private IExperiment _experiment;
        private IData _dataMgr;
        //private IComponentDataService _componentDataService;

        private static IEventAggregator _eventAggregator;
        public static IEventAggregator EventAggregator
        {
            get {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }


        private void LoadExp()
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = @"XML files (*.XML)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = false
            };
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            CaptionString = openFileDialog1.FileName;
            _experiment = new ThorCyteExperiment();
            if (openFileDialog1.FileName.ToUpper().EndsWith("RUN.XML"))
            {
                _dataMgr = new ThorCyteData();
            }
            else
            {
                _dataMgr = new ThorImageData();
            }
            _experiment.Load(openFileDialog1.FileName);
            _dataMgr.SetExperimentInfo(_experiment);
            ComponentDataManager.Instance.Load(_experiment);
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            container.RegisterInstance(_experiment);
            container.RegisterInstance(_dataMgr);
            const int scanid = 1;
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(scanid);
        }


    }
}
