using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;

namespace ThorCyte.ProtocolModule.ViewModels
{
    /// <summary>
    /// Defines a PannelVm of _modules and _connections between the _modules.
    /// </summary>
    public sealed class PannelViewModel : BindableBase
    {
        #region Properties and Fields

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
        private ImpObservableCollection<ModuleVmBase> _modules;

        public ImpObservableCollection<ModuleVmBase> Modules
        {
            get
            {
                return _modules ?? (_modules = new ImpObservableCollection<ModuleVmBase>());
            }
        }

        private readonly List<CombinationModVm> _combinationModulesInWorkspace = new List<CombinationModVm>();

        public List<CombinationModVm> CombinationModulesInWorkspace
        {
            get { return _combinationModulesInWorkspace; }
        }

        /// <summary>
        /// The collection of _connections in the PannelVm.
        /// </summary>
        private ImpObservableCollection<ConnectorModel> _connections;

        public ImpObservableCollection<ConnectorModel> Connections
        {
            get
            {
                if (_connections == null)
                {
                    _connections = new ImpObservableCollection<ConnectorModel>();
                    _connections.ItemsRemoved += connections_ItemsRemoved;
                }
                return _connections;
            }
        }

        private ModuleVmBase _selectedModuleViewModel;

        public ModuleVmBase SelectedModuleViewModel
        {
            get { return _selectedModuleViewModel; }
            set
            {
                if (_selectedModuleViewModel == value)
                {
                    return;
                }

                SetProperty(ref _selectedModuleViewModel, value);
                if (_selectedModuleViewModel != null)
                {
                    _selectedModuleViewModel.Refresh();
                }
            }
        }

        private TreeViewItemModel _selectedViewItem;

        public TreeViewItemModel SelectedViewItem
        {
            get { return _selectedViewItem; }
            set { SetProperty(ref _selectedViewItem, value); }
        }

        #endregion

        #region Constructor

        public PannelViewModel()
        {
            StatusMessage = "Ready.";
            Initialize();
        }

        #endregion

        #region Private Methods

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

            foreach (var name in ProtocolModule.Categories)
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
            filtedModules.AddRange(ProtocolModule.ModuleInfos.Where(info => !info.IsCombo && info.DisplayName.ToLower().Contains(mName.ToLower())));

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
            cmbfiltedModules.AddRange(ProtocolModule.CombinationModuleDefs.Where(info => info.Name.ToLower().Contains(mName.ToLower())));
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

        public void Initialize()
        {
            foreach (var minfo in ListModuleInfos.Where(info => !Equals(info, null)))
            {
                minfo.Items.Clear();
            }

            foreach (var name in ProtocolModule.Categories)
            {
                ListModuleInfos[0].Items.Add(new TreeViewItemModel
                {
                    Name = name,
                    ItemType = GetModuleType(name)
                });
            }

            // add regular subModules
            foreach (var info in ProtocolModule.ModuleInfos)
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

            foreach (var cmod in ProtocolModule.CombinationModuleDefs)
            {
                AddCombinationModuleNode(cmod);
            }

            StatusMessage = "Ready.";
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
            foreach (var module in _modules)
            {
                module.IsSelected = false;
            }
        }
        #endregion Private Methods
    }
}
