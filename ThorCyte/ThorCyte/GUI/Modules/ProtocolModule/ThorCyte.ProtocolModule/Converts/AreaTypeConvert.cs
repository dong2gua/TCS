using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.Converts
{
    public class AreaTypeConvert : IValueConverter
    {
        private const string Um = "um";
        private const string Mm = "mm";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var unitType = (UnitType)value;
            var param = (string)parameter;
            switch (unitType)
            {
                case UnitType.Micron:
                    return param == Um;
                case UnitType.Mm:
                    return param == Mm;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            switch (param)
            {
                case Um:
                    return UnitType.Micron;
                case Mm:
                    return UnitType.Mm;
                default:
                    return null;
            }
        }
    }
}
