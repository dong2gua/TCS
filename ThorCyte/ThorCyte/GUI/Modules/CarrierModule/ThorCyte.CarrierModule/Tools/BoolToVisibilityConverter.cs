using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThorCyte.CarrierModule.Tools
{
    public class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return ((Visibility)value == Visibility.Visible);
        }
    }
}
