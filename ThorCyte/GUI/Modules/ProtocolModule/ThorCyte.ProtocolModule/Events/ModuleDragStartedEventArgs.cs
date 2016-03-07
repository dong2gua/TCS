using System.Collections;
using System.Windows;

namespace ThorCyte.ProtocolModule.Events
{
    /// <summary>
    /// Arguments for event raised when the user starts to drag a _module in the PannelVm.
    /// </summary>
    public class ModuleDragStartedEventArgs : ModuleDragEventArgs
    {
        #region Properties and Fields

        public bool Cancel { get; set; }

        #endregion

        #region Constructors

        internal ModuleDragStartedEventArgs(RoutedEvent routedEvent, object source, ICollection nodes) :
            base(routedEvent, source, nodes)
        {
        }

        #endregion
    }
}
