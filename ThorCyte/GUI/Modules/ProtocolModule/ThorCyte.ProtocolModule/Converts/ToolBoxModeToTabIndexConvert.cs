using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.ProtocolModule.ViewModels;

namespace ThorCyte.ProtocolModule.Converts
{
    public class ToolBoxModeToTabIndexConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var method = (ToolBoxMode)value;
            switch (method)
            {
                case ToolBoxMode.Moduletree:
                    return 0;
                case ToolBoxMode.Setting:
                    return 1;
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return ToolBoxMode.Moduletree;
            }

            var param = (int)parameter;
            switch (param)
            {
                case 0:
                    return ToolBoxMode.Moduletree;
                case 1:
                    return ToolBoxMode.Setting;
                default:
                    return null;
            }
        }
    }
}
