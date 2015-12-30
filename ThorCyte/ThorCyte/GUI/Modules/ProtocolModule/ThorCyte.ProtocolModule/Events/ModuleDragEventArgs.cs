using System.Collections;
using System.Windows;

namespace ThorCyte.ProtocolModule.Events
{
    #region Events

    /// <summary>
    /// Defines the event handler for ModuleDragStarted events.
    /// </summary>
    public delegate void NodeDragEventHandler(object sender, ModuleDragEventArgs e);

    /// <summary>
    /// Defines the event handler for ModuleDragStarted events.
    /// </summary>
    public delegate void NodeDraggingEventHandler(object sender, ModuleDraggingEventArgs e);

    /// <summary>
    /// Defines the event handler for ModuleDragCompleted events.
    /// </summary>
    public delegate void NodeDragCompletedEventHandler(object sender, ModuleDragCompletedEventArgs e);

    /// <summary>
    /// Defines the event handler for ModuleDragStarted events.
    /// </summary>
    public delegate void NodeDragStartedEventHandler(object sender, ModuleDragStartedEventArgs e);

    #endregion

    /// <summary>
    /// Base class for _module dragging event args.
    /// </summary>
    public class ModuleDragEventArgs : RoutedEventArgs
    {
        #region Properties and Fields

        private readonly ICollection _nodes;

        /// <summary>
        /// The ModuleVmBase's or their DataContext (when non-NULL).
        /// </summary>
        public ICollection Nodes
        {
            get
            {
                return _nodes;
            }
        }

        #endregion

        #region Constructors

        protected ModuleDragEventArgs(RoutedEvent routedEvent, object source, ICollection nodes) :
            base(routedEvent, source)
        {
            _nodes = nodes;
        }

        #endregion
    }
}
