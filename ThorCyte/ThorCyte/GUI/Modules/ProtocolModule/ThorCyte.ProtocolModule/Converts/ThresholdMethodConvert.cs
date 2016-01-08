using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.Converts
{
    class ThresholdMethodConvert : IValueConverter
    {
        private const string ManualMethod = "Manual";
        private const string StatisticalMethod = "Statistical";
        private const string OtsuMethod = "Otsu";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            var method = (ThresholdMethod)value;
            switch (method)
            {
                case ThresholdMethod.Manual:
                    return param == ManualMethod;
                case ThresholdMethod.Statistical:
                    return param == StatisticalMethod;
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
                case StatisticalMethod:
                    return ThresholdMethod.Statistical;
                case OtsuMethod:
                    return ThresholdMethod.Otsu;
                default:
                    return null;
            }
        }
    }
}
