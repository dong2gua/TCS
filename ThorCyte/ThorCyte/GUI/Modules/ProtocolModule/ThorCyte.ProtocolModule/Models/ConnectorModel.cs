using System;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ImageProcess;
using Prism.Mvvm;

namespace ThorCyte.ProtocolModule.Models
{
    /// <summary>
    /// Defines a _connection between two connectors (aka _connection points) of two _nodes.
    /// </summary>
    public sealed class ConnectorModel : BindableBase
    {
        #region Events
        /// <summary>
        /// Event fired when the connection has changed.
        /// </summary>
        public event EventHandler<EventArgs> ConnectionChanged;

        #endregion

        #region Properties and Fields
        /// <summary>
        /// The source connector the _connection is attached to.
        /// </summary>
        private PortModel _sourcePort;

        public PortModel SourcePort
        {
            get { return _sourcePort; }
            set
            {
                if (_sourcePort == value)
                {
                    return;
                }

                if (_sourcePort != null)
                {
                    _sourcePort.AttachedConnections.Remove(this);
                    _sourcePort.HotspotUpdated -= SourcePort_HotspotUpdated;
                }

                _sourcePort = value;

                if (_sourcePort != null)
                {
                    ConnectionBrush = value.PortBrush;
                    _sourcePort.AttachedConnections.Add(this);
                    _sourcePort.HotspotUpdated += SourcePort_HotspotUpdated;
                    SourcePortHotspot = _sourcePort.HotSpot;
                }

                OnPropertyChanged("SourcePort");
            }
        }

        private Brush _connectionBrush = Brushes.Black;

        public Brush ConnectionBrush
        {
            get { return _connectionBrush; }
            set
            {
                if (Equals(_connectionBrush, value))
                {
                    return;
                }
                SetProperty(ref _connectionBrush, value);
            }
        }

        /// <summary>
        /// The destination connector the _connection is attached to.
        /// </summary>
        private PortModel _destPort;

        public PortModel DestPort
        {
            get { return _destPort; }
            set
            {
                if (_destPort == value)
                {
                    return;
                }
                
                //Former dest port
                if (_destPort != null)
                {
                    _destPort.AttachedConnections.Remove(this);
                    _destPort.HotspotUpdated -= DestPort_HotspotUpdated;
                }

                //Latter dest port
                _destPort = value;

                if (_destPort != null)
                {
                    _destPort.AttachedConnections.Add(this);
                    _destPort.HotspotUpdated += DestPort_HotspotUpdated;
                    DestPortHotspot = _destPort.HotSpot;
                }

                OnPropertyChanged("DestPort");
                OnConnectionChanged();
            }
        }

        /// <summary>
        /// The source and dest hotspots used for generating _connection points.
        /// </summary>
        private Point _sourcePortHotspot;

        public Point SourcePortHotspot
        {
            get { return _sourcePortHotspot; }
            set
            {
                SetProperty(ref _sourcePortHotspot, value);
            }
        }

        private Point _destPortHotspot;

        public Point DestPortHotspot
        {
            get { return _destPortHotspot; }
            set
            {
                SetProperty(ref _destPortHotspot, value);
            }
        }

        #endregion

        #region Methods

        public ConnectorModel(PortModel pSource,PortModel pDest)
        {
            SourcePort = pSource;
            DestPort = pDest;
        }

        public ConnectorModel()
        {
        }

        /// <summary>
        /// Raises the 'ConnectionChanged' event.
        /// </summary>
        private void OnConnectionChanged()
        {
            if (ConnectionChanged != null)
            {
                ConnectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when the hotspot of the source connector has been updated.
        /// </summary>
        private void SourcePort_HotspotUpdated(object sender, EventArgs e)
        {
            SourcePortHotspot = SourcePort.HotSpot;
        }

        /// <summary>
        /// Event raised when the hotspot of the dest connector has been updated.
        /// </summary>
        private void DestPort_HotspotUpdated(object sender, EventArgs e)
        {
            DestPortHotspot = DestPort.HotSpot;
        }

        public void TransferExecute()
        {
            //pass the ref to dest port.
            _destPort.Image = _sourcePort.Image;
            _destPort.ComponentName = _sourcePort.ComponentName;
            _destPort.ParentModule.Execute();
        }

        public void TransferExecute(ImageData img)
        {
            _destPort.Image = img.Clone();
            _destPort.ComponentName = _sourcePort.ComponentName;
            _destPort.ParentModule.Execute();
        }

        /// <summary>
        /// Serialize the connection object.
        /// </summary>
        /// <param name="writer">xml writer</param>
        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("connector");
            writer.WriteAttributeString("outport-module-id", SourcePort.ParentModule.Id.ToString());
            writer.WriteAttributeString("outport-index", 0.ToString()); //Output port alaways 1, so index of source port is alaways 0.
            writer.WriteAttributeString("inport-module-id", DestPort.ParentModule.Id.ToString());
            writer.WriteAttributeString("inport-index", DestPort.ParentModule.InputPorts.IndexOf(DestPort).ToString());
            writer.WriteEndElement();	// </connector>
        }

        public static ConnectorModel Create(PortModel p1, PortModel p2)
        {
            return new ConnectorModel(p1,p2);
        }

        public void RemoveFromPorts()
        {
            DestPort.AttachedConnections.Remove(this);
            SourcePort.AttachedConnections.Remove(this);
        }


        #endregion Private Methods

    }
}
