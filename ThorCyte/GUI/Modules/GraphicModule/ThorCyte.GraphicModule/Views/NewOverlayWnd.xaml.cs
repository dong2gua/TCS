using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.ViewModels;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for NewOverlayWnd.xaml
    /// </summary>
    public partial class NewOverlayWnd 
    {

        #region Fields

        private HistogramVm _histogramVm;
        private bool _isEdit;
        private string _originalName;

        #endregion

        #region Constructor

        public NewOverlayWnd(HistogramVm vm,string oringinalName = "",bool isEdit = false)
        {
            InitializeComponent();
            _histogramVm = vm;
            _originalName = oringinalName;
            _isEdit = isEdit;
        }

        #endregion

        #region Events

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null)
            {
                return;
            }
            var name = tb.Text.TrimStart(' ').TrimEnd(' ');
            if (IsLoaded)
            {
                OkBtn.IsEnabled = name.Length > 0;
            }
            e.Handled = true;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
            {
                return;
            }
            var name = OverlayInfo.OverlayName;
            var colorInfo = (ColorInfo)OverlayInfo.CurrentColorInfo.Clone();
            Close();
            if (_histogramVm != null)
            {
                if (!_isEdit)
                {
                    if (CheckForDuplicates(name, colorInfo.ColorBrush.Color))
                    {
                        OverlayInfo.Clear();
                        return;
                    }
                    _histogramVm.CreateOverlay(name, colorInfo);
                }
                else
                {
                    _histogramVm.EditOverlay(_originalName,name,colorInfo);
                }
            }
            OverlayInfo.Clear();
        }

        private void WndLoaded(object sender, RoutedEventArgs e)
        {
            var name = TbName.Text.TrimStart(' ').TrimEnd(' ');
            OkBtn.IsEnabled = name.Length > 0;
            e.Handled = true;
        }

        #endregion

        #region Methods

        private bool CheckForDuplicates(string overlayName,Color color)
        {
            foreach (var overlay in _histogramVm.OverlayList)
            {
                if (overlay.Name.ToLower().Equals(overlayName.ToLower()))
                {
                    MessageBox.Show(this, "The name you have chosen (" + overlayName + ") already exists.", "Message");
                    return true;
                }
                if (overlay.OverlayColorInfo.ColorBrush.Color.Equals(color))
                {
                    MessageBox.Show(this, "The color you have chosen (" + overlay.OverlayColorInfo.ColorBrush.Color + ") is already used by " + overlay.Name, "Message");
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
