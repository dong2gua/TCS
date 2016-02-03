using System;
using System.Windows.Media;
using Prism.Mvvm;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Models
{
    public class ColorInfo : BindableBase, ICloneable
    {
        #region Properties

        private string _colorName;

        public string ColorName
        {
            get { return _colorName; }
            set
            {
                if (_colorName == value)
                {
                    return;
                }
                SetProperty(ref _colorName, value);
            }
        }

        private SolidColorBrush _colorBrush;

        public SolidColorBrush ColorBrush
        {
            get { return _colorBrush; }
            set
            {
                if (Equals(_colorBrush, value))
                {
                    return;
                }
                SetProperty(ref _colorBrush, value);
            }
        }

        private ColorType _type;

        public ColorType Type
        {
            get { return _type; }
            set
            {
                if (value == _type)
                {
                    return;
                }
                SetProperty(ref _type, value);
            }
        }

        public ColorInfo()
        {
            _colorBrush = new SolidColorBrush();
            _colorName = "None";
            _type = ColorType.None;
        }

        public ColorInfo(SolidColorBrush color, string name, ColorType type)
        {
            _colorBrush = color;
            _colorName = name;
            _type = type;
        }

        #endregion

        public object Clone()
        {
            var brush = new SolidColorBrush(_colorBrush.Color);
            return new ColorInfo(brush, _colorName, _type);
        }
    }
}
