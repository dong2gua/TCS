using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Types;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ChannelModVm : ModuleBase
    {
        #region Properties and Fields

        private ImageData _img;

        public override bool Executable
        {
            get { return true; }
        }

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

        #endregion

        #region Methods

        public ChannelModVm()
        { 
            ChannelNames = new ObservableCollection<string>();
            _channels = new List<Channel>();
        }

        public override void OnDeserialize(XmlReader reader)
        {
            _selectedChannel = reader["channel"];
        }

        public override void OnExecute()
        {
            try
            {
                _img = Macro.CurrentImages[SelectedChannel].Clone();
                if (_img == null)
                {
                    throw new CyteException("ChannelModVm", "Invaild execution image is null");
                }
                SetOutputImage(_img);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Channel Module error: " + ex.Message);
                throw;
            }
            
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

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("channel", SelectedChannel);

        }

        #endregion
    }
}
