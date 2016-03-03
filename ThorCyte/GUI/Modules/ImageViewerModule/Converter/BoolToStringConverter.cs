using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ThorCyte.ImageViewerModule.Converter
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool) || parameter == null)
                return value;
            var boolValue = (bool)value;
            var results = parameter.ToString().Split('|');
            if (results.Count() < 2)
                return value;
            if (boolValue)
                return results[0];
            else
                return results[1];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
