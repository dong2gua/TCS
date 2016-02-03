using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThorCyte.CarrierModule.ViewModels;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for TileView.xaml
    /// </summary>
    public partial class TileView : UserControl
    {
        public TileViewModel vm;
        
        public TileView()
        {
            InitializeComponent();
            vm = new TileViewModel();
            DataContext = vm;
        }

        private void TileView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
             //vm.ViewSizeMax = (ActualHeight > ActualWidth ? ActualHeight : ActualWidth);
        }
    }
}
