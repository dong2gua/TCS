using System;
using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ThresholdModVm : ModuleBase
    {
        #region Properties and fields
        private ImageData _img;

        public override string CaptionString { get { return string.Format("{0} ({1})", Method, Threshold); } }

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
            }
        }

        private int _minThreshold;

        public int MinThreshold
        {
            get { return _minThreshold; }
            set
            {
                if (value == _minThreshold)
                {
                    return;
                }
                SetProperty(ref _minThreshold, value);
            }
        }

        private int _maxThreshold;

        public int MaxThreshold
        {
            get { return _maxThreshold; }
            set
            {
                if (value == _maxThreshold)
                {
                    return;
                }
                SetProperty(ref _maxThreshold, value);
            }
        }

        private ushort _threshold;

        public ushort Threshold
        {
            get { return _threshold; }
            set
            {
                if (value == _threshold)
                {
                    return;
                }
                SetProperty(ref _threshold, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private bool _isManual;

        public bool IsManual
        {
            get { return _isManual; }
            set
            {
                if (value == _isManual)
                {
                    return;
                }
                SetProperty(ref _isManual, value);
            }
        }

        private bool _isOtsu;

        public bool IsOtsu
        {
            get { return _isOtsu; }
            set
            {
                if (value == _isOtsu)
                {
                    return;
                }
                SetProperty(ref _isOtsu, value);
            }
        }

        private bool _isRecalculateEnabled;

        public bool IsRecalculateEnabled
        {
            get { return _isRecalculateEnabled; }
            set
            {
                if (value == _isRecalculateEnabled)
                {
                    return;
                }
                SetProperty(ref _isRecalculateEnabled, value);
            }
        }

        private ThresholdMethod _method;

        public ThresholdMethod Method
        {
            get { return _method; }
            set
            {
                if (value == _method)
                {
                    return;
                }
                SetProperty(ref _method, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private bool _isStatisticalVisible;

        public bool IsStatisticalVisible
        {
            get { return _isStatisticalVisible; }
            set
            {
                if (value == _isStatisticalVisible)
                {
                    return;
                }
                SetProperty(ref _isStatisticalVisible, value);
            }
        }

        private bool _isWarningVisible;

        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set
            {
                if (value == _isWarningVisible)
                {
                    return;
                }
                SetProperty(ref _isWarningVisible, value);
            }
        }

        #endregion

        #region Constructors

        #endregion

        #region Methods

        public override void Initialize()
        {
            HasImage = true;
            View = new ThresholdModule();
            ModType = ModuleType.SmtContourCategory;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
            OutputPort.DataType = PortDataType.BinaryImage;
            OutputPort.ParentModule = this;
            Method = ThresholdMethod.Otsu;
        }

        public override void OnExecute()
        {
            _img = InputImage;

            var thType = ThresholdType.Auto;
            switch (Method)
            {
                case ThresholdMethod.Manual:
                    thType = ThresholdType.Manual;
                    break;
            }

            var processedImg = _img.Threshold(Threshold, thType);

            _img.Dispose();

            if (processedImg == null)
            {
                throw new CyteException("ThresholdModVm", "Invaild execution image is null");
            }
            SetOutputImage(processedImg);
        }

        public override void OnDeserialize(XmlReader reader)
        {
            var method = reader["method"]; // jcl-7579

            if (method != null)
            {
                Method = (ThresholdMethod)Enum.Parse(typeof(ThresholdMethod), method);
            }

            if (reader["threshold"] != null)
            {
                Threshold = XmlConvert.ToUInt16(reader["threshold"]);
            }

            if (reader["recalculate"] != null)
            {
                IsRecalculateEnabled = XmlConvert.ToBoolean(reader["recalculate"]);
            }
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("method", Method.ToString());
            writer.WriteAttributeString("threshold", Threshold.ToString());
        }

        #endregion
    }
}
