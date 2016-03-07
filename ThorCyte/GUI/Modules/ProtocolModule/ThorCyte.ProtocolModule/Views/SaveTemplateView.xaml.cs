using System.Windows.Media;
using ThorCyte.ProtocolModule.ViewModels;

namespace ThorCyte.ProtocolModule.Views
{
    /// <summary>
    /// Interaction logic for SaveTemplateView.xaml
    /// </summary>
    public partial class SaveTemplateView
    {
        public SaveTemplateView()
        {
            InitializeComponent();
            DataContext = new SaveTemplateViewModel(this);
        }
    }
}
