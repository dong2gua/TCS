using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using ThorCyte.ProtocolModule.ViewModels.ModulesBase;
using ThorCyte.ProtocolModule.Views;

namespace ThorCyte.ProtocolModule.ViewModels
{
    /// <summary>
    /// Defines a PannelVm of _modules and _connections between the _modules.
    /// </summary>
    public sealed class PannelViewModel : BindableBase
    {
        #region Properties and Fields
        private readonly Macro _thisMacro;

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        private List<TreeViewItemModel> _listModuleInfos = new List<TreeViewItemModel>
        {
            new TreeViewItemModel { Name = GlobalConst.SingleNodeStr,Items = new List<TreeViewItemModel>()},
            new TreeViewItemModel { Name = GlobalConst.MultiNodeStr,Items = new List<TreeViewItemModel>()}
        };


        public List<TreeViewItemModel> ListModuleInfos
        {
            get { return _listModuleInfos; }
            set { _listModuleInfos = value; }
        }

        /// <summary>
        /// The collection of _modules in the PannelVm.
        /// </summary>
        //private ImpObservableCollection<ModuleVmBase> _modules;

        public ImpObservableCollection<ModuleVmBase> Modules
        {
            get
            {
                return Macro.Modules ?? (Macro.Modules = new ImpObservableCollection<ModuleVmBase>());
            }
        }

        /// <summary>
        /// The collection of _connections in the PannelVm.
        /// </summary>
        //private ImpObservableCollection<ConnectorModel> _connections;

        public ImpObservableCollection<ConnectorModel> Connections
        {
            get
            {
                if (Macro.Connections == null)
                {
                    Macro.Connections = new ImpObservableCollection<ConnectorModel>();
                    Macro.Connections.ItemsRemoved += connections_ItemsRemoved;
                }
                return Macro.Connections;
            }
        }

        //private readonly List<CombinationModVm> CombinationModulesInWorkspace = new List<CombinationModVm>();

        public List<CombinationModVm> CombinationModulesInWorkspace
        {
            get { return Macro.CombinationModulesInWorkspace; }
        }

        //private ModuleVmBase _selectedModuleViewModel;

        public ModuleVmBase SelectedModuleViewModel
        {
            get { return Macro.SelectedModuleViewModel; }
            set
            {
                if (Macro.SelectedModuleViewModel == value)
                {
                    return;
                }

                SetProperty(ref Macro.SelectedModuleViewModel, value);
                if (Macro.SelectedModuleViewModel != null)
                {
                    Macro.SelectedModuleViewModel.Refresh();
                }
            }
        }

        private TreeViewItemModel _selectedViewItem;

        public TreeViewItemModel SelectedViewItem
        {
            get { return _selectedViewItem; }
            set { SetProperty(ref _selectedViewItem, value); }
        }

        private IEventAggregator EventAggregator
        {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }

        #endregion

        #region Constructor

        public PannelViewModel()
        {
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
            _thisMacro = Macro.Instance;
            MacroEditor.Instance.CreateModule += CreateModule;
            Macro.CreateCombinationModuleFromWorkspace += CreateCombinationModuleFromWorkspace;
            Macro.CreateModule += CreateModule;
            Macro.CreateConnector += CreateConnector;
            StatusMessage = "Ready.";
            Initialize();
        }
        #endregion

        #region Methods
        private void ExpLoaded(int scanId)
        {
            Clear();
        }

        private void Clear()
        {
            Modules.Clear();
            Connections.Clear();
            CombinationModulesInWorkspace.Clear();
            SelectedViewItem = null;
        }

