using System.Windows;
using TestCarrier.ViewModels;

namespace TestCarrier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowVm();
        }
    }

}
