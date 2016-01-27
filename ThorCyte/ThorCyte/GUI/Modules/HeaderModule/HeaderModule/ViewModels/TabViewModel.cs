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
                    _experiment.Load(fileName);
                    _data.SetExperimentInfo(_experiment);
                    _unityContainer.RegisterInstance<IExperiment>(_experiment);
                    _unityContainer.RegisterInstance<IData>(_data);
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

            MessageBoxResult result = MessageBox.Show("Are you sure replace analysis result?", "Save analysis result", MessageBoxButton.YesNo, MessageBoxImage.Question,MessageBoxResult.No);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    var di = new DirectoryInfo(_experiment.GetExperimentInfo().AnalysisPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                    else
                    {
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }
                    ComponentDataManager.Instance.Save(_experiment.GetExperimentInfo().AnalysisPath);
                    _eventAggregator.GetEvent<SaveAnalysisResultEvent>().Publish(0);
                    break;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                    break;
            }

            
        }  
    }
}
