using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThorCyte.ProtocolModule.ViewModels
{
    public class ModuleTreeViewModel : BindableBase
    {
        public ModuleTreeViewModel()
        {
            MessageHelper.RemoveTreeModuleAction += RemoveTreeModule;
            Initialize();
        }

        private void RemoveTreeModule(object obj)
        {
            var tvi = obj as TreeViewItemModel;
            if (tvi == null) return;

            var iscate = ModuleInfoMgr.CombinationModuleDefs.Where(mc => mc.Category == tvi.Name).Any(mc => tvi.ItemType == ModuleType.None);
            var strprompt = iscate
                ? string.Format("Remove Module-Template Category {0}\n \nAre you sure?", tvi.Name)
                : string.Format("Remove Module-Template {0}\n \nAre you sure?", tvi.Name);
            var res = MessageBox.Show(Application.Current.MainWindow, strprompt, "ThorCyte", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;

            if (tvi.ItemType != ModuleType.SmtCombinedModule && !iscate)
            {                
                MessageBox.Show(Application.Current.MainWindow, "Can not remove this module!", "ThorCyte", MessageBoxButton.OK);
                return;
            }

            if (iscate)
            {
                var cateallMods = ModuleInfoMgr.CombinationModuleDefs.Where(mc => mc.Category == tvi.Name).ToList();

                foreach (var m in cateallMods)
                {
                    ModuleInfoMgr.CombinationModuleDefs.Remove(m);

                }
            }
            else
            {
                var dm = ModuleInfoMgr.CombinationModuleDefs.Where(mc => mc.Name == tvi.Name && mc.Category == tvi.Category).ToList();

                foreach (var m in dm)
                {
                    ModuleInfoMgr.CombinationModuleDefs.Remove(m);
                }
            }

            ModuleInfoMgr.Instance.SaveCombinationModuleTemplates();
            MessageHelper.SetMacroTemplateUpdated();
            MessageHelper.PostMessage(string.Format("Module-Template {0} Removed!", tvi.Name));
        }

        private List<TreeViewItemModel> _listModuleInfos = new List<TreeViewItemModel>
        {
            new TreeViewItemModel 
            { 
                Name = GlobalConst.SingleNodeStr,
                Items = new List<TreeViewItemModel>(),
                IsExpanded = true
            },
            new TreeViewItemModel
            {
                Name = GlobalConst.MultiNodeStr,
                Items = new List<TreeViewItemModel>(),
                IsExpanded = true
            }
        };


        public List<TreeViewItemModel> ListModuleInfos
        {
            get { return _listModuleInfos; }
            set { _listModuleInfos = value; }
        }

        public void Initialize()
        {
            foreach (var minfo in ListModuleInfos.Where(info => !Equals(info, null)))
            {
                minfo.Items.Clear();
            }

            ListModuleInfos[0].IsExpanded = true;

            foreach (var name in ModuleInfoMgr.Categories)
            {
                ListModuleInfos[0].Items.Add(new TreeViewItemModel
                {
                    Name = name,
                    ItemType = ModuleType.None,
                    Category = ListModuleInfos[0].Name
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

                        var tvItem = new TreeViewItemModel()
                        {
                            Name = info.DisplayName,
                            ItemType = GetModuleType(info.DisplayName),
                            Category = item.Name
                        };

                        if (info.Reference == null || info.Reference.Trim() == string.Empty) tvItem.IsEnabled = false;

                        item.Items.Add(tvItem);
                        break;
                    }
                }
            }

            // add combination modules
            foreach (var cmod in ModuleInfoMgr.CombinationModuleDefs)
                AddCombinationModuleNode(cmod);

            MessageHelper.PostMessage("Ready.");
        }

        // add combination module node to the tree
        public void AddCombinationModuleNode(CombinationModule mod)
        {
            // try existing categories

            foreach (var node in ListModuleInfos[1].Items)
            {
                if (node.Name == mod.Category)
                {
                    var cmdNode = new TreeViewItemModel();
                    cmdNode.Name = mod.DisplayName;
                    cmdNode.ItemType = ModuleType.SmtCombinedModule;
                    cmdNode.Category = node.Name;

                    foreach (var m in mod.SubModules)
                    {
                        cmdNode.Items.Add(new TreeViewItemModel()
                        {
                            Name = m.Name,
                            ItemType = ModuleType.None,
                            Category = cmdNode.Name
                        });
                    }

                    node.Items.Add(cmdNode);
                    node.IsExpanded = true;
                    return;
                }
            }

            ListModuleInfos[1].Items.Add(new TreeViewItemModel
            {
                Name = mod.Category,
                ItemType = ModuleType.None,
                Category = ListModuleInfos[1].Name
            });


            AddCombinationModuleNode(mod);
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
                    ItemType = ModuleType.None,
                    Category = ListModuleInfos[0].Name
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

                    var tvItem = new TreeViewItemModel()
                    {
                        Name = info.DisplayName,
                        ItemType = GetModuleType(info.DisplayName),
                        Category = item.Name
                    };

                    if (info.Reference == null || info.Reference.Trim() == string.Empty) tvItem.IsEnabled = false;
                    item.Items.Add(tvItem);
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

            // add combination modules
            foreach (var cmod in ModuleInfoMgr.CombinationModuleDefs.Where(cm => cm.Name.ToLower().Contains(mName.ToLower())))
                AddCombinationModuleNode(cmod);

            MessageHelper.PostMessage(string.Format("Searching done for \"{0}\" ...", mName));
        }

        private ModuleType GetModuleType(string key)
        {
            if (string.IsNullOrEmpty(key)) return ModuleType.None;
            return DataDictionary.ModuleTypeDic.ContainsKey(key) ? DataDictionary.ModuleTypeDic[key] : ModuleType.None;
        }
    }
}
