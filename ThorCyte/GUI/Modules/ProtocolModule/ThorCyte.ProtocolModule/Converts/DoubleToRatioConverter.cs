using System;
using System.Globalization;
using System.Windows.Data;

namespace ThorCyte.ProtocolModule.Converts
{
    public class DoubleToRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return string.Format("{0}%", (int)((double)value * 100));

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
