using System.Windows.Media;

namespace ThorCyte.GraphicModule.Helper
{
    public static class ConstantHelper
    {

        public static readonly Brush[] BrushTable =
        {
           Brushes.Red,Brushes.LawnGreen,Brushes.Orange,Brushes.Yellow,Brushes.Magenta,Brushes.Cyan,Brushes.White,Brushes.Black
        };

        public static readonly Color[] ColorTable = 
		{
			Colors.Red, Colors.LawnGreen, Colors.Orange, Colors.Yellow, Colors.Magenta, Colors.Cyan, Colors.White
		};

        public static readonly Color[] TemperatureColors = 
		{
			Colors.Navy, Colors.Blue, Colors.Cyan, Colors.Green, Colors.GreenYellow, Colors.Yellow, Colors.Orange, Colors.Red
		};

        public const int ColorCount = 6;

        public const int HighResolution = 200;

        public const int LowBinCount = 200;

        public const int DefaultHistogramYScale = 10;

        public const double DefaultCoordinateWidth = 200;

        public const string PrefixScattergramName = "S";

        public const string PrefixHistogramName = "H";

        public const string PrefixRegionName = "R";

        public const string DensityString = "Density";

        public const string HistogramYTitle = "Count";

        public const string CustomerStr = "Custom";

        public const string DefaultColorStr = "Default";

        public const string DefaultTabName = "Tab";

        public const string AxisMaxTextFormat = "#.#E+0";

        public const string AxisMinTextFormat = "0.00";

        public const string GraphicXmlPath = "graphic.xml";
    }
}
