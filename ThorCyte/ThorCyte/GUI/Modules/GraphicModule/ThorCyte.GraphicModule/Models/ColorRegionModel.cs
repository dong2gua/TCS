using System.Windows.Media;
using Prism.Mvvm;

namespace ThorCyte.GraphicModule.Models
{
    public class ColorRegionModel : BindableBase
    {
        #region Properties

        private Color _regionColor = Colors.White;

        public Color RegionColor
        {
            get { return _regionColor; }
            set
            {
                if (_regionColor == value)
                {
                    return;
                }
                SetProperty(ref _regionColor, value);
            }
        }

        private string _regionColorString;
        public string RegionColorString
        {
            get { return _regionColorString; }
            set
            {
                if (_regionColorString == value)
                {
                    return;
                }
                SetProperty(ref _regionColorString, value);
            }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value)
                {
                    return;
                }
                SetProperty(ref _isChecked, value);
            }
        }

        #endregion
    }
}
