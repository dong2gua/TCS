using System;
using System.Windows;
using System.Windows.Data;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Converter
{
    public class OperatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ImageOperator)
            {
                var operation = (ImageOperator)value;
                var p = parameter.ToString();
                switch(parameter.ToString())
                {
                    case "0":
                        if (operation == ImageOperator.Multiply|| operation == ImageOperator.ShiftPeak)
                            return Visibility.Collapsed;
                        else return Visibility.Visible;
                    case "1":
                        if (operation == ImageOperator.Multiply || operation == ImageOperator.ShiftPeak)
                            return Visibility.Visible;
                        else return Visibility.Collapsed;
                    case "2":
                        if (operation == ImageOperator.Invert)
                            return false;
                        else return true;
                    default:
                        return null;
                }
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
