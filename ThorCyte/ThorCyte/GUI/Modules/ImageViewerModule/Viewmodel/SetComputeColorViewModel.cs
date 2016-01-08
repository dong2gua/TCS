using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Types;
using Prism.Mvvm;
using ThorCyte.ImageViewerModule.Model;
using System.Windows.Media;
using Prism.Commands;
using System.Windows.Input;
using ThorCyte.ImageViewerModule.View;
namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class SetComputeColorViewModel : BindableBase
    {
        public SetComputeColorViewModel(IList<Channel> channels, IList<ComputeColor> computeColors)
        {
            SelectionChangedCommand = new DelegateCommand<ComputeColorItem>(OnSelectionChanged);
            ClickOKCommand = new DelegateCommand<SetComputeColorWindow>(OnClickOK);
            ClickCancelCommand = new DelegateCommand<SetComputeColorWindow>(OnClickCancel);
            ChannelName = "newcomputecolor";
            if (channels == null || computeColors == null) return;
            _channels = channels;
            _computeColors = computeColors;
            ChannelList = new List<ComputeColorItem>();
            foreach(var o in channels)
            {
                var item = new ComputeColorItem() { IsSelected = false, Channel = o, Color = Colors.Red };
                ChannelList.Add(item);
            }
        }
        private bool _isNew;
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                SetProperty<bool>(ref _isNew, value, "IsNew");
            }
        }

        private IList<Channel> _channels;
        private IList<ComputeColor> _computeColors;
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand ClickOKCommand { get; private set; }
        public ICommand ClickCancelCommand { get; private set; }

        private string _channelName;
        public string ChannelName
        {
            get { return _channelName; }
            set { SetProperty<string>(ref _channelName, value, "ChannelName"); }
        }
        private IList<ComputeColorItem> _channelList;
        public IList<ComputeColorItem> ChannelList
        {
            get { return _channelList; }
            set { SetProperty<IList<ComputeColorItem>>(ref _channelList, value, "ChannelList"); }
        }

        private bool _isCheckedAll;
        public bool IsCheckedAll
        {
            get { return _isCheckedAll; }
            set
            {
                foreach (var o in ChannelList)
                {
                    o.IsSelected = value;
                }
                SetProperty<bool>(ref _isCheckedAll, value, "IsCheckedAll");
            }
        }
        private void OnSelectionChanged(ComputeColorItem item)
        {
            item.IsSelected = !item.IsSelected;
            _isCheckedAll = true;
            foreach (var o in ChannelList)
            {
                if (o.IsSelected == false)
                    _isCheckedAll = false;
            }
            OnPropertyChanged("IsCheckedAll");
        }
        private void OnClickOK(SetComputeColorWindow window)
        {
            if (IsNew)
            {
                foreach (var o in _channels)
                {
                    if (o.ChannelName == _channelName)
                        return;
                }
                foreach (var o in _computeColors)
                {
                    if (o.Name == _channelName)
                        return;
                }
            }
            window.DialogResult = true;
            window.Close();
        }
        private void OnClickCancel(SetComputeColorWindow window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
