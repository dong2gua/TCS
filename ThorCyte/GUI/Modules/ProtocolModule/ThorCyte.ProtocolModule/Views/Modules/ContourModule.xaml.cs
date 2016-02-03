using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ThorCyte.ProtocolModule.Views.Modules
{
    /// <summary>
    /// Interaction logic for ContourModule.xaml
    /// </summary>
    public partial class ContourModule
    {
        public ContourModule()
        {
            InitializeComponent();
        }

        //private void txt_ConponentFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    var tb = sender as TextBox;
        //    if (tb == null) return;
        //    var bindingExpression = tb.GetBindingExpression(TextBox.TextProperty);
        //    if (bindingExpression != null)
        //        bindingExpression.UpdateSource();
        //}
    }
}
