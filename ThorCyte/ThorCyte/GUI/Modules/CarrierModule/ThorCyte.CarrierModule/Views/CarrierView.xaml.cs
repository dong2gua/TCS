using System.Diagnostics;
using System.Windows.Controls;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for CarrierWindow.xaml
    /// </summary>
    public partial class CarrierView : UserControl , ICarrierView
    {
        public CarrierView()
        {
            InitializeComponent();
        }

        public void SetView(UserControl ctlCarri,UserControl ctlTile)
        {            
            grid.Children.Clear();            
            grid.Children.Add(ctlCarri);
            gridTile.Children.Clear();
            gridTile.Children.Add(ctlTile);
        }
    }
}
