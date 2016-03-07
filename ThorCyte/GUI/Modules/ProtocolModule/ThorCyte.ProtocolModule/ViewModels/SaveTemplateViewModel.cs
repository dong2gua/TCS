using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Mvvm;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;


namespace ThorCyte.ProtocolModule.ViewModels
{
    public class SaveTemplateViewModel : BindableBase
    {
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public ImpObservableCollection<ModuleBase> AsIsModules { get; private set; }
        public ImpObservableCollection<ConnectorModel> AsIsConnections { get; private set; }


        public ImpObservableCollection<string> Categorys { get; private set; }
        private string _category = string.Empty;
        public string Category
        {
            get { return _category; }
            set
            {
                if (_category == value) return;
                SetProperty(ref _category, value);
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                SetProperty(ref _name, value);
            }
        }

        private ImpObservableCollection<string> _subModuleCaptions;
        public ImpObservableCollection<string> SubModuleCaptions
        {
            get { return _subModuleCaptions; }
        }


        private string _selectedmodCaption;
        public string SelectedModCaption
        {
            get { return _selectedmodCaption; }
            set
            {
                if (_selectedmodCaption == value) return;
                SetProperty(ref _selectedmodCaption, value);
            }
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment == value) return;
                SetProperty(ref _comment, value);
            }
        }

        private DependencyObject _viewObj;
        private Dictionary<int, int> _refdic;

        public SaveTemplateViewModel(DependencyObject viewObj)
        {
            _viewObj = viewObj;
            SaveCommand = new DelegateCommand(OnSave);
            CancelCommand = new DelegateCommand(OnCancel);
            _subModuleCaptions = new ImpObservableCollection<string>();
            AsIsModules = new ImpObservableCollection<ModuleBase>();
            AsIsConnections = new ImpObservableCollection<ConnectorModel>();
            Categorys = new ImpObservableCollection<string>();
            _refdic = new Dictionary<int, int>();
            InitializeSelectedModules();
            _category = "Combination";

            foreach (var cm in ModuleInfoMgr.CombinationModuleDefs)
            {
                if (!Categorys.Contains(cm.Category))
                    Categorys.Add(cm.Category);
            }
        }


        public void InitializeSelectedModules()
        {
            if (Macro.Modules.Count(md => md.IsSelected) == 0) return;

            var l = double.MaxValue;
            var pMinDistance = new Point(double.MaxValue, double.MaxValue);

            foreach (var m in Macro.Modules.Where(md => md.IsSelected))
            {
                if (m.X < pMinDistance.X)
                    pMinDistance.X = m.X;

                if (m.Y < pMinDistance.Y)
                    pMinDistance.Y = m.Y;

            }

            var pOffset = new Point(230.0, 30.0);

            foreach (var m in Macro.Modules.Where(md => md.IsSelected))
            {
                var mc = (ModuleBase)m.Clone();
                mc.Id = GetMyModuleId();
                mc.X = mc.X - (int)pMinDistance.X + (int)pOffset.X;
                mc.Y = mc.Y - (int)pMinDistance.Y + (int)pOffset.Y;
                _refdic[m.Id] = mc.Id;
                AsIsModules.Add(mc);
            }


            foreach (var m in Macro.Modules.Where(md => md.IsSelected))
            {
                foreach (var c in m.OutputPort.AttachedConnections)
                {
                    if (Macro.Modules.Where(md => md.IsSelected).Contains(c.DestPort.ParentModule))
                    {
                        var outMod = m;
                        var inMod = c.DestPort.ParentModule;
                        var inportIdx = inMod.InputPorts.IndexOf(c.DestPort);
                        AsIsConnections.Add(CreateConnector(_refdic[inMod.Id], _refdic[outMod.Id], inportIdx, 0));
                    }
                }
            }

        }

        private int GetMyModuleId()
        {
            var ret = 0;
            while (AsIsModules.Any(m => m.Id == ret))
            {
                ret++;
            }

            return ret;
        }

        private ConnectorModel CreateConnector(int inPortId, int outPortId, int inPortIndex, int outPortIndex)
        {
            ModuleBase inModule = null;
            ModuleBase outModule = null;

            foreach (var module in AsIsModules)
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
                return null;
            }
            return new ConnectorModel(outModule.OutputPort, inModule.InputPorts[inPortIndex]);
        }

        private bool checkCategoryName()
        {
            if (Category.Trim() == string.Empty)
            {
                MessageBox.Show(Application.Current.MainWindow, "Template Category can not be empty, please check!", "Save Module Template", MessageBoxButton.OK);
                return false;
            }

            if (Name.Trim() == string.Empty)
            {
                MessageBox.Show(Application.Current.MainWindow, "Template Name can not be empty, please check!", "Save Module Template", MessageBoxButton.OK);
                return false;
            }

            if (Name.Trim() == Category)
            {
                MessageBox.Show(Application.Current.MainWindow, "Template Name [" + Name.Trim() + "] can not as same as category name, please check!", "Save Module Template", MessageBoxButton.OK);
                return false;
            }

            var isNameduplicate = ModuleInfoMgr.CombinationModuleDefs.Where(cm => cm.Category == Category).Any(cm => cm.Name == Name.Trim());

            if (isNameduplicate)
            {
                MessageBox.Show(Application.Current.MainWindow, "Duplicate Template name [" + Name.Trim() + "] in the same category [" + Category + "], please check!", "Save Module Template", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void OnSave(object obj)
        {
            //Do some save action.
            try
            {
                if (!checkCategoryName()) return;
                var combMod = new CombinationModule(_name, new List<ModuleBase>(AsIsModules.ToArray()));
                combMod.Category = Category;
                combMod.CaptionString = SelectedModCaption;
                combMod.Comment = Comment;
                ModuleInfoMgr.CombinationModuleDefs.Add(combMod);
                ModuleInfoMgr.Instance.SaveCombinationModuleTemplates();
                MessageHelper.SetMacroTemplateUpdated();
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("Error occoured in SaveTemplateViewModule.OnSave", ex);
                MessageBox.Show(Application.Current.MainWindow, ex.Message, "Save Module Template", MessageBoxButton.OK);
                return;
            }
            MessageHelper.PostMessage(string.Format("Save Template {0} Complete!", _name));
            CloseParentDialog(true);
        }

        private void OnCancel(object obj)
        {
            //Do cancel action.
            CloseParentDialog(false);
        }


        private void CloseParentDialog(bool result)
        {
            AsIsModules.Clear();
            AsIsConnections.Clear();
            _refdic.Clear();

            var p = LogicalTreeHelper.GetParent(_viewObj);
            var pw = p as Window;
            if (pw == null) return;
            pw.DialogResult = result;
            pw.Close();
        }
    }
}
