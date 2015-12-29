using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.CarrierModule.ViewModels;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for TileView.xaml
    /// </summary>
    public partial class TileView : UserControl
    {
        public TileViewModel vm;
        
        public TileView()
        {
            InitializeComponent();
            vm = new TileViewModel();
            this.DataContext = vm;
        }


        private void TileView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            vm.ViewSizeMax = (ActualHeight > ActualWidth ? ActualHeight : ActualWidth) - 20;
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            vm.ViewSizeMax = (ActualHeight > ActualWidth ? ActualHeight : ActualWidth) - 20;
        }
    }
}
