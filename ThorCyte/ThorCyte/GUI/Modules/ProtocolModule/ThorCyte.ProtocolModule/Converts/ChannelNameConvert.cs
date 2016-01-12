using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ProtocolModule.Converts
{
    class ChannelNameConvert :  IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var channel = (Channel)value;
            return channel.ChannelName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
