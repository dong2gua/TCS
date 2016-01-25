using System;
using System.Diagnostics;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;

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

                //todo: call find parent and child.
                Debug.WriteLine("Find Association (parent: {0} child: {1}) overlap.",parent,child);

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

    }
}
