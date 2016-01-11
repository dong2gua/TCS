using System.Diagnostics;
using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        #endregion

        #region Methods

        public ChannelModVm()
        {
        }

        public override void OnDeserialize(XmlReader reader)
        {
            _selectedChannel = reader["channel"];
        }

        public override void OnExecute()
        {            
            _img = Macro.CurrentDataMgr.GetTileData(InputPorts[0].ScanId, InputPorts[0].RegionId, SelectedIndex, 0, 0,
                InputPorts[0].TileId, 0);
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

            if (_chaModel.Channels.Count > 0)
            {
                SelectedChannel = _chaModel.Channels[0];
            }


            if (Macro.CurrentScanInfo != null)
            {
                foreach (var channel in Macro.CurrentScanInfo.ChannelList)
                {
                    _chaModel.Channels.Add(channel.ChannelName);
                }
            }
        }

        public override void Deserialize(XmlReader reader)
        {
            base.Deserialize(reader);
        }

        #endregion
    }
}
