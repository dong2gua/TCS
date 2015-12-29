using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for CarrierViewContainer.xaml
    /// </summary>
    public partial class CarrierViewContainer : UserControl
    {
        public CarrierViewContainer()
        {
            InitializeComponent();
        }

        public void SetView(UserControl c)
        {
            GContainer.Children.Clear();
            GContainer.Children.Add(c);
        }
    }
}
