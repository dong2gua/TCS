using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using ThorCyte.ProtocolModule.Views;

namespace ThorCyte.ProtocolModule.ViewModels
{
    /// <summary>
    /// Defines a PannelVm of _modules and _connections between the _modules.
    /// </summary>
    public sealed class PannelViewModel : BindableBase
    {
        #region Properties and Fields


        private List<TreeViewItemModel> _listModuleInfos = new List<TreeViewItemModel>
        {
            new TreeViewItemModel { Name = GlobalConst.SingleNodeStr,Items = new List<TreeViewItemModel>()},
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
        public ImpObservableCollection<ModuleBase> Modules
        {
            get
            {
                return Macro.Modules ?? (Macro.Modules = new ImpObservableCollection<ModuleBase>());
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

        public ModuleBase SelectedModuleViewModel
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

        #endregion

        #region Constructor

        public PannelViewModel()
        {
            MacroEditor.Instance.CreateModule += CreateModule;
            Macro.Clear += Clear;
            MessageHelper.PostMessage("Ready.");
            Initialize();
        }
        #endregion

        #region Methods

        private void Clear()
        {
            Modules.Clear();
            Connections.Clear();
            SelectedViewItem = null;
        }

        public void Initialize()
        {
            foreach (var minfo in ListModuleInfos.Where(info => !Equals(info, null)))
            {
                minfo.Items.Clear();
            }

            foreach (var name in ModuleInfoMgr.Categories)
            {
                ListModuleInfos[0].Items.Add(new TreeViewItemModel
                {
                    Name = name,
                    ItemType = GetModuleType(name)
                    
                });
            }

            // add regular subModules
            foreach (var info in ModuleInfoMgr.ModuleInfos)
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

            MessageHelper.PostMessage("Ready.");

        }

        



        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        private void connections_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            
            
            
            foreach (ConnectorModel connection in e.Items)
            {
                connection.SourcePort.AttachedConnections.Remove(connection);
                connection.DestPort.AttachedConnections.Remove(connection);
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

            MessageHelper.PostMessage(string.Format("Searching for \"{0}\" ...", mName));

            foreach (var minfo in ListModuleInfos.Where(info => !Equals(info, null)))
            {
                minfo.Items.Clear();
            }

            foreach (var name in ModuleInfoMgr.Categories)
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
            filtedModules.AddRange(ModuleInfoMgr.ModuleInfos.Where(info => !info.IsCombo && info.DisplayName.ToLower().Contains(mName.ToLower())));

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

            foreach (var lstModinfo in ListModuleInfos)
            {
                var rLst = lstModinfo.Items.Where(item => item.Items == null).ToList();

                foreach (var item in rLst)
                {
                    lstModinfo.Items.Remove(item);
                }
            }
            MessageHelper.PostMessage(string.Format("Searching done for \"{0}\" ...", mName));
        }


        private ModuleType GetModuleType(string key)
        {
            if (string.IsNullOrEmpty(key)) return ModuleType.None;
            return DataDictionary.ModuleTypeDic.ContainsKey(key) ? DataDictionary.ModuleTypeDic[key] : ModuleType.None;
        }

        public void UnSelectedAll()
        {
            foreach (var module in Modules)
            {
                module.IsSelected = false;
            }
        }

        private void CreateModule(Point location)
        {
            var moduleInfo = ModuleInfoMgr.GetModuleInfoByDisplayName(SelectedViewItem.Name);
            if (moduleInfo == null)
            {
                return;
            }
            var module = Macro.CreateModule(moduleInfo);
            module.X = (int)location.X;
            module.Y = (int)location.Y;
            module.Initialize();
        }

        #endregion
    }
}
