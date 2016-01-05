using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using ThorCyte.ProtocolModule.Views;
using ModuleInfo = ThorCyte.ProtocolModule.Models.ModuleInfo;

namespace ThorCyte.ProtocolModule
{
    public class ProtocolModule : IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;
        private static ProtocolModule _instance;


        private const string ModuleInfoPath = @"..\..\..\..\..\..\Xml\Modules.xml";
        private const string ComModuleInfoPath = @"..\..\..\..\..\..\Xml\CombinationModules.xml";

        //Singleton
        public static ProtocolModule Instance
        {
            get { return _instance ?? (_instance = ServiceLocator.Current.GetInstance<ProtocolModule>()); }
        }

        public MainWindowViewModel MainVm
        {
            get { return MainWindowViewModel.Instance; }
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

        public static ImageData CurrentImage { set; get; }

        public ProtocolModule()
        {
            _regionViewRegistry = ServiceLocator.Current.GetInstance<IRegionViewRegistry>();
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance<ProtocolModule>(this);
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);

        }

        private ScanInfo _currentScanInfo;
        public ScanInfo CurrentScanInfo
        {
            get { return _currentScanInfo; }
            set { _currentScanInfo = value; }
        }

        private int _currentScanId;
        public int CurrentScanId
        {
            get { return _currentScanId; }
            set { _currentScanId = value; }
        }

        private IEventAggregator EventAggregator
        {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }

        public void Initialize()
        {
            LoadModuleInfos(ModuleInfoPath);
            LoadCombinationModuleTemplates(ComModuleInfoPath);
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ProtocolRegion, typeof(MacroEditor));
            MacroEditor.Instance.CreateModule += CreateModule;
        }

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
                var _tempstr = string.Empty;
                reader = new XmlTextReader(fileName);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "category":
                                _tempstr = reader["name"];
                                if (!string.Equals(_tempstr, GlobalConst.Systemstr))
                                {
                                    _categories.Add(_tempstr);
                                }
                                break;

                            case "module":
                                var info = new ModuleInfo
                                {
                                    Category = _tempstr,
                                    Name = reader["name"],
                                    Reference = reader["reference"] ?? string.Empty,
                                    DisplayName = reader["disp-name"] ?? reader["name"]
                                };
                                if (string.Equals(_tempstr, GlobalConst.Systemstr))
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
            catch (Exception ex)
            {
                if (reader == null || reader.LineNumber == 0) // file open error
                {
                    throw ex;
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
                throw new CyteException("ProtocolSystem.LoadCombinationModuleDefs", ex.Message);
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

        private CombinationModVm GetCombinationModuleTemplate(Guid guid, string name)
        {
            return MainVm.PannelVm.CombinationModulesInWorkspace.FirstOrDefault(cmd => cmd.Guid == guid && cmd.DisplayName.ToLower() == name.ToLower());
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
        public void LoadMacro(XmlReader reader)
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
                                    throw new CyteException("ProtocolSystem.LoadMacro", string.Format("Could not create module [{0}].\nModule is not defined in modules.xml.", reader["name"]));
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

        private ModuleVmBase CreateCombinationModuleFromWorkspace(string name, int id) // jcl-6568
        {
            try
            {
                CombinationModVm module = null;
                foreach (var cmd in MainVm.PannelVm.CombinationModulesInWorkspace)
                {
                    if (cmd.Id == id)
                    {
                        module = new CombinationModVm(cmd)
                        {
                            ParentMacro = this
                        };

                        module.SubModules.ForEach(m => m.ParentMacro = this);
                        MainVm.PannelVm.Modules.Add(module);
                    }
                }

                return module;
            }
            catch (Exception ex)
            {
                throw new CyteException("ProtocolSystem.CreateModule", string.Format("Could not create module [{0}].", name) + ex.Message);
            }
        }

        private void CreateModule(Point location)
        {
            var moduleInfo = GetModuleInfoByDisplayName(MainVm.PannelVm.SelectedViewItem.Name);
            if (moduleInfo == null)
            {
                return;
            }
            var module = CreateModule(moduleInfo);
            module.X = (int)location.X;
            module.Y = (int)location.Y;
            module.Initialize();
        }

        public ModuleVmBase CreateModule(ModuleInfo modInfo)
        {
            try
            {
                ModuleVmBase module;
                if (modInfo.IsCombo) // create combination module
                {
                    var template = GetCombinationModuleTemplate(modInfo.Guid, modInfo.DisplayName);
                    module = new CombinationModVm(template);
                    foreach (var m in ((CombinationModVm)module).SubModules)
                    {
                        m.ParentMacro = this; // set the parent of each sub module to be script
                    }
                }
                else
                {
                    module = (ModuleVmBase)Activator.CreateInstance(Type.GetType(modInfo.Reference, true));
                    module.Name = modInfo.Name;
                    module.DisplayName = modInfo.DisplayName;
                }
                module.ParentMacro = this;
                MainVm.PannelVm.Modules.Add(module);
                return module;
            }
            catch (Exception ex)
            {
                throw new CyteException("ProtocolSystem.CreateModule", string.Format("Could not create module [{0}].", modInfo.Reference) + ex.Message);
            }
        }

        private void CreateConnector(int inPortId, int outPortId, int inPortIndex, int outPortIndex)
        {
            ModuleVmBase inModule = null;
            ModuleVmBase outModule = null;

            foreach (var module in MainVm.PannelVm.Modules)
            {
                if (module.Id == inPortId)
                {
                    inModule = module;
                }
                else if (module.Id == outPortId)
                {
                    outModule = module;
                }

                if (inModule != null && outModule != null)
                {
                    break;
                }
            }

            if (inModule == null || outModule == null)
            {
                return;
            }
            var connector = new ConnectorModel(outModule.OutputPort, inModule.InputPorts[inPortIndex]);

            MainVm.PannelVm.Connections.Add(connector);
        }

        public MacroEditor GetView()
        {
            return MacroEditor.Instance;
        }
    }
}
