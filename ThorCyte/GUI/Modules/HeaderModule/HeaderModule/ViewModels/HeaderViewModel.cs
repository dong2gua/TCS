using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using ComponentDataService;
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
        public ICommand SaveCommand { get; set; }

        public HeaderViewModel(IUnityContainer container, IEventAggregator eventAggregator)
        {
            _unityContainer = container;
            _eventAggregator = eventAggregator;
            OpenCommand = new DelegateCommand(OpenExperiment);
            CloseCommand = new DelegateCommand(CloseExperiment);
            SaveCommand = new DelegateCommand(SaveAnalysisResult);
        }

        private void OpenExperiment()
        {
            //OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Filter = "Experiment files (*.xml, *.oct)|*.xml; *.oct|All files (*.*)|*.*";
            //if (dlg.ShowDialog() == true)
            //{
            //    string fileName = dlg.FileName;
            //    if (fileName.Contains("Run.xml"))
            //    {
            //        _experiment = new ThorCyteExperiment();
            //        _data = new ThorCyteData();
            //    }
            //    else if (fileName.Contains("Experiment.xml"))
            //    {
            //        _experiment = new ThorImageExperiment();
            //        _data = new ThorImageData();
            //    }
            //    if (_experiment != null)
            //    {
            //        _experiment.Load(fileName);
            //        _data.SetExperimentInfo(_experiment);
            //        _unityContainer.RegisterInstance<IExperiment>(_experiment);
            //        _unityContainer.RegisterInstance<IData>(_data);
            //        ComponentDataManager.Instance.Load(_experiment);
            //        if(_experiment.GetScanCount() > 0)
            //            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(1);
            //    }
            //}
        }

        private void CloseExperiment()
        {
        }

        private void SaveAnalysisResult()
        {
            //if(_experiment == null)
            //    return;
            //var di = new DirectoryInfo(_experiment.GetExperimentInfo().AnalysisPath);
            //if (!di.Exists)
            //{
            //    di.Create();
            //}
            //else
            //{
            //    foreach (FileInfo file in di.GetFiles())
            //    {
            //        file.Delete();
            //    }
            //    foreach (DirectoryInfo dir in di.GetDirectories())
            //    {
            //        dir.Delete(true);
            //    }
            //}
            //_eventAggregator.GetEvent<SaveAnalysisResultEvent>().Publish(0);
        }  
    }
}
