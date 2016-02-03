using System.Windows;
using System.Windows.Controls;
using ThorCyte.AnalysisModule.ViewModels;

namespace ThorCyte.AnalysisModule.Views
{
    /// <summary>
    /// Interaction logic for AnalysisView.xaml
    /// </summary>
    public partial class AnalysisView : UserControl
    {
        public AnalysisView(AnalysisViewModel analysisViewModel)
        {
            InitializeComponent();
            DataContext = analysisViewModel;
            //var s = new Style();
            //s.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            //TabControl.ItemContainerStyle = s;
        }

        //private void TabOnChecked(object sender, RoutedEventArgs e)
        //{
        //    var btn = sender as RadioButton;
        //    TabControl.SelectedIndex = btn.TabIndex;
        //}
    }
}
