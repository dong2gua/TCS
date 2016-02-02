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
using ThorCyte.HeaderModule.ViewModels;

namespace ThorCyte.HeaderModule.Views
{
    /// <summary>
    /// Interaction logic for AnalysisDialog.xaml
    /// </summary>
    public partial class AnalysisView : CustomWindow
    {
        public AnalysisView(AnalysisViewModel analysisViewModel)
        {
            InitializeComponent();
            DataContext = analysisViewModel;
        }
    }
}
