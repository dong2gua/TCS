using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using ComponentDataService.Types;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    internal class EventModVm : ModuleBase
    {
        #region Properties and Fields

        public const string NoneColorStr = "(None)";
        public const string CustomerColorStr = "Custom";

        public override bool Executable
        {
            get { return true; }
        }

        public override string CaptionString
        {
            get
            {
                var caption = string.Format("({0})", _expandBy);
                if (_isDynamicBackground)
                    caption += " B";

                if (_isPeripheral)
                    caption += " P";

                return caption;
            }
        }

        private int _expandBy;

        public int ExpandBy
        {
            get { return _expandBy; }
            set
            {
                if (value == _expandBy)
                {
                    return;
                }
                SetProperty(ref _expandBy, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private bool _isKeepsEventsOnBoundary;

        public bool IsKeepsEventsOnBoundary
        {
            get { return _isKeepsEventsOnBoundary; }
            set
            {
                if (value == _isKeepsEventsOnBoundary)
                {
                    return;
                }
                SetProperty(ref _isKeepsEventsOnBoundary, value);
            }
        }

        private bool _isDynamicBackground;

        public bool IsDynamicBackground
        {
            get { return _isDynamicBackground; }
            set
            {
                if (value == _isDynamicBackground)
                {
                    return;
                }
                SetProperty(ref _isDynamicBackground, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private int _bkDistance;

        public int BkDistance
        {
            get { return _bkDistance; }
            set
            {
                if (value == _bkDistance)
                {
                    return;
                }
                SetProperty(ref _bkDistance, value);
            }
        }

        private int _bkWidth;

        public int BkWidth
        {
            get { return _bkWidth; }
            set
            {
                if (value == _bkWidth)
                {
                    return;
                }
                SetProperty(ref _bkWidth, value);
            }
        }

        private int _bkLowPct;

        public int BkLowPct
        {
            get { return _bkLowPct; }
            set
            {
                if (value == _bkLowPct)
                {
                    return;
                }
                SetProperty(ref _bkLowPct, value);
            }
        }

        private int _bkHighPct;

        public int BkHighPct
        {
            get { return _bkHighPct; }
            set
            {
                if (value == _bkHighPct)
                {
                    return;
                }
                SetProperty(ref _bkHighPct, value);
            }
        }

        private bool _isPeripheral;

        public bool IsPeripheral
        {
            get { return _isPeripheral; }
            set
            {
                if (value == _isPeripheral)
                {
                    return;
                }
                SetProperty(ref _isPeripheral, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private int _periDistance;

        public int PeriDistance
        {
            get { return _periDistance; }
            set
            {
                if (value == _periDistance)
                {
                    return;
                }
                SetProperty(ref _periDistance, value);
            }
        }

        private int _periWidth;

        public int PeriWidth
        {
            get { return _periWidth; }
            set
            {
                if (value == _periWidth)
                {
                    return;
                }
                SetProperty(ref _periWidth, value);
            }
        }

        public ImpObservableCollection<ChannelsCorrection> ChannelCollection { get; set; }


        public bool[] BkCorrectionTable
        {
            get { return GetChCorr(); }
        }
        #endregion

        #region Methods

        public EventModVm()
        {
            ChannelCollection = new ImpObservableCollection<ChannelsCorrection>();
            ExpandBy = 4;
            IsDynamicBackground = true;
            IsPeripheral = false;
            BkDistance = 8;
            BkWidth = 2;
            BkHighPct = 70;
            BkLowPct = 30;
            PeriDistance = 6;
            PeriWidth = 18;
        }

        public override void OnExecute()
        {
            try
            {

                var componentName = InputPorts[0].ComponentName;
                var define = new BlobDefine
                {
                    DataExpand = ExpandBy,
                    BackgroundDistance = BkDistance,
                    BackgroundHighBoundPercent = BkHighPct,
                    BackgroundLowBoundPercent = BkLowPct,
                    BackgroundWidth = BkWidth,
                    DynamicBkCorrections = GetChCorr(),
                    IsDynamicBackground = IsDynamicBackground,
                    IsPeripheral = IsPeripheral,
                    PeripheralDistance = PeriDistance,
                    PeripheralWidth = PeriWidth
                };

                Macro.CurrentConponentService.CreateEvents(componentName, Macro.CurrentScanId, Macro.CurrentRegionId + 1,
                    Macro.CurrentTileId, Macro.CurrentImages, define);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Event Module error: " + ex.Message);
                throw;
            }
            


        }

        private bool[] GetChCorr()
        {
            return ChannelCollection.Select(chc => chc.IsChecked).ToArray();
        }

        public override void Initialize()
        {
            View = new EventModule();
            HasImage = false;
            ModType = ModuleType.SmtEventCategory;
            Name = GlobalConst.EventModuleName;
            InputPorts[0].DataType = PortDataType.Event;
            InputPorts[0].ParentModule = this;
            OutputPort.DataType = PortDataType.Event;
            OutputPort.ParentModule = this;


            ChannelCollection.Clear();
            foreach (var channel in Macro.CurrentScanInfo.ChannelList)
            {
                ChannelCollection.Add(new ChannelsCorrection(true, channel.ChannelName));
            }

            foreach (var channel in Macro.CurrentScanInfo.VirtualChannelList)
            {
                ChannelCollection.Add(new ChannelsCorrection(true, channel.ChannelName));
            }

        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("expand-by", ExpandBy.ToString());
            writer.WriteAttributeString("keep-boundary-events", IsKeepsEventsOnBoundary.ToString().ToLower());

            writer.WriteAttributeString("peripheral", IsPeripheral.ToString().ToLower());
            writer.WriteAttributeString("peri-distance", PeriDistance.ToString());
            writer.WriteAttributeString("peri-width", PeriWidth.ToString());

            writer.WriteAttributeString("dynamic-background", IsDynamicBackground.ToString().ToLower());
            writer.WriteAttributeString("distance", BkDistance.ToString());
            writer.WriteAttributeString("width", BkWidth.ToString());
            writer.WriteAttributeString("low", BkLowPct.ToString());
            writer.WriteAttributeString("high", BkHighPct.ToString());

            if (BkCorrectionTable != null)
            {
                writer.WriteStartElement("bk-correction");
                writer.WriteAttributeString("count", BkCorrectionTable.Length.ToString());
                var i = 0;
                foreach (var corrCh in ChannelCollection)
                {
                    if (corrCh.ChannelName != null)
                    {
                        writer.WriteStartElement("channel");
                        writer.WriteAttributeString("index", i.ToString());
                        writer.WriteAttributeString("label", corrCh.ChannelName);
                        writer.WriteAttributeString("correct", corrCh.IsChecked.ToString().ToLower());
                        writer.WriteEndElement();
                    }
                    i++;
                }
                writer.WriteEndElement();
            }
        }

        public override void OnDeserialize(XmlReader reader)
        {
            if (reader["expand-by"] != null)
            {
                ExpandBy = Convert.ToInt32(reader["expand-by"]);
            }

            if (reader["keep-boundary-events"] != null)
            {
                IsKeepsEventsOnBoundary = Convert.ToBoolean(reader["keep-boundary-events"]);
            }

            if (reader["dynamic-background"] != null)
            {
                IsDynamicBackground = Convert.ToBoolean(reader["dynamic-background"]);
            }

            if (reader["distance"] != null)
            {
                BkDistance = Convert.ToInt32(reader["distance"]);
            }
            if (reader["width"] != null)
            {
                BkWidth = Convert.ToInt32(reader["width"]);
            }

            if (reader["low"] != null)
            {
                BkLowPct = Convert.ToInt32(reader["low"]);
            }

            if (reader["high"] != null)
            {
                BkHighPct = Convert.ToInt32(reader["high"]);
            }

            if (reader["peripheral"] != null)
            {
                IsPeripheral = Convert.ToBoolean(reader["peripheral"]);
            }

            if (reader["peri-distance"] != null)
            {
                PeriDistance = Convert.ToInt32(reader["peri-distance"]);
            }

            if (reader["peri-width"] != null)
            {
                PeriWidth = Convert.ToInt32(reader["peri-width"]);
            }

            if (reader.IsEmptyElement) return;
            ChannelCollection.Clear();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "channel":
                            var index = Convert.ToInt32(reader["index"]);
                            ChannelCollection.Add(new ChannelsCorrection(Convert.ToBoolean(reader["correct"]),reader["label"]));
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "module")
                    return;
            }

        }

        #endregion
    }

    public class ChannelsCorrection : BindableBase
    {
        private bool _ischecked;
        public bool IsChecked
        {
            get
            {
                return _ischecked;
            }
            set { SetProperty(ref _ischecked, value); }
        }

        private string _channelname;
        public string ChannelName
        {
            get { return _channelname; }
            set { SetProperty(ref _channelname, value); }
        }

        public ChannelsCorrection()
        {
            IsChecked = false;
            ChannelName = string.Empty;
        }

        public ChannelsCorrection(bool isChecked, string channelname)
        {
            IsChecked = isChecked;
            ChannelName = channelname;
        }
    }
}
