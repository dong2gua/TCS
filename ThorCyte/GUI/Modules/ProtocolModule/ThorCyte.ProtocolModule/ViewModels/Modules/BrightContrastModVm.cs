using System;
using System.Diagnostics;
using System.Windows;
using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class BrightContrastModVm : ModuleBase
    {
        private const double FactorB = 1/163.83;
        
        public override bool Executable
        {
            get { return true; }
        }

        public BrightContrastModVm()
        {
            Bright = 0;
            Contrast = 1;
            DisplaySize = 100;
            P1 = TransformPointCornidate(new Point(0.0, 0.0));
            P2 = TransformPointCornidate(new Point(Convert.ToDouble(DisplaySize), Convert.ToDouble(DisplaySize)));
            ContrastAngle = 45;
        }

        public override string CaptionString
        {
            get
            {
                return string.Format("B{0} C{1}", _bright, _contrast);
            }
        }

        private int _bright;
        public int Bright
        {
            get { return _bright; }
            set
            {
                if (_bright == value)
                {
                    return;
                }
                SetProperty(ref _bright, value);
                OnPropertyChanged("CaptionString");
                UpdateDisplayLine();
            }
        }

        private double _contrast;
        public double Contrast
        {
            get { return _contrast; }
            set
            {
                var rVal = Math.Round(value, 2);
                if (Math.Abs(_contrast - rVal) < 0.01)
                {
                    return;
                }
                SetProperty(ref _contrast, rVal);
                OnPropertyChanged("CaptionString");
                UpdateDisplayLine();
            }
        }

        private double _contrastAngle;
        public double ContrastAngle
        {
            get { return _contrastAngle; }
            set
            {
                SetProperty(ref _contrastAngle, value);
                Contrast = Math.Tan(_contrastAngle/180*Math.PI);
            }
        }


        private Point _p1;
        public Point P1
        {
            get { return _p1; }
            set
            {
                if (_p1 == value)
                {
                    return;
                }
                SetProperty(ref _p1, value);
            }
        }

        private Point _p2;
        public Point P2
        {
            get { return _p2; }
            set
            {
                if (_p2 == value)
                {
                    return;
                }
                SetProperty(ref _p2, value);
            }
        }

        private int _displaySize;
        public int DisplaySize
        {
            get { return _displaySize; }
            set
            {
                if (_displaySize == value)
                {
                    return;
                }
                SetProperty(ref _displaySize, value);
            }
        }

        private ImageData _img;

        public override void OnExecute()
        {
            try
            {
                _img = GetInPort(0).Image;

                var processedImg = _img.SetBrightnessAndContrast(_contrast, _bright, Macro.ImageMaxBits);

                _img.Dispose();

                if (processedImg == null)
                {
                    throw new CyteException("BrightContrastModVm", "Invaild execution image is null");
                }
                SetOutputImage(processedImg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Bright Contrast Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtBrightContrastModule;
            HasImage = true;
            View = new BrightContrastModule();
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
        }

        private void UpdateDisplayLine()
        {
            P1 = TransformPointCornidate(new Point(0.0, _bright*FactorB));
            P2 =
                TransformPointCornidate(new Point(Convert.ToDouble(_displaySize),
                    Convert.ToDouble(_displaySize)*Math.Tan(_contrastAngle/180*Math.PI) + _bright*FactorB));
        }

        private Point TransformPointCornidate(Point p)
        {
            var pOut = new Point
            {
                X = p.X,
                Y = DisplaySize - p.Y
            };

            return pOut;
        }


        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("bright", Bright.ToString());
            writer.WriteAttributeString("contrast", Contrast.ToString("F"));
        }

        public override void OnDeserialize(XmlReader reader)
        {
            if (reader["bright"] != null)
            {
                Bright = XmlConvert.ToInt32(reader["bright"]);
            }

            if (reader["contrast"] != null)
            {
                Contrast = XmlConvert.ToDouble(reader["contrast"]);
            }

        }

        public override object Clone()
        {
            var mod = new BrightContrastModVm();
            //===============Common======================
            mod.Name = Name;
            mod.Id = GetNextModId();
            mod.DisplayName = DisplayName;
            mod.ScanNo = ScanNo;
            mod.Enabled = Enabled;
            mod.X = X;
            mod.Y = Y;

            //===============BrightContrastModule=====================
            mod.HasImage = HasImage;
            mod.ModType = ModType;
            mod.View = new BrightContrastModule();

            mod.InputPorts[0].DataType = InputPorts[0].DataType;
            mod.InputPorts[0].ParentModule = mod;
            mod.OutputPort.DataType = OutputPort.DataType;
            mod.OutputPort.ParentModule = mod;

            mod.Bright = Bright;
            mod.Contrast = Contrast;

            return mod;
        }


    }
}
