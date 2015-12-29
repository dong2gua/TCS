using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Input;
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
        //public bool IsSelected { get; set; }
        public Color Color { get; set; }
        public ushort[] Data { get; set; }

    }
}
