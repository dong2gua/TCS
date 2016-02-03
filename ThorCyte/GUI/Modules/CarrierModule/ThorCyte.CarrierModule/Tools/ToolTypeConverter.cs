using System;
using System.Globalization;
using System.Windows.Data;

namespace ThorCyte.CarrierModule.Tools
{
    /// <summary>
    /// Convert ToolType to bool.
    /// Can be used to check active tool button/menu item
    /// in client application.
    /// ConverterParameter should be string representation 
    /// of the button tool type ("Pointer", "Rectangle" etc.)
    /// </summary>
    [ValueConversion(typeof(ToolType), typeof(bool))]
    public class ToolTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var name = Enum.GetName(typeof(ToolType), value);

            return (name == (string)parameter);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return new NotSupportedException(GetType().Name + "Properties.Settings.Default.ConvertBackNotSupported");
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToOppositeBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class DoubletoStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var var = (double)value * 100;
            var str = var + "%";
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
