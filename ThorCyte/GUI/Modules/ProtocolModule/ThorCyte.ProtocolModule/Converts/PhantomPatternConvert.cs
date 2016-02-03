using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.Converts
{
    public class PhantomPatternConvert : IValueConverter
    {
        private const string LatticeString = "lattice";
        private const string RandomString = "random";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pattern = (PhantomPattern)value;
            var param = (string)parameter;
            return pattern == PhantomPattern.Lattice ? param == LatticeString : param == RandomString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            return param == LatticeString ? PhantomPattern.Lattice : PhantomPattern.Random;
        }
    }
}
