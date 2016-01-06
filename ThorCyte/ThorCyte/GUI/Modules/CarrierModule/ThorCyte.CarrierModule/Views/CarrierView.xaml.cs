using ThorCyte.CarrierModule.ViewModels;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for CarrierWindow.xaml
    /// </summary>
    public partial class CarrierView
    {
        public CarrierView()
        {
            InitializeComponent();
            DataContext = new CarrierViewModel();
        }
    }
}
