using System.Windows.Controls;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.ViewModels;

namespace ThorCyte.ProtocolModule.Views
{
    /// <summary>
    /// Interaction logic for ImageDisplayView.xaml
    /// </summary>
    public partial class ImageDisplayView : UserControl
    {
        public ImageDisplayView()
        {
            InitializeComponent();
            DataContext = new ImageDisplayViewModel();
        }

        public ImageDisplayView(DisplayImageEventArgs args)
        {
            InitializeComponent();
            DataContext = new ImageDisplayViewModel(args);
        }
    }
}
