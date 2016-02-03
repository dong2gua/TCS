using System.Windows;
using System.Windows.Controls;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Models;

namespace ThorCyte.GraphicModule.Controls
{
    class ComColor : ContentControl
    {
        #region Properties

        public bool IsColorPickerEnabled
        {
            get { return (bool)GetValue(IsColorPickerEnabledProperty); }
            set { SetValue(IsColorPickerEnabledProperty, value); }
        }

        public ImpObservableCollection<ColorInfo> ColorList
        {
            get { return (ImpObservableCollection<ColorInfo>)GetValue(ColorListProperty); }
            set { SetValue(ColorListProperty, value); }
        }

        public ColorInfo SelectedColor
        {
            get { return (ColorInfo)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty IsColorPickerEnabledProperty =
            DependencyProperty.Register("IsColorPickerEnabled", typeof(bool), typeof(ComColor), new UIPropertyMetadata(false));

        public static readonly DependencyProperty ColorListProperty =
            DependencyProperty.Register("ColorList", typeof(ImpObservableCollection<ColorInfo>), typeof(ComColor));

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(ColorInfo), typeof(ComColor));

        #endregion
    }
}
