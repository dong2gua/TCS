using System;
using System.Windows;
using System.Windows.Data;

namespace ThorCyte.GraphicModule.Converts
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool flag;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else
            {
                if ((Visibility)value  == Visibility.Visible)
                {
                    return Visibility.Hidden;
                }
                flag = ((Visibility)value == Visibility.Visible);
            }
            if (parameter != null && string.Compare("false", parameter.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                flag = !flag;
            }
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
