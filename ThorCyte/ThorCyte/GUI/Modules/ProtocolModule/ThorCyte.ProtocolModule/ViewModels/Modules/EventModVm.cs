﻿using System;
using System.Xml;
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


        #endregion

        #region Methods

        public override void OnExecute()
        {
            base.OnExecute();

        }


        public override void Initialize()
        {
            View = new EventModule();
            ModType = ModuleType.SmtEventCategory;
            Name = GlobalConst.EventModuleName;
            InputPorts[0].DataType = PortDataType.Event;
            InputPorts[0].ParentModule = this;
            OutputPort.DataType = PortDataType.Event;
            OutputPort.ParentModule = this;
        }

        public override void OnSerialize(XmlWriter writer)
        {
            base.OnSerialize(writer);

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
        }

        #endregion
    }
}
