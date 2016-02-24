using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.HeaderModule.Common;
using ThorCyte.HeaderModule.Views;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThorCyte.HeaderModule.ViewModels
{
    public class TabViewModel: BindableBase, ILongTimeTask
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
                    if (Directory.Exists(path))
                    {
                        var analysisViewModel = new AnalysisViewModel(info.ExperimentPath, false);
                        var w = new AnalysisView(analysisViewModel);
                        if (w.ShowDialog() == true)
                        {
                            _experiment.SetAnalysisPath(analysisViewModel.SaveAnalysisPath, true);
                        }
                    }
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
            SaveWaiting dlg = new SaveWaiting(this);
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void Save()
        {
            ComponentDataManager.Instance.Save(_experiment.GetExperimentInfo().AnalysisPath);
            _eventAggregator.GetEvent<SaveAnalysisResultEvent>().Publish(0);
            _waitingDlg.TaskEnd(null);
        }

        private Thread _saveThread;
        private SaveWaiting _waitingDlg;
        public void Start(SaveWaiting dlg)
        {
            _waitingDlg = dlg;
            _saveThread = new Thread(Save);
            _saveThread.Start();

        }
    }
}
