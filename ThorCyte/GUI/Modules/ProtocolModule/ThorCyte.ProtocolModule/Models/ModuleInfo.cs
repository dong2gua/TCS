using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;

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

        public string ViewReference { get; set; }

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
        private const string ModuleInfoPath = @".\Xml\Modules.xml";
        private const string ComModuleInfoPath = @".\Xml\CombinationModules.xml";
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

        private static List<CombinationModule> _combinationModuleDefs = new List<CombinationModule>();
        internal static List<CombinationModule> CombinationModuleDefs
        {
            get { return _combinationModuleDefs; }
        }
        #endregion

        #region Constructor
        private ModuleInfoMgr()
        {
            LoadModuleInfos(ModuleInfoPath);
            LoadCombinationModuleTemplates();
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
                                    ViewReference = reader["viewreference"] ?? string.Empty,
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
                throw new CyteException("ModuleInfo.LoadModuleInfo", msg);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public void LoadCombinationModuleTemplates()
        {
            if (!File.Exists(ComModuleInfoPath))
                return;

            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(ComModuleInfoPath);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "combination-module")
                    {
                        var temp = new CombinationModule();
                        temp.CreateFromXml(reader);
                        AddCombinationModuleTemplate(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CyteException("ModuleInfo.LoadCombinationModuleDefs", ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        // save combination module definitions to the definition file
        public void SaveCombinationModuleTemplates()
        {
            if (string.IsNullOrEmpty(ComModuleInfoPath))
                return;

            var writer = new XmlTextWriter(ComModuleInfoPath, new UTF8Encoding());
            writer.Formatting = Formatting.Indented;

            writer.WriteStartDocument(false);
            writer.WriteStartElement("combination-modules");

            foreach (CombinationModule cmod in _combinationModuleDefs)
            {
                cmod.WriteToXml(writer);
                AddCombinationModuleTemplate(cmod);
            }

            writer.WriteEndElement();	// </combination-modules>
            writer.Close();
        }

        public static void AddCombinationModuleTemplate(CombinationModule cmod)
        {
            if (!_combinationModuleDefs.Contains(cmod))
            {
                _combinationModuleDefs.Add(cmod);
            }
            
            var info = new ModuleInfo();
            info.Category = cmod.Category;
            info.DisplayName = info.Name = cmod.DisplayName;
            info.Guid = cmod.Guid;
            info.IsCombo = true;

            if(!_moduleInfos.Contains(info)) 
                _moduleInfos.Add(info);
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
