
using System;
using System.Linq;

namespace ThorCyte.ProtocolModule.Models
{
    public class ModuleInfo
    {
        #region Properties and Fields

        static readonly string[] SmBasicModules =  
        {
			"Detectors",
			"FieldScanMod",
			"Channel",
			"Threshold",
			"Contour",
			"Event",
			"Phantom",
			"OverlapParentChild",
			"OverlapParentChild2"
        };

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Category { get; set; }

        public string Reference { get; set; }

        public string Description { get; set; }

        public Guid Guid { get; set; }

        public bool IsCombo { get; set; }// true if the associated module is a combination module

        public bool IsBasicModule
        {
            get { return SmBasicModules.Any(name => Name == name); }
        }

        #endregion
    }
}
