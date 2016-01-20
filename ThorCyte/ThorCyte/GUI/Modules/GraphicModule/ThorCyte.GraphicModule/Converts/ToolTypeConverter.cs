using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Converts
{
    public class ToolTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,object parameter, CultureInfo culture)
        {
            var name = Enum.GetName(typeof(ToolType), value);
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return string.Equals(name.ToLower(), (string)parameter);
        }

        public object ConvertBack(object value, Type targetType,object parameter, CultureInfo culture)
        {
            var isChecked = (bool)value;
            var param = (string)parameter;
            if (isChecked)
            {
                return Enum.Parse(typeof (ToolType), param,true);
            }
            return ToolType.Pointer;
        }
    }
}
