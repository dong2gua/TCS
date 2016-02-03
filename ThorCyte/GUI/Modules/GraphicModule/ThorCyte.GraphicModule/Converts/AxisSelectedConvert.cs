using System;
using System.Globalization;
using System.Windows.Data;

namespace ThorCyte.GraphicModule.Converts
{
    public class AxisSelectedConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isSelected = (bool) value;
            var param = (string) parameter;
            return Convert(isSelected, param);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isSelected = (bool)value;
            var param = (string)parameter;
            return Convert(isSelected,param);
        }

        private bool Convert(bool isSelected, string param)
        {
            if (isSelected)
            {
                return param == "X";
            }
            return param != "X";
        }
    }
}
