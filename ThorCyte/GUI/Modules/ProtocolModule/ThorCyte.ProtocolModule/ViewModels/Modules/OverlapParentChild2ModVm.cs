using System;
using System.Diagnostics;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class OverlapParentChild2ModVm : ModuleBase
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
                var child1 = GetInPort(1).ComponentName;
                var child2 = GetInPort(2).ComponentName;

                Debug.WriteLine("Find association of parent: {0}, child1: {1}, child2: {2}", parent, child1, child2);
                Macro.CurrentConponentService.Association(parent, child1, child2);

                SetOutputComponent(parent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Association2 Module error: " + ex.Message);
                throw;
            }
        }


        public override void Initialize()
        {
            ModType = ModuleType.SmtOverlapParentChildModule;
            HasImage = false;
            OutputPort.DataType = PortDataType.Event;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.Event;
            InputPorts[0].ParentModule = this;
            InputPorts[1].DataType = PortDataType.Event;
            InputPorts[1].ParentModule = this;
            InputPorts[2].DataType = PortDataType.Event;
            InputPorts[2].ParentModule = this;
        }
    }
}
