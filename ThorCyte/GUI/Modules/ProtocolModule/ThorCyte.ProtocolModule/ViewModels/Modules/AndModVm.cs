using System;
using System.Diagnostics;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class AndModVm : ModuleBase
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

                var processedImg = _img1.BitwiseAnd(_img2);
                _img1.Dispose();
                _img2.Dispose();

                if (processedImg == null)
                {
                    throw new CyteException("AndModVm", "Invaild execution image is null");
                }
                SetOutputImage(processedImg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("And Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtAndModule;
            HasImage = true;
            OutputPort.DataType = PortDataType.BinaryImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.BinaryImage;
            InputPorts[0].ParentModule = this;
            InputPorts[1].DataType = PortDataType.BinaryImage;
            InputPorts[1].ParentModule = this;
        }

        public override object Clone()
        {
            var mod = new AndModVm();
            //===============Common======================
            mod.Name = Name;
            mod.Id = GetNextModId();
            mod.DisplayName = DisplayName;
            mod.ScanNo = ScanNo;
            mod.Enabled = Enabled;
            mod.X = X;
            mod.Y = Y;

            //===============And=====================
            mod.HasImage = HasImage;
            mod.ModType = ModType;
            mod.InputPorts[0].DataType = InputPorts[0].DataType;
            mod.InputPorts[0].ParentModule = mod;
            mod.InputPorts[1].DataType = InputPorts[1].DataType;
            mod.InputPorts[1].ParentModule = mod;
            mod.OutputPort.DataType = OutputPort.DataType;
            mod.OutputPort.ParentModule = mod;

            return mod;
        }
    }
}
