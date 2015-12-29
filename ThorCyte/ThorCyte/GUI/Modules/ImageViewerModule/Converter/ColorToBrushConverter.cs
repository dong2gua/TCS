using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.Converter
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color)
            {
                var color = (Color)value;
                SolidColorBrush brush = new SolidColorBrush(color);
                return brush;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
