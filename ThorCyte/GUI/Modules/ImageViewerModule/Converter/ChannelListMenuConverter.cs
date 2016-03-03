using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ThorCyte.ImageViewerModule.Model;

namespace ThorCyte.ImageViewerModule.Converter
{
    public class ChannelListMenuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var channel = value as ChannelImage;
            if (channel == null) return Visibility.Collapsed;
            if (channel.IsComputeColor)
                return Visibility.Visible;
            else if (channel.ChannelInfo.IsvirtualChannel)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
