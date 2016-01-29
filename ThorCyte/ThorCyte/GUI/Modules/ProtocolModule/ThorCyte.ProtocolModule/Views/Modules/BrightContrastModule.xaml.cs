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

namespace ThorCyte.ProtocolModule.Views.Modules
{
    /// <summary>
    /// Interaction logic for BrightContrastModule.xaml
    /// </summary>
    public partial class BrightContrastModule : UserControl
    {
        public BrightContrastModule()
        {
            InitializeComponent();

            ContSld.Maximum = Math.Atan(31.0)/Math.PI*180;
        }
    }
}
