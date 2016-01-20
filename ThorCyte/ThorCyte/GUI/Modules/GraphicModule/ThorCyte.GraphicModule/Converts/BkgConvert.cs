using System;
using System.Globalization;
using System.Windows.Data;

namespace ThorCyte.GraphicModule.Converts
{
    class BkgConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((bool)value, (string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((bool)value, (string)parameter);
        }

        private bool Convert(bool value,string parameter)
        {
            var isWhite = value;
            var para = parameter;
            if (para == "black")
            {
                return !isWhite;
            }
            return isWhite;
        }
    }
}
