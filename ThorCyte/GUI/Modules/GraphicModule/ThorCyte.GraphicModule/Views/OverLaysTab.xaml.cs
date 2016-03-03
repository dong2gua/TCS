using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for OverLaysTab.xaml
    /// </summary>
    public partial class OverLaysTab
    {
        public OverLaysTab()
        {
            InitializeComponent();
        }

        private void OnNewOverlay(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (HistogramVm)DataContext;
            var overlayWnd = new NewOverlayWnd(vm);
            overlayWnd.ShowDialog();
        }

        private void OnEditOverlay(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (HistogramVm)DataContext;
            var overlayWnd = new NewOverlayWnd(vm,vm.SelectedOverlay,true);
            overlayWnd.ShowDialog();
        }
    }
}