        public void Initialize()
        {
            foreach (var minfo in ListModuleInfos.Where(info => !Equals(info, null)))
            {
                minfo.Items.Clear();
            }

            foreach (var name in Macro.Categories)
            {
                ListModuleInfos[0].Items.Add(new TreeViewItemModel
                {
                    Name = name,
                    ItemType = GetModuleType(name)
                });
            }

            // add regular subModules
            foreach (var info in Macro.ModuleInfos)
            {
                foreach (var item in ListModuleInfos[0].Items)
                {
                    if (!info.IsCombo && item.Name == info.Category)
                    {
                        if (item.Items == null)
                        {
                            item.Items = new List<TreeViewItemModel>();
                        }
                        item.Items.Add(new TreeViewItemModel
                        {
                            Name = info.DisplayName,
                            ItemType = GetModuleType(info.DisplayName)
                        });
                        break;
                    }
                }
            }

            foreach (var cmod in Macro.CombinationModuleDefs)
            {
                AddCombinationModuleNode(cmod);
            }
            StatusMessage = "Ready.";
        }


        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        private void connections_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorModel connection in e.Items)
            {
                connection.SourcePort = null;
                connection.DestPort = null;
            }
        }

        public void FilterModuleInfo(string mName)
        {
            if (mName == string.Empty)
            {
                Initialize();
                return;
            }

            StatusMessage = string.Format("Searching for \"{0}\" ...", mName);

            foreach (var minfo in ListModuleInfos.Where(info => !Equals(info, null)))
            {
                minfo.Items.Clear();
            }

            foreach (var name in Macro.Categories)
            {
                ListModuleInfos[0].Items.Add(new TreeViewItemModel
                {
                    Name = name,
                    ItemType = GetModuleType(name)
                });
            }

            var filtedModules = new List<ModuleInfo>();
            filtedModules.Clear();
            // filt my modules
            filtedModules.AddRange(Macro.ModuleInfos.Where(info => !info.IsCombo && info.DisplayName.ToLower().Contains(mName.ToLower())));

            // add regular subModules
            foreach (var info in filtedModules)
            {
                foreach (var item in ListModuleInfos[0].Items.Where(item => !info.IsCombo && item.Name == info.Category))
                {
                    if (item.Items == null)
                    {
                        item.Items = new List<TreeViewItemModel>();
                    }
                    item.Items.Add(new TreeViewItemModel
                    {
                        Name = info.DisplayName,
                        ItemType = GetModuleType(info.DisplayName)
                    });
                    break;
                }
            }

            var cmbfiltedModules = new List<CombinationModVm>();
            cmbfiltedModules.Clear();
            cmbfiltedModules.AddRange(Macro.CombinationModuleDefs.Where(info => info.Name.ToLower().Contains(mName.ToLower())));
            foreach (var cmod in cmbfiltedModules)
            {
                AddCombinationModuleNode(cmod);
            }


            foreach (var _lstModinfo in ListModuleInfos)
            {
                var rLst = _lstModinfo.Items.Where(item => item.Items == null).ToList();

                foreach (var item in rLst)
                {
                    _lstModinfo.Items.Remove(item);
                }
            }
            StatusMessage = string.Format("Searching done for \"{0}\" ...", mName);
        }


        private ModuleType GetModuleType(string key)
        {
            if (string.IsNullOrEmpty(key)) return ModuleType.None;
            return DataDictionary.ModuleTypeDic.ContainsKey(key) ? DataDictionary.ModuleTypeDic[key] : ModuleType.None;
        }

        // add combination module node to the tree
        public void AddCombinationModuleNode(CombinationModVm mod)
        {
            // try existing categories
            foreach (var item in ListModuleInfos[1].Items)
            {
                if (string.Equals(item.Name, mod.Category))
                {
                    var cmdItem = new TreeViewItemModel
                    {
                        Name = mod.Name,
                        ItemType = GetModuleType(mod.Name)
                    };

                    foreach (var moduleVmBase in mod.SubModules)
                    {
                        var m = (CombinationModVm)moduleVmBase;
                        if (cmdItem.Items == null)
                        {
                            cmdItem.Items = new List<TreeViewItemModel>();
                        }
                        cmdItem.Items.Add(new TreeViewItemModel
                        {
                            Name = m.Name,
                            ItemType = GetModuleType(m.Name)
                        });
                    }
                    if (item.Items == null)
                    {
                        item.Items = new List<TreeViewItemModel>();
                    }
                    item.Items.Add(cmdItem);
                    return;
                }
            }

            // no category found, create a new category node and add to it
            ListModuleInfos[1].Items.Add(new TreeViewItemModel
            {
                Name = mod.Category,
                ItemType = GetModuleType(mod.Category)
            });
            AddCombinationModuleNode(mod);
        }

        public void UnSelectedAll()
        {
            foreach (var module in Modules)
            {
                module.IsSelected = false;
            }
        }

        private ModuleVmBase CreateCombinationModuleFromWorkspace(string name, int id) // jcl-6568
        {
            try
            {
                CombinationModVm module = null;
                foreach (var cmd in CombinationModulesInWorkspace)
                {
                    if (cmd.Id == id)
                    {
                        module = new CombinationModVm(cmd)
                        {
                            ParentMacro = _thisMacro
                        };

                        module.SubModules.ForEach(m => m.ParentMacro = _thisMacro);
                        Modules.Add(module);
                    }
                }

                return module;
            }
            catch (Exception ex)
            {
                throw new CyteException("ProtocolModule.CreateModule", string.Format("Could not create module [{0}].", name) + ex.Message);
            }
        }

        private void CreateModule(Point location)
        {
            var moduleInfo = Macro.GetModuleInfoByDisplayName(SelectedViewItem.Name);
            if (moduleInfo == null)
            {
                return;
            }
            var module = CreateModule(moduleInfo);
            module.X = (int)location.X;
            module.Y = (int)location.Y;
            module.Initialize();
        }

        public CombinationModVm GetCombinationModuleTemplate(Guid guid, string name)
        {
            return CombinationModulesInWorkspace.FirstOrDefault(cmd => cmd.Guid == guid && cmd.DisplayName.ToLower() == name.ToLower());
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
                        m.ParentMacro = _thisMacro; // set the parent of each sub module to be script
                    }
                }
                else
                {
                    module = (ModuleVmBase)Activator.CreateInstance(Type.GetType(modInfo.Reference, true));
                    module.Name = modInfo.Name;
                    module.DisplayName = modInfo.DisplayName;
                }
                module.ParentMacro = _thisMacro;
                Modules.Add(module);
                return module;
            }
            catch (Exception ex)
            {
                throw new CyteException("ProtocolModule.CreateModule", string.Format("Could not create module [{0}].", modInfo.Reference) + ex.Message);
            }
        }

        private void CreateConnector(int inPortId, int outPortId, int inPortIndex, int outPortIndex)
        {
            ModuleVmBase inModule = null;
            ModuleVmBase outModule = null;

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
        #endregion
    }
}
