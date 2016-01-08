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
            vm= new ImageViewerViewModel();
            this.DataContext = vm;
            selectViewportCanvas.OnClick += SelectViewportCanvas_OnClick;
        }
        ImageViewerViewModel vm;
        private void SelectViewportCanvas_OnClick(int select)
        {
            dropDownButton.IsOpen = false;
            vm.OnViewportTypeChange(select);
        }

    }
}
