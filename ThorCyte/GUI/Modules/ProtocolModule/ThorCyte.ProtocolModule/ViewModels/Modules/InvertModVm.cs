using System;
using System.Diagnostics;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class InvertModVm : ModuleBase
    {
        public override bool Executable
        {
            get { return true; }
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
                    throw new CyteException("InvertModVm", "Invaild execution image is null");
                }
                SetOutputImage(processedImg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Invert Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtInvertModule;
            HasImage = true;
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
        }
    }
}
