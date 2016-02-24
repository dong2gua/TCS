using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;
using ThorCyte.ImageViewerModule.View;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class SetVirtualChannelViewModel : BindableBase
    {
        public ICommand ClickOKCommand { get; private set; }
        public ICommand ClickCancelCommand { get; private set; }
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
        private IList<Channel> _channelList;
        public IList<Channel> ChannelList
        {
            get { return _channelList; }
            set { SetProperty<IList<Channel>>(ref _channelList, value, "ChannelList"); }
        }

        private Channel _channel1;
        public Channel Channel1
        {
            get { return _channel1; }
            set { SetProperty<Channel>(ref _channel1, value, "Channel1"); }
        }
        private Channel _channel2;
        public Channel Channel2
        {
            get { return _channel2; }
            set { SetProperty<Channel>(ref _channel2, value, "Channel2"); }
        }
        private double _operand;
        public double Operand
        {
            get { return _operand; }
            set
            {
                SetProperty<double>(ref _operand, value, "Operand");
            }
        }
        private ImageOperator _operator;
        public ImageOperator Operator
        {
            get { return _operator; }
            set
            {
                SetProperty<ImageOperator>(ref _operator, value, "Operator");
            }
        }
        public SetVirtualChannelViewModel(IList<Channel> channels, IList<VirtualChannel> virtualChannels, IList<ComputeColor> computeColors)
        {
            ClickOKCommand = new DelegateCommand<SetVirtualChannelWindow>(OnClickOK);
            ClickCancelCommand = new DelegateCommand<SetVirtualChannelWindow>(OnClickCancel);
            if (channels == null || virtualChannels == null) return;
            ChannelList = new List<Channel>();
            foreach (var o in channels)
                ChannelList.Add(o);
            foreach (var o in virtualChannels)
                ChannelList.Add(o);
            _computeColors = computeColors;
            IsNew = true;
        }
        public void SetCurrentChannel(VirtualChannel channel)
        {
            IsNew = false;
            ChannelName = channel.ChannelName;
            Channel1 = channel.FirstChannel;
            Channel2 = channel.SecondChannel;
            Operator = channel.Operator;
            Operand = channel.Operand;
            int i = ChannelList.Count - 1;
            while (i > 0)
            {
                if (!ChannelList[i].IsvirtualChannel) break;
                if (ChannelList[i] == channel)
                {
                    ChannelList.RemoveAt(i);
                    break;
                }
                ChannelList.RemoveAt(i);
                i--;
            }
        }
        private void OnClickOK(SetVirtualChannelWindow window)
        {
            if (Channel1 == null || (Channel2 == null && (Operator != ImageOperator.Multiply && Operator != ImageOperator.Invert && Operator != ImageOperator.ShiftPeak)))
                return;
            if (IsNew)
            {
                foreach (var o in _channelList)
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
        private void OnClickCancel(SetVirtualChannelWindow window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
