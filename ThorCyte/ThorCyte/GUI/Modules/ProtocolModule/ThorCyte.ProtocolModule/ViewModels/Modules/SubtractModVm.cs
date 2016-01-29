using System;
using System.Diagnostics;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class SubtractModVm : ModuleBase
    {
        public override bool Executable
        {
            get { return true; }
        }

        private ImageData _img1;
        private ImageData _img2;

        public override void OnExecute()
        {
            try
            {
                _img1 = GetInPort(0).Image;
                _img2 = GetInPort(1).Image;

                var processedImg = _img1.Sub(_img2);

                _img1.Dispose();
                _img2.Dispose();

                if (processedImg == null)
                {
                    throw new CyteException("SubtractModVm", "Invaild execution image is null");
                }
                SetOutputImage(processedImg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Subtract Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtAddModule;
            HasImage = true;
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
            InputPorts[1].DataType = PortDataType.GrayImage;
            InputPorts[1].ParentModule = this;
        }

    }
}
