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
using ThorCyte.ExperimentDetailModule.ViewModels;

namespace ThorCyte.ExperimentDetailModule.Views
{
    /// <summary>
    /// Interaction logic for ExperimentInfoView.xaml
    /// </summary>
    public partial class ExperimentInfoView : UserControl
    {
        public ExperimentInfoView(ExperimentInfoViewModel experimentInfoViewModel)
        {
            InitializeComponent();
            DataContext = experimentInfoViewModel;
        }
    }
}
