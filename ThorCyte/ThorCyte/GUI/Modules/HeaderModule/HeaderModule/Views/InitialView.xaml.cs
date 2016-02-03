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

namespace ThorCyte.HeaderModule.Views
{
    /// <summary>
    /// Interaction logic for InitialView.xaml
    /// </summary>
    public partial class InitialView : UserControl
    {
        public InitialView()
        {
            InitializeComponent();
        }

        private void Grid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = sender as Grid;
            double size = grid.ActualHeight > grid.ActualWidth ? grid.ActualWidth : grid.ActualHeight;
            this.SoftWare.FontSize = size/10;
            this.Version.FontSize = size/25;

        }
    }
}
