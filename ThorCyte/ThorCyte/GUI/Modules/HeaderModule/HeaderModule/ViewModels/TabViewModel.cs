using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.HeaderModule.Views;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.HeaderModule.ViewModels
{
    public class TabViewModel: BindableBase
    {
        private string _currentTab;
        private IUnityContainer _unityContainer;
        private IEventAggregator _eventAggregator;
        private IExperiment _experiment;
        private IData _data;

        public ICommand OpenCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        private bool _isLoaded;

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public ICommand TabCommand { get; set; }

        public TabViewModel()
        {
            _unityContainer = ServiceLocator.Current.GetInstance<IUnityContainer>();
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            TabCommand = new DelegateCommand<object>(SelectTab);
            OpenCommand = new DelegateCommand(OpenExperiment);
            CloseCommand = new DelegateCommand(CloseExperiment);
            SaveCommand = new DelegateCommand(SaveAnalysisResult);
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
                    if (!_experiment.Load(fileName))
                    {
                        MessageBox.Show("Experiment file has error or not support. Please check it!", "Error Load experiment", MessageBoxButton.OK);
                        return;
                    }
                    _data.SetExperimentInfo(_experiment);
                    _unityContainer.RegisterInstance<IExperiment>(_experiment);
                    _unityContainer.RegisterInstance<IData>(_data);
                    ExperimentInfo info = _experiment.GetExperimentInfo();
                    string path = info.ExperimentPath + "\\Analysis";
                    string AnalysisPath;
                    var di = new DirectoryInfo(path);
                    if (di.Exists)
                    {
                        AnalysisViewModel analysisViewModel = new AnalysisViewModel(info.ExperimentPath, false);
                        AnalysisView w = new AnalysisView(analysisViewModel);
                        if (w.ShowDialog() != true)
                        {
                            AnalysisPath = path + "\\Temp";
                        }
                        else
                        {
                            AnalysisPath = analysisViewModel.SaveAnalysisPath;
                        }
                    }
                    else
                    {
                        di.Create();
                        AnalysisPath = path + "\\Temp";
                    }
                    _experiment.SetAnalysisPath(AnalysisPath);
                    ComponentDataManager.Instance.Load(_experiment);
                    IsLoaded = true;
                    SelectTab("ReviewModule");
                    if (_experiment.GetScanCount() > 0)
                        _eventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(1);
                }
            }
        }

        private void CloseExperiment()
        {
        }

        private void SaveAnalysisResult()
        {
            if (_experiment == null)
                return;
            string path = _experiment.GetExperimentInfo().ExperimentPath;
            AnalysisViewModel analysisViewModel = new AnalysisViewModel(path, true);
            AnalysisView w = new AnalysisView(analysisViewModel);
            if (w.ShowDialog() != true) return;
            _experiment.SetAnalysisPath(analysisViewModel.SaveAnalysisPath);
            ComponentDataManager.Instance.Save(_experiment.GetExperimentInfo().AnalysisPath);
            _eventAggregator.GetEvent<SaveAnalysisResultEvent>().Publish(0); 
        }  
    }
}
