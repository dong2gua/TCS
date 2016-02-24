using System;
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
            MessageHelper.SetSelectViewItem += SetSelectViewItem;
            MessageHelper.PostMessage("Ready.");
        }

        private void SetSelectViewItem(object item)
        {
            if (item == null) return;
            SelectedViewItem = item as TreeViewItemModel;
        }

        #endregion

        #region Methods

        private void Clear()
        {
            Modules.Clear();
            Connections.Clear();
            SelectedViewItem = null;
            SelectedModuleViewModel = null;
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
