using System;
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
        public ICommand OpenCommnad { get; set; }
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
            var scanid = 1;
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(scanid);
        }
    }


}
