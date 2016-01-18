using System.Windows.Controls;
using ThorCyte.HeaderModule.ViewModels;

namespace ThorCyte.HeaderModule.Views
{
    /// <summary>
    /// Interaction logic for TabView.xaml
    /// </summary>
    public partial class TabView : UserControl
    {
        public TabView(TabViewModel tabViewModel)
        {
            InitializeComponent();
            DataContext = tabViewModel;
        }
    }
}
