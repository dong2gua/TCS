using System.Windows.Forms;
using System.Windows.Input;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace TestProtocol.ViewModels
{
    public class MainWndVm : BindableBase
    {
        public ICommand LoadExpCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        private const int Scanid = 1;

        private string _captionString;

        public string CaptionString
        {
            get { return _captionString; }
            set { SetProperty(ref _captionString, value); }
        }

        public MainWndVm()
        {
            LoadExpCommand = new DelegateCommand(OpenExperiment);
            SaveCommand = new DelegateCommand(Save);
            CaptionString = "Protocol Test View";
        }


        private IExperiment _experiment;
        private IData _dataMgr;

        private static IEventAggregator _eventAggregator;
        public static IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private void Save()
        {
            EventAggregator.GetEvent<SaveAnalysisResultEvent>().Publish(Scanid);
        }

        private void OpenExperiment()
        {
            var dlg = new OpenFileDialog { Filter = "Experiment files (*.xml, *.oct)|*.xml; *.oct|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var fileName = dlg.FileName;
                var path = dlg.FileName;
                if (fileName.Contains("Run.xml"))
                {
                    _experiment = new ThorCyteExperiment();
                    _dataMgr = new ThorCyteData();
                    path = path.Replace("Run.xml", string.Empty);
                }
                else if (fileName.Contains("Experiment.xml"))
                {
                    _experiment = new ThorImageExperiment();
                    _dataMgr = new ThorImageData();
                    path = path.Replace("Experiment.xml", string.Empty);
                }
                if (_experiment != null)
                {
                    _experiment.Load(fileName);
                    _experiment.SetAnalysisPath(path+@"Analysis\Temp");
                    _dataMgr.SetExperimentInfo(_experiment);
                    var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
                    container.RegisterInstance(_experiment);
                    container.RegisterInstance(_dataMgr);
                    ComponentDataManager.Instance.Load(_experiment);
                    EventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(1);
                }
            }
        }


    }
}
