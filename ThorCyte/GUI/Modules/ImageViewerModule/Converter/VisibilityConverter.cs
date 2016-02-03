using System;
using System.Windows;
using System.Windows.Data;

namespace ThorCyte.ImageViewerModule.Converter
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool visibile;
            if (value is bool)
                visibile = (bool)value;
            else if (value is Visibility)
                visibile = (Visibility)value == Visibility.Visible;
            else
                return Visibility.Hidden;
            if (parameter != null)
            {
                if (string.Compare("false", parameter.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                    visibile = !visibile;
            }
            return visibile ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
