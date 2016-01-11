using System;
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
        private Macro()
        {
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
        }
        #endregion

        #region Fields and Properties

        private static ImageData _currentImage;
        public static ImageData CurrentImage {
            get { return _currentImage; }
            set
            {
                if(_currentImage.Equals(value)) return;
                
                if (_currentImage != null)
                {
                    _currentImage.Dispose();
                }
                _currentImage = value;
            }
        }

        public delegate void ClearHandler();
        public static ClearHandler Clear;

        private static Macro _uniqueInstance;
        public static Macro Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = new Macro()); }
        }

        public static ScanInfo CurrentScanInfo { get; private set; }
        public static int CurrentScanId { get; private set; }
        public static IData CurrentDataMgr { get; private set; }

        public static ImpObservableCollection<ConnectorModel> Connections;
        public static ImpObservableCollection<ModuleBase> Modules;
        public static ModuleBase SelectedModuleViewModel;

        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        #endregion

        #region Methods
        private void ExpLoaded(int scanId)
        {
            Clear(); 
            CurrentScanId = scanId;
            CurrentScanInfo = ServiceLocator.Current.GetInstance<IExperiment>().GetScanInfo(scanId);
            CurrentDataMgr = ServiceLocator.Current.GetInstance<IData>();
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
                            ModuleBase module;
                            if (reader.Name == "combination-module")
                            {
                                throw new CyteException("Macro.Load","Combination module does not support yet.");
                            }
                            var info = ModuleInfoMgr.GetModuleInfo(reader["name"]);
                            if (info == null)
                                throw new CyteException("Macro.Load", string.Format("Could not create module [{0}].\nModule is not defined in modules.xml.", reader["name"]));
                            if (string.IsNullOrEmpty(info.Reference))
                            {
                                continue;
                            }
                            module = CreateModule(info);

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

        public static ModuleBase CreateModule(ModuleInfo modInfo)
        {
            try
            {
                if (modInfo.IsCombo) throw new CyteException("Macro.CreateModule", "Combination module does not support.");// create combination module
                var module = (ModuleBase)Activator.CreateInstance(Type.GetType(modInfo.Reference, true));
                module.Name = modInfo.Name;
                module.DisplayName = modInfo.DisplayName;
                Modules.Add(module);
                return module;
            }
            catch (Exception ex)
            {
                throw new CyteException("Macro.CreateModule", string.Format("Could not create module [{0}].", modInfo.Reference) + ex.Message);
            }
        }

        private void CreateConnector(int inPortId, int outPortId, int inPortIndex, int outPortIndex)
        {
            ModuleBase inModule = null;
            ModuleBase outModule = null;

            foreach (var module in Modules)
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
            Connections.Add(connector);
        }


        public static void Run()
        {
            //Find the Experiment module.
            var expMod = Modules.FirstOrDefault(m => m is ExperimentModVm);
            if (expMod == null) return;

            foreach (var region in CurrentScanInfo.ScanRegionList)
            {
                foreach (var tile in region.ScanFieldList)
                {
                    ((ExperimentModVm)expMod).AnalyzeImage(region.RegionId, tile.ScanFieldId);
                }
            }
        }


        #endregion
    }
}
