using System;
using System.Diagnostics;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class TransformModVm : ModuleBase
    {
        public override bool Executable
        {
            get { return true; }
        }

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
            }
        }

        private ImageData _img1;

        public override void OnExecute()
        {
            try
            {
                _img1 = GetInPort(0).Image;

                var processedImg = _img1.Invert(Macro.ImageMaxBits);

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
    }
}
