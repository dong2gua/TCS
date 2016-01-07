using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;

namespace ThorCyte.ProtocolModule.Models
{
    public class Macro
    {
        #region Constants
        private const string ModuleInfoPath = @"..\..\..\..\..\..\Xml\Modules.xml";
        private const string ComModuleInfoPath = @"..\..\..\..\..\..\Xml\CombinationModules.xml";
        #endregion

        #region Constants
        private Macro()
        {
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
            LoadModuleInfos(ModuleInfoPath);
            LoadCombinationModuleTemplates(ComModuleInfoPath);
        }
        #endregion

        #region Fields and Properties
        public static ImageData CurrentImage { set; get; }
        public delegate ModuleVmBase CreateCombinationModuleFromWorkspaceHandler(string name, int id);
        public static CreateCombinationModuleFromWorkspaceHandler CreateCombinationModuleFromWorkspace;
        public delegate ModuleVmBase CreateModuleHandler(ModuleInfo info);
        public static CreateModuleHandler CreateModule;
        public delegate void CreateConnectorHandler(int inPortId, int outPortId, int inPortIndex, int outPortIndex);
        public static CreateConnectorHandler CreateConnector;

        private static Macro _uniqueInstance;
        public static Macro Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = new Macro()); }
        }

        private static readonly List<string> _categories = new List<string>();
        internal static List<string> Categories
        {
            get { return _categories; }
        }

        private static readonly List<ModuleInfo> _sysModuleInfos = new List<ModuleInfo>();
        internal static List<ModuleInfo> SysModuleInfos
        {
            get { return _sysModuleInfos; }
        }

        private static readonly List<ModuleInfo> _moduleInfos = new List<ModuleInfo>();
        internal static List<ModuleInfo> ModuleInfos
        {
            get { return _moduleInfos; }
        }

        private static readonly List<CombinationModVm> _combinationModuleDefs = new List<CombinationModVm>();
        internal static List<CombinationModVm> CombinationModuleDefs
        {
            get { return _combinationModuleDefs; }
        }

        public static ScanInfo CurrentScanInfo { get; set; }
        public static int CurrentScanId { get; set; }

        public static ImpObservableCollection<ConnectorModel> Connections;
        public static ImpObservableCollection<ModuleVmBase> Modules;
        public static readonly List<CombinationModVm> CombinationModulesInWorkspace = new List<CombinationModVm>();
        public static ModuleVmBase SelectedModuleViewModel;


        private IEventAggregator EventAggregator
        {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }
        #endregion

        #region Methods

        private void ExpLoaded(int scanId)
        {
            CurrentScanId = scanId;
            CurrentScanInfo = ServiceLocator.Current.GetInstance<IExperiment>().GetScanInfo(scanId);
        }

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
                                if (string.Equals(tempstr, GlobalConst.Systemstr))
                                {
                                    _sysModuleInfos.Add(info);
                                }
                                else
                                {
                                    _moduleInfos.Add(info);
                                }
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

        // load combination module definitions at startup
        private static void LoadCombinationModuleTemplates(string path)
        {
            if (!File.Exists(path))
                return;

            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(path);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "combination-module")
                    {
                        var temp = new CombinationModVm();
                        temp.CreateFromXml(reader);
                        AddCombinationModuleTemplate(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CyteException("ProtocolModule.LoadCombinationModuleDefs", ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }


        private static void AddCombinationModuleTemplate(CombinationModVm cmod)
        {
            _combinationModuleDefs.Add(cmod);
            var info = new ModuleInfo
            {
                Category = cmod.Category,
                Guid = cmod.Guid,
                IsCombo = true
            };
            info.DisplayName = info.Name;
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

        /// <summary>
        /// Load Marco data from xml file.
        /// </summary>
        /// <param name="reader"></param>
        public void Load(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "module":
                            var id = XmlConvert.ToInt32(reader["id"]);
                            ModuleVmBase module;
                            if (reader.Name == "combination-module")
                            {
                                var name = reader["name"];

                                module = CreateCombinationModuleFromWorkspace(name, id);
                            }
                            else
                            {
                                var info = GetModuleInfo(reader["name"]);
                                if (info == null)
                                    throw new CyteException("Macro.LoadMacro", string.Format("Could not create module [{0}].\nModule is not defined in modules.xml.", reader["name"]));
                                if (string.IsNullOrEmpty(info.Reference))
                                {
                                    continue;
                                }
                                module = CreateModule(info);
                            }

                            module.Id = id;
                            module.Enabled = Convert.ToBoolean(reader["enabled"]);
                            module.ScanNo = Convert.ToInt32(reader["scale"]);

                            if (reader["x"] != null)
                            {
                                module.X = (int)(XmlConvert.ToInt32(reader["x"]) * 1.5);
                            }

                            if (reader["y"] != null)
                            {
                                module.Y = (int)(XmlConvert.ToInt32(reader["y"]) * 1.5);
                            }
                            module.Initialize();
                            module.Deserialize(reader);
                            break;
                        case "connector":
                            if (reader["inport-module-id"] != null && reader["outport-module-id"] != null)
                            {
                                var inPortId = XmlConvert.ToInt32(reader["inport-module-id"]);
                                var outPortId = XmlConvert.ToInt32(reader["outport-module-id"]);
                                var inPortIndex = 0;
                                if (reader["inport-index"] != null)
                                {
                                    inPortIndex = XmlConvert.ToInt32(reader["inport-index"]);
                                }
                                var outPortIndex = 0;
                                if (reader["outport-index"] != null)
                                {
                                    outPortIndex = XmlConvert.ToInt32(reader["outport-index"]);
                                }
                                CreateConnector(inPortId, outPortId, inPortIndex, outPortIndex);
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "macro")
                    {
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
