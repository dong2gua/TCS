using System.Collections;
using System.Windows;

namespace ThorCyte.ProtocolModule.Events
{
    /// <summary>
    /// Arguments for event raised when the user has completed dragging a _module in the PannelVm.
    /// </summary>
    public class ModuleDragCompletedEventArgs : ModuleDragEventArgs
    {
        #region Constructors

        public ModuleDragCompletedEventArgs(RoutedEvent routedEvent, object source, ICollection nodes) :
            base(routedEvent, source, nodes)
        {
        }

        #endregion
    }
}
