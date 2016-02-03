using System.Windows.Controls;
using ThorCyte.ImageViewerModule.Viewmodel;

namespace ThorCyte.ImageViewerModule.View
{
    /// <summary>
    /// Interaction logic for ImageViewerView.xaml
    /// </summary>
    public partial class ImageViewerView : UserControl
    {
        private ImageViewerViewModel vm;
        public ImageViewerView()
        {
            InitializeComponent();
            vm= new ImageViewerViewModel();
            this.DataContext = vm;
        }
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            popup.IsOpen = true;
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            popup.IsOpen = false;
            var listView = sender as ListView;
            vm.OnViewportTypeChange(listView.SelectedIndex);
            var selection = (listView.SelectedItem as ContentControl).Content as Image;
            if(selection!=null)
            imgViewportLayout.Source = selection.Source;
        }
    }
}
