using System.Windows;
using System.Windows.Controls;

namespace ThorCyte.ProtocolModule.Controls
{
    /// <summary>
    /// Implements an ListBox for displaying _nodes in the NetworkView UI.
    /// </summary>
    internal class ModuleItemsControl : ListBox
    {
        #region Constructors

        public ModuleItemsControl()
        {
            //By default, we don't want this UI element to be focusable.
            Focusable = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find the ModuleVmBase UI element that has the specified data context.
        /// Return null if no such ModuleVmBase exists.
        /// </summary>
        internal Module FindAssociatedModule(object moduleDataContext)
        {
            return (Module)ItemContainerGenerator.ContainerFromItem(moduleDataContext);
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item. 
        /// </summary>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Module();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container. 
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Module;
        }

        #endregion
    }
}
