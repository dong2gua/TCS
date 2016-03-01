using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.ViewModels
{
    public class ModuleTreeViewModel : BindableBase
    {
        public ModuleTreeViewModel()
        {
            Initialize();
        }

        private List<TreeViewItemModel> _listModuleInfos = new List<TreeViewItemModel>
        {
            new TreeViewItemModel 
            { 
                Name = GlobalConst.SingleNodeStr,
                Items = new List<TreeViewItemModel>(),
                IsExpanded = true
            },
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
                            ItemType = GetModuleType(info.DisplayName)
                        };

                        if (info.Reference == null || info.Reference.Trim() == string.Empty) tvItem.IsEnabled = false;

                        item.Items.Add(tvItem);
                        break;
                    }
                }
            }

            MessageHelper.PostMessage("Ready.");

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
                    ItemType = ModuleType.None
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
                        ItemType = GetModuleType(info.DisplayName)
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
            MessageHelper.PostMessage(string.Format("Searching done for \"{0}\" ...", mName));
        }

        private ModuleType GetModuleType(string key)
        {
            if (string.IsNullOrEmpty(key)) return ModuleType.None;
            return DataDictionary.ModuleTypeDic.ContainsKey(key) ? DataDictionary.ModuleTypeDic[key] : ModuleType.None;
        }


    }
}
