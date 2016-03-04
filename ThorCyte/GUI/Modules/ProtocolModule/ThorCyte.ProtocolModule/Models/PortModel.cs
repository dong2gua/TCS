using System;
using System.Windows;
using System.Windows.Media;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Utils;
using System.Linq;
using ImageProcess;
using ThorCyte.ProtocolModule.ViewModels.Modules;

namespace ThorCyte.ProtocolModule.Models
{
    /// <summary>
    /// Defines a connector (aka _connection point) that can be attached to a module and is used to connect the module to another module.
    /// </summary>
    public sealed class PortModel : BindableBase
    {
        #region Events
        /// <summary>
        /// Event raised when the connector _hotSpot has been updated.
        /// </summary>
        public event EventHandler<EventArgs> HotspotUpdated;

        #endregion

        #region Properties and Fields

        /// <summary>
        /// The _connection that is attached to this connector, or null if no _connection is attached.
        /// </summary>
        private ImpObservableCollection<ConnectorModel> _attachedConnections;

        public ImpObservableCollection<ConnectorModel> AttachedConnections
        {
            get
            {
                if (_attachedConnections == null)
                {
                    _attachedConnections = new ImpObservableCollection<ConnectorModel>();
                    _attachedConnections.ItemsAdded += attachedConnections_ItemsAdded;
                    _attachedConnections.ItemsRemoved += attachedConnections_ItemsRemoved;
                }

                return _attachedConnections;
            }
        }

        public ModuleBase ParentModule { get; set; }


        public bool DataExists
        {
            get
            {
                bool res;
                switch (DataType)
                {
                    case PortDataType.None:
                        res = true;
                        break;

                    case PortDataType.Event:
                        if (ComponentName == null)
                        {
                            res = false;
                        }
                        else
                        {
                            res = ComponentName.Trim() != string.Empty;
                        }
                        break;

                    case PortDataType.BinaryImage:
                    case PortDataType.GrayImage: 
                    case PortDataType.Image:
                    case PortDataType.MultiChannelImage:
                        if (Image != null && !Image.IsClosed)
                        {
                            res = true;
                        }
                        else
                        {
                            res = false;
                        }
                        break;

                    case PortDataType.Setting:
                        res = true;
                        break;

                    case PortDataType.Flag:
                        res = true;
                        break;

                    default :
                        res = false;
                        break;
                }
                return res;
            }
        }

        /// <summary>
        /// Returns 'true' if the connector connected to another node.
        /// </summary>
        public bool IsConnected
        {
            get { return AttachedConnections.Any(connection => connection.SourcePort != null && connection.DestPort != null); }
        }

        /// <summary>
        /// Returns 'true' if a connection is attached to the connector.
        /// The other end of the connection may or may not be attached to a node.
        /// </summary>
        public bool IsConnectionAttached
        {
            get { return AttachedConnections.Count > 0; }
        }

        /// <summary>
        /// The _hotSpot (or center) of the connector. This is pushed through from Port in the UI.
        /// </summary>
        private Point _hotSpot;

        public Point HotSpot
        {
            get { return _hotSpot; }
            set
            {
                if (_hotSpot == value)
                {
                    return;
                }
                _hotSpot = value;
                OnHotspotUpdated();
            }
        }

        public ImageData Image { get; set; }
        public string ComponentName { get; set; }


        private Brush _portBrush = Brushes.LightGray;

        public Brush PortBrush
        {
            get { return _portBrush; }
            set
            {
                if (Equals(_portBrush, value))
                {
                    return;
                }

                SetProperty(ref _portBrush, value);
            }
        }

        public PortType PortType { get; set; }

        private PortDataType _portDataType;

        public PortDataType DataType
        {
            get { return _portDataType; }
            set
            {
                _portDataType = value;
                SetPortBrush(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Debug checking to ensure that no connection is added to the list twice.
        /// </summary>
        private void attachedConnections_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorModel connection in e.Items)
            {
                connection.ConnectionChanged += connection_ConnectionChanged;
            }

            if ((AttachedConnections.Count - e.Items.Count) == 0)
            {
                // The first connection has been added, notify the data-binding system that
                // 'IsConnected' should be re-evaluated.
                OnPropertyChanged("IsConnectionAttached");
                OnPropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// Event raised when connections have been removed from the connector.
        /// </summary>
        private void attachedConnections_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorModel connection in e.Items)
            {
                connection.ConnectionChanged -= connection_ConnectionChanged;
            }

            if (AttachedConnections.Count == 0)
            {
                // No longer connected to anything, notify the data-binding system that
                // 'IsConnected' should be re-evaluated.
                OnPropertyChanged("IsConnectionAttached");
                OnPropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// Event raised when a connection attached to the connector has changed.
        /// </summary>
        private void connection_ConnectionChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("IsConnectionAttached");
            OnPropertyChanged("IsConnected");
        }

        private void OnHotspotUpdated()
        {
            OnPropertyChanged("_hotSpot");
            if (HotspotUpdated != null)
            {
                HotspotUpdated(this, EventArgs.Empty);
            }
        }

        public PortModel(ModuleBase parent, PortType type = PortType.None, PortDataType dataType = PortDataType.None)
        {
            ParentModule = parent;
            PortType = type;
            _portDataType = dataType;
            SetPortBrush(dataType);
        }

        public PortModel(PortType type = PortType.None, PortDataType dataType = PortDataType.None)
        {
            PortType = type;
            _portDataType = dataType;
            SetPortBrush(dataType);
        }

        private void SetPortBrush(PortDataType dataType = PortDataType.None)
        {
            switch (dataType)
            {
                case PortDataType.None:
                    _portBrush = Brushes.LightGray;
                    break;
                case PortDataType.BinaryImage:
                    _portBrush = Brushes.Black;
                    break;
                case PortDataType.GrayImage:
                    _portBrush = Brushes.Blue;
                    break;
                case PortDataType.MultiChannelImage:
                    _portBrush = Brushes.Cyan;
                    break;
                case PortDataType.Image: // Binary || Gray || MultiChannel
                    _portBrush = Brushes.Navy;
                    break;
                case PortDataType.Setting:
                    _portBrush = Brushes.Yellow;
                    break;
                case PortDataType.Event:
                    _portBrush = Brushes.Red;
                    break;
            }
        }

        public void Clear()
        {
            Image = null;
            ComponentName = string.Empty;
        }

        #endregion
    }
}
