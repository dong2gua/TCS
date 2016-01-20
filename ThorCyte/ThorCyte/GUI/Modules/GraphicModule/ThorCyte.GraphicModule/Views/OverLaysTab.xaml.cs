using System.Linq;
using System.Windows.Media;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.Utils;
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
            OverlayInfo.Clear();
            overlayWnd.ShowDialog();
        }

        private void OnEditOverlay(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (HistogramVm)DataContext;
            var overlayWnd = new NewOverlayWnd(vm,vm.SelectedOverlay.Name,true);
            OverlayInfo.OverlayName = vm.SelectedOverlay.Name;
            if (vm.SelectedOverlay.OverlayColorInfo.Type != ColorType.Customer)
            {
                OverlayInfo.CurrentColorInfo =
                    OverlayInfo.ColorList.FirstOrDefault(colorInfo => colorInfo.ColorBrush.Color == vm.SelectedOverlay.OverlayColorInfo.ColorBrush.Color);
            }
            else
            {
                var count = OverlayInfo.ColorList.Count;
                OverlayInfo.CurrentColorInfo = OverlayInfo.ColorList[count - 1];
                OverlayInfo.CurrentColorInfo.ColorBrush = new SolidColorBrush(vm.SelectedOverlay.OverlayColorInfo.ColorBrush.Color);
            }

            overlayWnd.ShowDialog();
        }
    }
}
