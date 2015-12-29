using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.HeaderModule.ViewModels
{
    public class HeaderViewModel : BindableBase
    {
        private IUnityContainer _unityContainer;
        private IEventAggregator _eventAggregator;
        private IExperiment _experiment;
        private IData _data;

        public ICommand OpenCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand TabCommand { get; set; }
        

        public HeaderViewModel(IUnityContainer container, IEventAggregator eventAggregator)
        {
            _unityContainer = container;
            _eventAggregator = eventAggregator;
            OpenCommand = new DelegateCommand(OpenExperiment);
            CloseCommand = new DelegateCommand(CloseExperiment);
            TabCommand = new DelegateCommand<object>(SelectTab);
            SelectTab("ReviewModule");
        }

        private void OpenExperiment()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Experiment files (*.xml, *.oct)|*.xml; *.oct|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                string fileName = dlg.FileName;
                if (fileName.Contains("Run.xml"))
                {
                    _experiment = new ThorCyteExperiment();
                    _data = new ThorCyteData();
                }
                else if (fileName.Contains("Experiment.xml"))
                {
                    _experiment = new ThorImageExperiment();
                    _data = new ThorImageData();
                }
                if (_experiment != null)
                {
                    _experiment.Load(fileName);
                    _data.SetExperimentInfo(_experiment);
                    _unityContainer.RegisterInstance<IExperiment>(_experiment);
                    _unityContainer.RegisterInstance<IData>(_data);
                    if(_experiment.GetScanCount() > 0)
                        _eventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(1);
                }
            }
        }

        private void CloseExperiment()
        {
        }

        private void SelectTab(object obj)
        {
            var str = obj as string;
            _eventAggregator.GetEvent<ShowRegionEvent>().Publish(str);
        }
    }
}
