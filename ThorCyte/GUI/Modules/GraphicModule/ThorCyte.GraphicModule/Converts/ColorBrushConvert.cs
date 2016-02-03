using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ThorCyte.GraphicModule.Converts
{
    class ColorBrushConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = (SolidColorBrush)value;
            if (brush != null)
            {
                return brush.Color;
            }
            return null;
        }
    }
}
