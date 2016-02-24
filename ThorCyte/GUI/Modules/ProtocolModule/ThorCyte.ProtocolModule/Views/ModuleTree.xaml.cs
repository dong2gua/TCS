using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.Infrastructure.Events;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels;

namespace ThorCyte.ProtocolModule.Views
{
    /// <summary>
    /// Interaction logic for ModuleTree.xaml
    /// </summary>
    public partial class ModuleTree
    {
        private const string DefaultKeyword = "Find...";
        private string _searchKeyword;
        private Style _baseStyle;

        private IEventAggregator _eventAggregator;
        public IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        public ModuleTree()
        {
            InitializeComponent();
            MessageHelper.UnSelectViewItem += UnSelectViewItem;
            DataContext = new ModuleTreeViewModel();
            SerTb.Text = DefaultKeyword;
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
            ExpLoaded(0);
        }


        private void ExpLoaded(int scanid)
        {
            try
            {
                ClearSearch();
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("Error occurred in MacroEditor.ExpLoaded",ex);
            }
        }


        private void SerchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox == null) return;

            if (searchBox.Text == DefaultKeyword && Equals(searchBox.Foreground, Brushes.LightGray))
            {
                searchBox.Text = string.Empty;
                searchBox.Foreground = Brushes.LightYellow;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox == null) return;

            if (searchBox.Text == string.Empty)
            {
                searchBox.Text = DefaultKeyword;
                searchBox.Foreground = Brushes.LightGray;
            }
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            _searchKeyword = ((TextBox)sender).Text;
            SetModuleSelection(_searchKeyword);
        }

        public TreeViewItemModel TreeSelectedItem;

        private void OnTreeviewSelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            if (tree != null)
            {
                TreeSelectedItem = tree.SelectedItem as TreeViewItemModel;

                if (TreeSelectedItem != null)
                {
                    MessageHelper.SetSelectItem(TreeSelectedItem);
                }
            }
        }

        private void UnSelectViewItem(object item)
        {
            if (treeview.SelectedItem != null)
            {
                TreeSelectedItem.IsSelected = false;
                MessageHelper.SetSelectItem(TreeSelectedItem);
            }
        }

        private void SetModuleSelection(string moduleKeyWord)
        {
            //collpse tree view 
            ExpandTree(treeview, false);
            ((ModuleTreeViewModel)DataContext).FilterModuleInfo(moduleKeyWord);
            if (moduleKeyWord == string.Empty)
            {
                ExpandTree(treeview, false);
                return;
            }
            //Expand treeview
            ExpandTree(treeview, true);
        }

        public void ClearSearch()
        {
            SerTb.Text = string.Empty;
            SetModuleSelection(SerTb.Text);
            SearchBox_LostFocus(SerTb, new RoutedEventArgs());
        }

        /// <summary>
        /// Collapse or Expand treeview 
        /// </summary>
        /// <param name="treeContainer">treeview need to operate</param>
        /// <param name="mode">true--Expand false--Collapse</param>
        private void ExpandTree(ItemsControl treeContainer, bool mode)
        {

            _baseStyle = (Style) FindResource(typeof (TreeViewItem));

            if (mode)
            {
                var inStyle = new Style
                {
                    TargetType = typeof(TreeViewItem),
                    BasedOn = _baseStyle
                };

                inStyle.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty, mode));
                treeContainer.ItemContainerStyle = inStyle;
            }
            else
            {
                treeContainer.ItemContainerStyle = _baseStyle;
            }
        }



    }
}
