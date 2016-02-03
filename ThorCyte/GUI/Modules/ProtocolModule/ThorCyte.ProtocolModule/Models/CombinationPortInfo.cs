using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThorCyte.ProtocolModule.Models
{
    public class CombinationPortInfo
    {
        #region Properties

        public int ModuleId
        {
            get;
            set;
        }

        public int PortIndex
        {
            get;
            set;
        }

        public PortDataType DataType
        {
            get;
            set;
        }

        #endregion
    }
}
