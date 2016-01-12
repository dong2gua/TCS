using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ChannelModVm : ModuleBase
    {
        #region Properties and Fields

        private ImageData _img;
        public override string CaptionString
        {
            get { return _selectedChannel; }
        }

        public ObservableCollection<string> ChannelNames { get; set; }
        private readonly List<Channel> _channels;

        private string _selectedChannel;

        public string SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                if (value == _selectedChannel)
                {
                    return;
                }
                SetProperty(ref _selectedChannel, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }


        private int MaxBits { get; set; }

        #endregion

        #region Methods

        public ChannelModVm()
        {
            ChannelNames = new ObservableCollection<string>();
            _channels = new List<Channel>();
            MaxBits = ServiceLocator.Current.GetInstance<IExperiment>().GetExperimentInfo().IntensityBits;
        }

        public override void OnDeserialize(XmlReader reader)
        {
            _selectedChannel = reader["channel"];
        }

        public override void OnExecute()
        {
            _img = GetData(SelectedChannel);
            //_dataMgr.GetTileData()
            if (_img == null)
            {
                throw new CyteException("ChannelModVm", "Invaild execution image is null");
            }

            SetOutputImage(_img);
            _img.Dispose();
            _img = null;
        }

        public override void Initialize()
        {
            HasImage = true;
            View = new ChannelMod();
            ModType = ModuleType.SmtContourCategory;
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;

            if (Macro.CurrentScanInfo != null)
            {
                ChannelNames.Clear();
                _channels.Clear();
                foreach (var channel in Macro.CurrentScanInfo.ChannelList)
                {
                    _channels.Add(channel);
                    ChannelNames.Add(channel.ChannelName);
                }

                foreach (var channel in Macro.CurrentScanInfo.VirtualChannelList)
                {
                    _channels.Add(channel);
                    ChannelNames.Add(channel.ChannelName);
                }
            }

            if (ChannelNames.Count > 0)
            {
                SelectedChannel = ChannelNames[0];
            }
        }

        private ImageData GetData(string channelName)
        {
            ImageData data;
            var channel = _channels.FirstOrDefault(ch => ch.ChannelName == channelName);
            if (channel == null)
                throw new CyteException("Protocol.ChannelVM", string.Format("No such channel: {0}", channelName));

            if (channel.IsvirtualChannel)
            {
                var dic = new Dictionary<Channel, ImageData>();
                TraverseVirtualChannel(dic, channel as VirtualChannel);
                data = GetVirtualChData(channel as VirtualChannel, dic);
            }
            else
            {
                data = getRealChData(channel);
            }
            return data;
        }

        private void TraverseVirtualChannel(Dictionary<Channel, ImageData> dic, VirtualChannel channel)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, getRealChData(channel.FirstChannel));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, getRealChData(channel.SecondChannel));
                }
            }
        }

        private ImageData getRealChData(Channel channelInfo)
        {
            return Macro.CurrentDataMgr.GetTileData(Macro.CurrentScanId, Macro.CurrentRegionId, channelInfo.ChannelId, 0, 0,
                            Macro.CurrentTileId, 0);
        }

        private ImageData GetVirtualChData(VirtualChannel channel, Dictionary<Channel, ImageData> dic)
        {

            ImageData data2 = null;
            var data1 = !channel.FirstChannel.IsvirtualChannel ? dic[channel.FirstChannel] : GetVirtualChData(channel.FirstChannel as VirtualChannel, dic);

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert)
            {
                data2 = !channel.SecondChannel.IsvirtualChannel ? dic[channel.SecondChannel] : GetVirtualChData(channel.SecondChannel as VirtualChannel, dic);
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert))) return null;
            var result = new ImageData(data1.XSize, data1.YSize);
            switch (channel.Operator)
            {
                case ImageOperator.Add:
                    result = data1.Add(data2, MaxBits);
                    break;
                case ImageOperator.Subtract:
                    result = data1.Sub(data2);
                    break;
                case ImageOperator.Invert:
                    result = data1.Invert(MaxBits);
                    break;
                case ImageOperator.Max:
                    result = data1.Max(data2);
                    break;
                case ImageOperator.Min:
                    result = data1.Min(data2);
                    break;
                case ImageOperator.Multiply:
                    result = data1.MulConstant(channel.Operand, MaxBits);
                    break;
            }
            return result;
        }

        public override void Deserialize(XmlReader reader)
        {
            base.Deserialize(reader);
        }

        #endregion
    }
}
