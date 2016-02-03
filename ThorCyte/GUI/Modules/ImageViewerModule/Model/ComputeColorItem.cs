using Prism.Mvvm;
using System.Windows.Media;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Model
{
    public class ComputeColorItem:BindableBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty<bool>(ref _isSelected, value, "IsSelected"); }
        }
        public Channel Channel { get; set; }
        public Color Color { get; set; }
    }
}
