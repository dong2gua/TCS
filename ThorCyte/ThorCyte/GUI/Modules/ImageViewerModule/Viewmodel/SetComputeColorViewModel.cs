using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.Model;
using ThorCyte.ImageViewerModule.View;
using ThorCyte.Infrastructure.Types;
using System.Linq;

namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class SetComputeColorViewModel : BindableBase
    {
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand ClickOKCommand { get; private set; }
        public ICommand ClickCancelCommand { get; private set; }
        private IList<Channel> _channels;
        private IList<VirtualChannel> _virtualChannels;
        private IList<ComputeColor> _computeColors;
        private bool _isNew;
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                SetProperty<bool>(ref _isNew, value, "IsNew");
            }
        }
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
        public SetComputeColorViewModel(IList<Channel> channels, IList<VirtualChannel> virtualChannels, IList<ComputeColor> computeColors)
        {
            SelectionChangedCommand = new DelegateCommand<ComputeColorItem>(OnSelectionChanged);
            ClickOKCommand = new DelegateCommand<SetComputeColorWindow>(OnClickOK);
            ClickCancelCommand = new DelegateCommand<SetComputeColorWindow>(OnClickCancel);
            ChannelName = "color "+(computeColors.Count+1).ToString();
            if (channels == null || virtualChannels==null|| computeColors == null) return;
            _channels = channels;
            _virtualChannels = virtualChannels;
            _computeColors = computeColors;
            ChannelList = new List<ComputeColorItem>();
            foreach (var o in channels)
            {
                var item = new ComputeColorItem() { IsSelected = false, Channel = o, Color = Colors.Red };
                ChannelList.Add(item);
            }
            foreach (var o in virtualChannels)
            {
                var item = new ComputeColorItem() { IsSelected = false, Channel = o, Color = Colors.Red };
                ChannelList.Add(item);
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
            if (ChannelList.Where(x => x.IsSelected).Count() == 0) return;
            if (IsNew)
            {
                foreach (var o in _channels)
                {
                    if (o.ChannelName == _channelName)
                        return;
                }
                foreach (var o in _virtualChannels)
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
