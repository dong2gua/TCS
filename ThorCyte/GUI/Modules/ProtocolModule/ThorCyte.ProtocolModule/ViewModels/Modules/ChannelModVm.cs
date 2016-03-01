using System;
using System.Collections.ObjectModel;
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

        public override bool Executable
        {
            get { return true; }
        }

        public override string CaptionString
        {
            get { return _selectedChannel; }
        }

        public ObservableCollection<string> ChannelNames { get; set; }

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
                foreach (var channel in Macro.CurrentScanInfo.ChannelList)
                {
                    ChannelNames.Add(channel.ChannelName);
                }

                foreach (var channel in Macro.CurrentScanInfo.VirtualChannelList)
                {
                    ChannelNames.Add(channel.ChannelName);
                }
            }

            if (ChannelNames.Count > 0)
            {
                SelectedChannel = ChannelNames[0];
            }
        }

        public override void UpdateChannels()
        {
            if (Macro.CurrentScanInfo != null)
            {
                foreach (var channel in Macro.CurrentScanInfo.ChannelList)
                {
                    if (!ChannelNames.Contains(channel.ChannelName))
                    {
                        ChannelNames.Add(channel.ChannelName);
                    }
                }

                foreach (var channel in Macro.CurrentScanInfo.VirtualChannelList)
                {
                    if (!ChannelNames.Contains(channel.ChannelName))
                    {
                        ChannelNames.Add(channel.ChannelName);
                    }
                }
            }
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("channel", SelectedChannel);
        }

        public override object Clone()
        {
            var mod = new ChannelModVm();
            //===============Common======================
            mod.Name = Name;
            mod.Id = GetNextModId();
            mod.DisplayName = DisplayName;
            mod.ScanNo = ScanNo;
            mod.Enabled = Enabled;
            mod.X = X;
            mod.Y = Y;

            //===============Channel=====================
            mod.HasImage = HasImage;
            mod.View = new ChannelMod();
            mod.ModType = ModType;
            mod.OutputPort.DataType = OutputPort.DataType;
            mod.OutputPort.ParentModule = mod;
            mod.ChannelNames.Clear();
            foreach (var ch in ChannelNames)
            {
                mod.ChannelNames.Add(ch);
            }
            mod.SelectedChannel = SelectedChannel;

            return mod;
        }
        #endregion
    }
}
