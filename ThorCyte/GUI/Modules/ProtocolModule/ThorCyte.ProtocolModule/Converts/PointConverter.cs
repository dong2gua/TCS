using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThorCyte.ProtocolModule.Converts
{
    public class PointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return null;


            switch (parameter.ToString().Trim().ToUpper())
            {
                case "X":
                    return ((Point)value).X;
                case "Y":
                    return ((Point)value).Y;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
