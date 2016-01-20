using System;
using System.Globalization;
using System.Windows.Data;

namespace ThorCyte.GraphicModule.Converts
{
    class ResolutionConvert : IValueConverter
    {
        private const string HighResolutionStr = "high";
        private const string LowResolutionStr = "low";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isHighResolution = (bool)value;
            var param = (string)parameter;
            return Convert(isHighResolution, param);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isHighResolution = (bool)value;
            var param = (string)parameter;
            return Convert(isHighResolution,param);
        }

        private bool Convert(bool isHighResolution, string param)
        {
            if (isHighResolution)
            {
                return param == HighResolutionStr;
            }
            else
            {
                return param == LowResolutionStr;
            }
        }
    }
}
