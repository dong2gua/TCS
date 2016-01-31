using System;
using System.Diagnostics;
using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class TransformModVm : ModuleBase
    {
        public bool IsVaild;
        public override bool Executable
        {
            get { return IsVaild; }
        }

        public override string CaptionString { get
        {
            return string.Format("X{0}, Y{1}, Deg{2}", _transX, _transY, _rotateDeg);
        } }

        private int _transX;
        public int TransX
        {
            get { return _transX; }
            set
            {
                if (_transX == value)
                {
                    return;
                }
                SetProperty(ref _transX, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private int _transY;
        public int TransY
        {
            get { return _transY; }
            set
            {
                if (_transY == value)
                {
                    return;
                }
                SetProperty(ref _transY, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private int _rotateDeg;
        public int RotateDeg
        {
            get { return _rotateDeg; }
            set
            {
                if (_rotateDeg == value)
                {
                    return;
                }
                SetProperty(ref _rotateDeg, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private ImageData _img1;


        public TransformModVm()
        {
            IsVaild = true;
        }

        public override void OnExecute()
        {
            try
            {
                _img1 = GetInPort(0).Image;

                var processedImg = _img1.Transform(_transX, _transY, _rotateDeg);

                _img1.Dispose();

                if (processedImg == null)
                {
                    throw new CyteException("TransformModVm", "Invaild execution image is null");
                }
                SetOutputImage(processedImg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Transform Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtTransformModule;
            HasImage = true;
            View = new TransformModule();
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("trans_x", TransX.ToString());
            writer.WriteAttributeString("trans_y", TransY.ToString());
            writer.WriteAttributeString("rotate", RotateDeg.ToString());
        }

        public override void OnDeserialize(XmlReader reader)
        {
            if (reader["trans_x"] != null)
            {
                TransX = XmlConvert.ToInt32(reader["trans_x"]);
            }

            if (reader["trans_y"] != null)
            {
                TransY = XmlConvert.ToInt32(reader["trans_y"]);
            }

            if (reader["rotate"] != null)
            {
                RotateDeg = XmlConvert.ToInt32(reader["rotate"]);
            }
        }
    }
}
