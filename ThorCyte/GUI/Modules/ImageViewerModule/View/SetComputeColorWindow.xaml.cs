using System.Windows.Controls;
using ThorCyte.ImageViewerModule.Viewmodel;

namespace ThorCyte.ImageViewerModule.View
{
    /// <summary>
    /// Interaction logic for SetComputeColorWindow.xaml
    /// </summary>
    public partial class SetComputeColorWindow : CustomWindow
    {
        public SetComputeColorWindow()
        {
            InitializeComponent();
        }
        private void OK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = this.DataContext as SetComputeColorViewModel;
            if (vm == null) return;
            if (!vm.VertifyInput()) return;
            colordg.CommitEdit(DataGridEditingUnit.Row, true);
            this.DialogResult = true;
            this.Close();
        }
        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();

        }
    }
}
