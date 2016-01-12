using System.Windows;
using System.Windows.Controls;

namespace ThorCyte.ImageViewerModule.View
{
    /// <summary>
    /// Interaction logic for ViewportView.xaml
    /// </summary>
    public partial class ViewportView : UserControl
    {
        public ViewportView()
        {
            InitializeComponent();
        }
        private void ExpandList_Click(object sender, RoutedEventArgs e)
        {
            if (this.listView.Visibility == Visibility.Collapsed)
                this.listView.Visibility = Visibility.Visible;
            else
                this.listView.Visibility = Visibility.Collapsed;
        }
    }
}
