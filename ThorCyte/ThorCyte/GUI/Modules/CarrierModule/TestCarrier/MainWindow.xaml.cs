using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace TestCarrier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private IExperiment _experiment;

        private static IEventAggregator EventAggregator
        {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }

        private void Open_new(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog
            {
                //InitialDirectory = "c:\\",
                Filter = "XML files (*.XML)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = false
            };
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                if (openFileDialog1.FileName.ToUpper().EndsWith("RUN.XML"))
                {
                    _experiment = new ThorCyteExperiment();
                    var dir = openFileDialog1.FileName.Replace(openFileDialog1.SafeFileName, string.Empty);
                    _experiment.Load(dir);
                }
                else
                {
                    _experiment = new ThorImageExperiment();
                    _experiment.Load(openFileDialog1.FileName);
                }

                ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance<IExperiment>(_experiment);

                var scanid = 1;
                EventAggregator.GetEvent<ExperimentLoadedEvent>().Publish(scanid);
            }
        }

        private void Review_Rgeion(object sender, RoutedEventArgs e)
        {
            EventAggregator.GetEvent<ShowRegionEvent>().Publish("ReviewModule");
        }

        private void Analysis_Region(object sender, RoutedEventArgs e)
        {
            EventAggregator.GetEvent<ShowRegionEvent>().Publish("AnalysisModule");
        }

    }

}
