using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Converts
{
    class RegionColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorModel = (ColorRegionModel)value;
            if (colorModel != null)
            {
                return colorModel.RegionColor;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            if (color == Colors.Black || color == Colors.White)
            {
                return GraphicVmBase.ColorRegionList[0];
            }
            else
            {
                foreach (var colormodel in GraphicVmBase.ColorRegionList)
                {
                    if (colormodel.RegionColor == color)
                    {
                        return colormodel;
                    }
                }
                return value;
            }
        }
    }
}
