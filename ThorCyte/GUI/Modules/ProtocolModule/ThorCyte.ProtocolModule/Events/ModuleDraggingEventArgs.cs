using System.Collections;
using System.Windows;

namespace ThorCyte.ProtocolModule.Events
{
    /// <summary>
    /// Arguments for event raised while user is dragging a _module in the PannelVm.
    /// </summary>
    public class ModuleDraggingEventArgs : ModuleDragEventArgs
    {
        #region Properties and Fields

        /// <summary>
        /// The amount the _module has been dragged horizontally.
        /// </summary>
        private readonly double _horizontalChange;

        public double HorizontalChange
        {
            get { return _horizontalChange; }
        }

        /// <summary>
        /// The amount the _module has been dragged vertically.
        /// </summary>
        private readonly double _verticalChange;

        public double VerticalChange
        {
            get { return _verticalChange; }
        }

        #endregion

        #region Constructors

        internal ModuleDraggingEventArgs(RoutedEvent routedEvent, object source, ICollection nodes, double horizontalChange, double verticalChange) :
            base(routedEvent, source, nodes)
        {
            _horizontalChange = horizontalChange;
            _verticalChange = verticalChange;
        }

        #endregion
    }
}
