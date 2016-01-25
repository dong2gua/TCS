using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.Converts
{
    public class ThresholdMethodConvert : IValueConverter
    {
        private const string ManualMethod = "Manual";
        private const string OtsuMethod = "Otsu";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            var method = (ThresholdMethod)value;
            switch (method)
            {
                case ThresholdMethod.Manual:
                    return param == ManualMethod;
                case ThresholdMethod.Otsu:
                    return param == OtsuMethod;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            switch (param)
            {
                case ManualMethod:
                    return ThresholdMethod.Manual;
                case OtsuMethod:
                    return ThresholdMethod.Otsu;
                default:
                    return null;
            }
        }
    }
}
