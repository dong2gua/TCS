using System;
using System.Diagnostics;
using System.Xml;
using ImageProcess;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class PhantomModVm : ModuleBase
    {
        #region Properties and fields
        private ImageData _img;

        public override bool Executable
        {
            get { return true; }
        }

        public override string CaptionString { get { return _phantomName; } }

        private string _phantomName;

        public string PhantomName
        {
            get { return _phantomName; }
            set
            {
                if (value == _phantomName)
                {
                    return;
                }
                SetProperty(ref _phantomName, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private PhantomPattern _pattern;

        public PhantomPattern Pattern
        {
            get { return _pattern; }
            set
            {
                if (value == _pattern)
                {
                    return;
                }
                SetProperty(ref _pattern, value);
            }
        }

        private int _radius;

        public int Radius
        {
            get { return _radius; }
            set
            {
                if (value == _radius)
                {
                    return;
                }
                SetProperty(ref _radius, value);
            }
        }

        private int _distance;

        public int Distance
        {
            get { return _distance; }
            set
            {
                if (value == _distance)
                {
                    return;
                }
                SetProperty(ref _distance, value);
            }
        }

        private int _count;

        public int Count
        {
            get { return _count; }
            set
            {
                if (value == _count)
                {
                    return;
                }
                SetProperty(ref _count, value);
            }
        }

        #endregion

        #region Constructors

        public PhantomModVm()
        {
        }

        #endregion

        #region Methods

        public override void OnExecute()
        {
            try
            {
                _img = InputImage;

                //Todo: Add Implement logic here.

                _img.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Phantom Module error: " + ex.Message);
                throw;
            }
        }


        public override void Initialize()
        {
            View = new PhantomModule();
            ModType = ModuleType.SmtPhantomModule;
            Name = GlobalConst.PhantomModuleName;
            InputPorts[0].DataType = PortDataType.MultiChannelImage;
            InputPorts[0].ParentModule = this;
            OutputPort.DataType = PortDataType.Event;
            OutputPort.ParentModule = this;
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("phantom-name", PhantomName);
            writer.WriteAttributeString("pattern", Pattern.ToString());
            writer.WriteAttributeString("radius", Radius.ToString());
            writer.WriteAttributeString("distance",Distance.ToString());
            writer.WriteAttributeString("count", Count.ToString());
        }


        public override void OnDeserialize(XmlReader reader)
        {
            PhantomName = reader["phantom-name"];
            if (reader["pattern"] != null)
            {
                Pattern = (PhantomPattern)Enum.Parse(typeof(PhantomPattern), reader["pattern"]);
            }

            if (reader["radius"] != null)
            {
                Radius = XmlConvert.ToInt32(reader["radius"]);
            }

            if (reader["distance"] != null)
            {
                Distance = XmlConvert.ToInt32(reader["distance"]);
            }

            if (reader["count"] != null)
            {
                Count = XmlConvert.ToInt32(reader["count"]);
            }
        }
        #endregion
    }
}
