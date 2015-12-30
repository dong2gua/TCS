using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.ChannelMod;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ChannelModVm : ModuleVmBase
    {
        #region Properties and Fields

        private ImageData _img;

        public override string CaptionString
        {
            get { return _chaModel != null ? _selectedChannel : string.Empty; }
        }

        private ChannelModel _chaModel = new ChannelModel();

        public ChannelModel ChaModel
        {
            get { return _chaModel; }
            set { SetProperty(ref _chaModel, value); }
        }

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

        #endregion

        #region Methods

        public override void OnDeserialize(XmlReader reader)
        {
            _selectedChannel = reader["channel"];
        }

        public override void OnExecute()
        {            
            if (_img == null)
            {
                throw new CyteException("ChannelModVm","Invaild execution image is null");
            }

            SetOutputImage(_img);
            _img = null;
        }

        public override void Initialize()
        {
            _hasImage = true;
            View = new ChannelMod();
            ModType = ModuleType.SmtContourChannel;
            Name = GlobalConst.ChannelName;
            InputPorts[0].DataType = PortDataType.MultiChannelImage;
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].ParentModule = this;

            if (_chaModel.Channels.Count > 0)
            {
                _selectedChannel = _chaModel.Channels[0];
            }
        }

        public override void Deserialize(XmlReader reader)
        {
            base.Deserialize(reader);
            foreach (var channel in ProtocolModule.Instance.CurrentScanInfo.ChannelList)
            {
                _chaModel.Channels.Add(channel.ChannelName);
            }
        }

        #endregion
    }
}
