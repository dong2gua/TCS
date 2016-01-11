using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Utils;

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


    public class ModuleInfoMgr
    {
        #region Constants
        private const string ModuleInfoPath = @"..\..\..\..\..\..\Xml\Modules.xml";
        #endregion


        #region Properties and Fields

        private static ModuleInfoMgr _uniqueInstance;
        public static ModuleInfoMgr Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = new ModuleInfoMgr()); }
        }

        private static readonly List<string> _categories = new List<string>();
        internal static List<string> Categories
        {
            get { return _categories; }
        }

        private static readonly List<ModuleInfo> _moduleInfos = new List<ModuleInfo>();
        internal static List<ModuleInfo> ModuleInfos
        {
            get { return _moduleInfos; }
        }
        #endregion

        #region Constructor
        private ModuleInfoMgr()
        {
            LoadModuleInfos(ModuleInfoPath);
        }
        #endregion

        #region Methods
        private void LoadModuleInfos(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new CyteException("ProtocolModule.LoadModuleInfo", "Module info file does not exist.");
            }

            XmlTextReader reader = null;
            try
            {
                var tempstr = string.Empty;
                reader = new XmlTextReader(fileName);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "category":
                                tempstr = reader["name"];
                                if (!string.Equals(tempstr, GlobalConst.Systemstr))
                                {
                                    _categories.Add(tempstr);
                                }
                                break;

                            case "module":
                                var info = new ModuleInfo
                                {
                                    Category = tempstr,
                                    Name = reader["name"],
                                    Reference = reader["reference"] ?? string.Empty,
                                    DisplayName = reader["disp-name"] ?? reader["name"]
                                };

                                _moduleInfos.Add(info);
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (reader == null || reader.LineNumber == 0) // file open error
                {
                    throw;
                }

                // read error
                var msg = string.Format("Error reading {0} at line {1}.", fileName, reader.LineNumber);
                throw new CyteException("ProtocolModule.LoadModuleInfo", msg);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public static ModuleInfo GetModuleInfo(string name)
        {
            return _moduleInfos.FirstOrDefault(info => info.Name == name);
        }

        public static ModuleInfo GetModuleInfoByDisplayName(string name)
        {
            return _moduleInfos.FirstOrDefault(info => info.DisplayName == name);
        }
        #endregion
    }
}
