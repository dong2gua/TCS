using System;
using System.Globalization;
using System.Windows.Data;

namespace ThorCyte.GraphicModule.Converts
{
    class IsEnabledConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isChecked =(bool)value;
            return !isChecked;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
