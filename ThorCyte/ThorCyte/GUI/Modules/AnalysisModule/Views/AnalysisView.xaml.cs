using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ThorCyte.AnalysisModule.ViewModels;

namespace ThorCyte.AnalysisModule.Views
{
    /// <summary>
    /// Interaction logic for AnalysisView.xaml
    /// </summary>
    public partial class AnalysisView : UserControl
    {
        public AnalysisView(AnalysisViewModel analysisViewModel)
        {
            InitializeComponent();
            this.DataContext = analysisViewModel;
        }
    }
}
