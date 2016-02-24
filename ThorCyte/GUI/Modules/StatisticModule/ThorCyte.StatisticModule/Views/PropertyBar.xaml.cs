using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThorCyte.Statistic.Views
{
	/// <summary>
	/// PropertyBar.xaml 的交互逻辑
	/// </summary>
	public partial class PropertyBar : UserControl
	{
		public PropertyBar()
		{
			this.InitializeComponent();
		}

	    public void RFNameDoubleClick(object sender, EventArgs eventArgs)
	    {
	        var control = (TextBox) sender;
	        control.Background = new SolidColorBrush(Colors.White);
	        control.IsReadOnly = false;
            control.Select(0, control.Text.Length);
	    }

	    public void RFNameLostFocus(object sender, EventArgs eventArgs)
	    {
	        var control = (TextBox) sender;
	        control.Background = new SolidColorBrush(Colors.DarkGray);
            control.IsReadOnly = true;
	    }
        
	    public void RFNameKeyDown(object sender, KeyEventArgs eventArgs)
	    {
	        if (eventArgs.Key == Key.Enter)
	        {
	            var control = (TextBox) sender;
	            control.Background = new SolidColorBrush(Colors.DarkGray);
	            control.IsReadOnly = true;

                FrameworkElement parent = (FrameworkElement)control.Parent;
                while (parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
                {
                    parent = (FrameworkElement)parent.Parent;
                }

                DependencyObject scope = FocusManager.GetFocusScope(textBox);
                FocusManager.SetFocusedElement(scope, parent as IInputElement);
	        }
	    }
	}
}