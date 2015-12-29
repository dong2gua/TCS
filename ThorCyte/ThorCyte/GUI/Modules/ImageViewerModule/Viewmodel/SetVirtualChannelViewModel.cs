using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Types;
using Prism.Mvvm;
using ThorCyte.ImageViewerModule.Model;
using System.Windows.Input;
using ThorCyte.ImageViewerModule.View;
using Prism.Commands;
namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class SetVirtualChannelViewModel:BindableBase
    {
        public SetVirtualChannelViewModel(IList<Channel> channels,IList<ComputeColor> computeColors)
        {
            ClickOKCommand = new DelegateCommand<SetVirtualChannelWindow>(OnClickOK);
            ClickCancelCommand = new DelegateCommand<SetVirtualChannelWindow>(OnClickCancel);
            ChannelList = channels;
            ChannelName = "newvirtualchannel";
            _computeColors = computeColors;
        }
        private IList<ComputeColor> _computeColors;
        public ICommand ClickOKCommand { get; private set; }
        public ICommand ClickCancelCommand { get; private set; }
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
        private void OnClickOK(SetVirtualChannelWindow window)
        {
            if (IsNew)
            { 
                foreach (var o in _channelList)
                {
                    if (o.ChannelName == _channelName)
                        return;
                }
                foreach(var o in _computeColors)
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
