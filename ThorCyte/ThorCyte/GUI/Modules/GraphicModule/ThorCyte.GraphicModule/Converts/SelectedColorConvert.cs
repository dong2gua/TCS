using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ThorCyte.GraphicModule.Converts
{
    public class SelectedColorConvert:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            var param = (string) parameter;
            if (string.IsNullOrEmpty(param))
            {
                return true;
            }
            return color.ToString() == param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var check = (bool)value;
            return !check;
        }
    }
}
