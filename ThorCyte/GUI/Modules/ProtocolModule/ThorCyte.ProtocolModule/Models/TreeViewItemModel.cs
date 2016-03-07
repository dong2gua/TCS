using System.Collections.Generic;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Models
{
    public class TreeViewItemModel : BindableBase
    {
        #region Properties and Fields

        public ICommand RemoveItemCommand { get; private set; }

        public TreeViewItemModel()
        {
            RemoveItemCommand = new DelegateCommand(RemoveItem);
            Items = new List<TreeViewItemModel>();
        }

        public string Category { get; set; }

        public string Name { get; set; }

        public ModuleType ItemType { get; set; }

        public List<TreeViewItemModel> Items { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled;}
            set { SetProperty(ref _isEnabled, value); }
        }

        private void RemoveItem()
        {
            MessageHelper.SetRemoveTreeModule(this);
        }
        #endregion
    }
}
