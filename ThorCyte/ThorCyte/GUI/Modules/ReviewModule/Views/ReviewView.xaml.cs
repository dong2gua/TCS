
using System.Windows.Controls;
using ThorCyte.ReviewModule.ViewModels;

namespace ThorCyte.ReviewModule.Views
{
    /// <summary>
    /// Interaction logic for ReviewView.xaml
    /// </summary>
    public partial class ReviewView : UserControl
    {
        public ReviewView(ReviewViewModel reviewViewModel)
        {
            InitializeComponent();
            this.DataContext = reviewViewModel;
        }
    }
}
