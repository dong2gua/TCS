using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Converts
{
    class ColorTypeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorInfo = (ColorType)value;
            return colorInfo == ColorType.Customer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
