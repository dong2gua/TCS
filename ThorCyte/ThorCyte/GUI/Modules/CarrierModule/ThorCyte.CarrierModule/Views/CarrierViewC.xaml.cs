using System.Windows.Controls;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for CarrierViewC.xaml
    /// </summary>
    public partial class CarrierViewC : UserControl, ICarrierView
    {
        public CarrierViewC()
        {
            InitializeComponent();
        }

        public void SetView(UserControl ctlCarri, UserControl ctlTile)
        {
            grid.Children.Clear();
            grid.Children.Add(ctlCarri);
            gridTile.Children.Clear();
            gridTile.Children.Add(ctlTile);
        }
    }
}
