using System;
using System.Diagnostics;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class XorModVm : ModuleBase
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

                var processedImg = _img1.BitwiseXor(_img2);

                _img1.Dispose();
                _img2.Dispose();

                if (processedImg == null)
                {
                    throw new CyteException("XorModVm", "Invaild execution image is null");
                }
                SetOutputImage(processedImg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Xor Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtXorModule;
            HasImage = true;
            OutputPort.DataType = PortDataType.BinaryImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.BinaryImage;
            InputPorts[0].ParentModule = this;
            InputPorts[1].DataType = PortDataType.BinaryImage;
            InputPorts[1].ParentModule = this;
        }
    }
}
