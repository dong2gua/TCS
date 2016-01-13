using System;
using System.Xml;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ContourModVm : ModuleBase
    {
        #region Properties and Fields

        public override string CaptionString { get { return ComponentName; } }

        private string _componentName;

        public string ComponentName
        {
            get { return _componentName; }
            set
            {
                if (value == _componentName)
                {
                    return;
                }
                SetProperty(ref _componentName, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private bool _isMaxAreaChecked;
        public bool IsMaxAreaChecked
        {
            get { return _isMaxAreaChecked; }
            set
            {
                if (value == _isMaxAreaChecked)
                {
                    return;
                }
                SetProperty(ref _isMaxAreaChecked, value);
            }
        }

        private bool _isConcaveChecked;

        public bool IsConcaveChecked
        {
            get { return _isConcaveChecked; }
            set
            {
                if (value == _isConcaveChecked)
                {
                    return;
                }
                SetProperty(ref _isConcaveChecked, value);
            }
        }

        private bool _isBoundaryChecked;

        public bool IsBoundaryChecked
        {
            get { return _isBoundaryChecked; }
            set
            {
                if (value == _isBoundaryChecked)
                {
                    return;
                }
                SetProperty(ref _isBoundaryChecked, value);
            }
        }

        private UnitType _maxAreaUnit;

        public UnitType MaxAreaUnit
        {
            get { return _maxAreaUnit; }
            set
            {
                if (value == _maxAreaUnit)
                {
                    return;
                }
                SetProperty(ref _maxAreaUnit, value);
            }
        }

        private UnitType _minAreaUnit;

        public UnitType MinAreaUnit
        {
            get { return _minAreaUnit; }
            set
            {
                if (value == _minAreaUnit)
                {
                    return;
                }
                SetProperty(ref _minAreaUnit, value);
            }
        }

        private double _minArea;

        public double MinArea
        {
            get { return _minArea; }
            set
            {
                if (Math.Abs(value - _minArea) < double.Epsilon)
                {
                    return;
                }
                SetProperty(ref _minArea, value);
            }
        }
        private double _maxArea;

        public double MaxArea
        {
            get { return _maxArea; }
            set
            {
                if (Math.Abs(value - _maxArea) < double.Epsilon)
                {
                    return;
                }
                SetProperty(ref _maxArea, value);
            }
        }
        #endregion

        #region Constructor

        public ContourModVm()
        {
            IsMaxAreaChecked = true;
        }

        #endregion

        #region Methods

        public override void Initialize()
        {
            View = new ContourModule();
            ModType = ModuleType.SmtContourCategory;
            Name = GlobalConst.ContourModuleName;
            InputPorts[0].DataType = PortDataType.BinaryImage;
            InputPorts[0].ParentModule = this;
            OutputPort.DataType = PortDataType.Event;
            OutputPort.ParentModule = this;
        }

        public override void OnExecute()
        {
            base.OnExecute();
        }

        public override void OnSerialize(XmlWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(XmlReader reader)
        {
            ComponentName = reader["component"];
            //ContourColor = Global.ReadColor(reader);
            if (reader["min-area"] != null)
            {
                MinArea = XmlConvert.ToDouble(reader["min-area"]);
            }

            if (reader["max-area"] != null)
            {
                MaxArea = Convert.ToDouble(reader["max-area"]);
            }

            if (reader["check-max-area"] != null)
            {
                IsMaxAreaChecked = XmlConvert.ToBoolean(reader["check-max-area"]);
            }

            if (reader["concave"] != null)
            {
                IsConcaveChecked = XmlConvert.ToBoolean(reader["concave"]);
            }

            if (reader["cross-boundary"] != null)
            {
                IsBoundaryChecked = XmlConvert.ToBoolean(reader["cross-boundary"]);
            }

            if (reader["max-unit"] != null)
            {
                MaxAreaUnit = (UnitType)Enum.Parse(typeof(UnitType), reader["max-unit"]);
            }

            if (reader["unit"] != null)
            {
                MinAreaUnit = (UnitType)Enum.Parse(typeof(UnitType), reader["unit"]);
            }
        }
        #endregion
    }
}
