using System.Windows.Controls;
using ThorCyte.Statistic.ViewModels;

namespace ThorCyte.Statistic.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class StatisticView : UserControl
    {
        public StatisticView(StatisticViewModel pVM)
        {
            InitializeComponent();
            DataContext = pVM;
        }
    }
}
