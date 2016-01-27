using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace TestCarrier.ViewModels
{
    public class MainWindowVm : BindableBase
    {
        private const int Scanid = 1;
        
        
        public ICommand OpenCommnad { get; set; }
        public ICommand MacroStartCommand { get; set; }
        public ICommand MacroFinishCommand { get; set; }
        public ICommand ModeCommand { get; set; }
        private IExperiment _experiment;
        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private string _captionString;
        public string CaptionString
        {
            get { return _captionString; }
            set { SetProperty(ref _captionString, value); }
        }

        public MainWindowVm()
        {
            OpenCommnad = new DelegateCommand(Open_new);
            MacroStartCommand = new DelegateCommand(StartMacro);
            MacroFinishCommand = new DelegateCommand(EndMacro);
            ModeCommand = new DelegateCommand<string>(SwtichMode);
        }

        private void SwtichMode(string para)
        {
            switch (para)
            {
                case "Review":
                    EventAggregator.GetEvent<ShowRegionEvent>().Publish("ReviewModule");
                    Debug.WriteLine("ShowRegionEvent -- " + para);
                    break;
                case "Analysis":
                    EventAggregator.GetEvent<ShowRegionEvent>().Publish("AnalysisModule");
                    Debug.WriteLine("ShowRegionEvent -- " + para);

                    break;
            }
        }

        private int _currentWellid = 0;

        private void StartMacro()
        {
            var args = new MacroStartEventArgs
            {
                WellId = 1,
                RegionId = _currentWellid,
                TileId = 1 
            };

            _currentWellid += 1;
            EventAggregator.GetEvent<MacroStartEvnet>().Publish(args);
        }

        private void EndMacro()
        {
            _currentWellid = 0;
            EventAggregator.GetEvent<MacroFinishEvent>().Publish(Scanid);
        }


        private void Open_new()
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
            _experiment.Load(openFileDialog1.FileName);
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            container.RegisterInstance(_experiment);
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(Scanid);
        }
    }


}
