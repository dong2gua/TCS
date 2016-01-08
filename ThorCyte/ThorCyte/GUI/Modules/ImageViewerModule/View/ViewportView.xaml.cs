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
using System.IO;
using System.Xml.Serialization;
using ThorCyte.ImageViewerModule.Viewmodel;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.ImageViewerModule.DrawTools;
using ThorCyte.Infrastructure.Interfaces;
using ImageProcess;
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
            {
                this.listView.Visibility = Visibility.Visible;
            }
            else
            {
                this.listView.Visibility = Visibility.Collapsed;
            }

        }
    }
}
