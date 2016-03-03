using System.Windows.Media;
using Prism.Mvvm;

namespace ThorCyte.GraphicModule.Models
{
    public class OverlayInfo:BindableBase
    {
        #region Properties

        private string _overlayName = string.Empty;

        public string OverlayName
        {
            get { return _overlayName; }
            set
            {
                if (value == _overlayName)
                {
                    return;
                }
                SetProperty(ref _overlayName, value);
            }
        }

        private Color _overlayColor;

        public Color OverlayColor
        {
            get { return _overlayColor; }
            set
            {
                if (value == _overlayColor)
                {
                    return;
                }
                SetProperty(ref _overlayColor, value);
            }
        }

        #endregion
    }
}
