using System.Windows.Controls;
using ThorCyte.ImageViewerModule.Viewmodel;
namespace ThorCyte.ImageViewerModule.View
{
    /// <summary>
    /// Interaction logic for ImageViewerView.xaml
    /// </summary>
    public partial class ImageViewerView : UserControl
    {
        public ImageViewerView()
        {
            InitializeComponent();
            this.DataContext = new ImageViewerViewModel();
        }
    }
}
