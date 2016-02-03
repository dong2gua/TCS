using System;
using System.ComponentModel;
using System.Windows.Media;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Models
{
    public class OverlayInfo
    {
        #region Events

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        #region Properties

        private static string _overlayName;

        public static string OverlayName
        {
            get { return _overlayName; }
            set
            {
                if (value == _overlayName)
                {
                    return;
                }
                _overlayName = value;
                OnPropertyChanged("OverlayName");
            }
        }

        private static ColorInfo _currentColorInfo;

        public static ColorInfo CurrentColorInfo
        {
            get { return _currentColorInfo; }
            set
            {
                if (value == _currentColorInfo)
                {
                    return;
                }
                _currentColorInfo = value;
                OnPropertyChanged("CurrentColorInfo");
            }
        }

        private static readonly ImpObservableCollection<ColorInfo> _colorList = new ImpObservableCollection<ColorInfo>()
        {
            new ColorInfo(new SolidColorBrush(Colors.Red), "Red", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.Green), "Green", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.Blue), "Blue", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.Yellow), "Yellow", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.Magenta), "Magenta", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.Cyan), "Cyan", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.Gray), "Gray", ColorType.Normal),
            new ColorInfo(new SolidColorBrush(Colors.White), ConstantHelper.CustomerStr, ColorType.Customer)
        };

        public static ImpObservableCollection<ColorInfo> ColorList
        {
            get { return _colorList; }
        }

        #endregion

        #region Constructor

        public OverlayInfo()
        {
            _currentColorInfo = _colorList[0];
        }

        #endregion

        #region Methods

        public static void Clear()
        {
            _overlayName = string.Empty;
            _currentColorInfo = _colorList[0];
            _colorList[_colorList.Count - 1].ColorBrush = new SolidColorBrush(Colors.White);
        }

        static void OnPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
