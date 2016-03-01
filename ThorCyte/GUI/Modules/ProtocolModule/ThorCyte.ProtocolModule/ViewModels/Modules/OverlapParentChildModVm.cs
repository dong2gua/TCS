using System;
using System.Diagnostics;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class OverlapParentChildModVm : ModuleBase
    {
        public override bool Executable
        {
            get { return true; }
        }

        public override void OnExecute()
        {
            try
            {
                var parent = GetInPort(0).ComponentName;
                var child = GetInPort(1).ComponentName;

                Debug.WriteLine("Find association of parent: {0}, child: {1}",parent,child);
                Macro.CurrentConponentService.Association(parent,child);

                SetOutputComponent(parent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Association Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtOverlapParentChildModule;
            //Name = GlobalConst.OverlapParentChildModuleName;
            HasImage = false;
            OutputPort.DataType = PortDataType.Event;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.Event;
            InputPorts[0].ParentModule = this;
            InputPorts[1].DataType = PortDataType.Event;
            InputPorts[1].ParentModule = this;
        }

        public override object Clone()
        {
            var mod = new OverlapParentChildModVm();
            //===============Common======================
            mod.Name = Name;
            mod.Id = GetNextModId();
            mod.DisplayName = DisplayName;
            mod.ScanNo = ScanNo;
            mod.Enabled = Enabled;
            mod.X = X;
            mod.Y = Y;

            //===============Association=====================
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
