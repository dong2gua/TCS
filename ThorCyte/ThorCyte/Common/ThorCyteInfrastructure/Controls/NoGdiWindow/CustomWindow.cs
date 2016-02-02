

namespace System.Windows.Controls
{
    public class CustomWindow : NoGdiWindow
    {
        public CustomWindow()
        {
            this.Style = Application.Current.Resources["CustomWindowStyle"] as Style;
        }
    }
}
