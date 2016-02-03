using System;
using System.Globalization;
using System.Windows.Data;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Converts
{
    class GraphTypeConvert:IValueConverter
    {
        private const string BarStyle = "barchart";
        private const string OutlineStyle = "outline";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var graphType = (GraphStyle)value;
            var param = (string)parameter;
            if (param == OutlineStyle)
            {
                return graphType == GraphStyle.Outline;
            }
           
            if (param == BarStyle)
            {
                return graphType == GraphStyle.BarChart;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            var isChecked = (bool)value;

            if (isChecked)
            {
                if (param == OutlineStyle)
                {
                    return GraphStyle.Outline;
                }

                if (param == BarStyle)
                {
                    return GraphStyle.BarChart;
                }
            }
            else
            {
                if (param == OutlineStyle)
                {
                    return GraphStyle.BarChart;
                }

                if (param == BarStyle)
                {
                    return GraphStyle.Outline;
                }
            }

            return GraphStyle.BarChart;
        }
    }
}
