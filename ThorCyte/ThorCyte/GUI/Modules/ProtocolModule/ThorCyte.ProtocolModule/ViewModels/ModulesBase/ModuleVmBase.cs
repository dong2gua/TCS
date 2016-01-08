using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;
using ImageProcess;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.ModulesBase
{
    public abstract class ModuleVmBase : BindableBase
    {
        #region Properties and Fields

        public int Id { get; set; }

        public int ScanNo { get; set; }

        public Macro ParentMacro { get; set; }

        public string DisplayName { get; set; }

        public virtual string CaptionString { get; set; }

        public ModuleType ModType { get; set; }

        private ContentControl _view;
        public ContentControl View
        {
            get { return _view; }
            set
            {
                if (Equals(value, _view))
                {
                    return;
                }
                SetProperty(ref _view, value);
            }
        }

        private ImageData _inputImage;

        public ImageData InputImage
        {
            get { return _inputImage; }
        }

        protected bool _hasImage = false;

        public bool HasImage
        {
            get { return _hasImage; }
        }

        public bool OutPortExists
        {
            get { return _outputPort != null; }
        }

        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        private PortModel _outputPort = new PortModel(PortType.OutPort);

        public PortModel OutputPort
        {
            get { return _outputPort; }
            set { _outputPort = value; }
        }

        private int _x;

        public int X
        {
            get { return _x; }
            set
            {
                if (_x == value)
                {
                    return;
                }
                SetProperty(ref _x, value);
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                if (_y == value)
                {
                    return;
                }
                SetProperty(ref _y, value);
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }
                SetProperty(ref _name, value);
            }
        }

        /// <summary>
        /// Set to 'true' when the module is selected.
        /// </summary>
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }
                SetProperty(ref _isSelected, value);
            }
        }

        /// <summary>
        /// A helper property that retrieves a list (a new list each time) of all connections attached to the module. 
        /// </summary>
        public ICollection<ConnectorModel> AttachedConnections
        {
            get
            {
                var attachedConnections = new List<ConnectorModel>();

                foreach (var port in InputPorts)
                {
                    attachedConnections.AddRange(port.AttachedConnections);
                }

                if (_outputPort != null)
                {
                    attachedConnections.AddRange(_outputPort.AttachedConnections);
                }

                return attachedConnections;
            }
        }

        /// <summary>
        /// List of input connectors (connections points) attached to the node.
        /// </summary>
        private ImpObservableCollection<PortModel> _inputPorts;

        public ImpObservableCollection<PortModel> InputPorts
        {
            get
            {
                if (_inputPorts == null)
                {
                    _inputPorts = new ImpObservableCollection<PortModel>
                    {
                        new PortModel(PortType.InPort),
                        new PortModel(PortType.InPort),
                        new PortModel(PortType.InPort),
                        new PortModel(PortType.InPort),
                        new PortModel(PortType.InPort)
                    };
                    _inputPorts.ItemsAdded += InputPorts_ItemsAdded;
                    _inputPorts.ItemsRemoved += InputPorts_ItemsRemoved;
                }
                return _inputPorts;
            }
        }

        #endregion

        #region Constructors

        protected ModuleVmBase()
        {
        }



        #endregion

        #region Methods
        public virtual void OnSerialize(XmlWriter writer) { }
        public virtual void OnDeserialize(XmlReader reader) { }
        public virtual void OnExecute() { }
        public virtual void Initialize() { }
        public virtual void Refresh() { }
        public virtual void OnSetScanNo() { }

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void InputPorts_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (PortModel port in e.Items)
            {
                port.PortType = PortType.InPort;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void InputPorts_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (PortModel port in e.Items)
            {
                port.PortType = PortType.None;
            }
        }


        public void Execute()
        {
            try
            {
                if (_hasImage)	// set the input image from the first image input port
                {
                    foreach (var port in _inputPorts)
                    {
                        if (port.Image != null)
                        {
                            _inputImage = port.Image;
                        }
                    }
                }
                OnExecute();
            }
            finally
            {
                // clear data on inport after execution
                foreach (var port in _inputPorts)
                {
                    port.Clear();
                }
            }

            if (OutPortExists)
            {
                foreach (var connector in _outputPort.AttachedConnections)
                {
                    connector.TransferExecute();
                }
            }
        }

        public PortModel GetInPort(int index)
        {
            return _inputPorts.Count > index ? _inputPorts[index] : null;
        }

        protected void SetOutputImage(ImageData img)
        {
            if (img != null)
            {
                _outputPort.Image = img;
            }
        }

        public void SetScanNo()
        {

        }

        public void Serialize(XmlWriter writer)
        {
            if (this is CombinationModVm)
                writer.WriteStartElement("combination-module");
            else
                writer.WriteStartElement("module");

            writer.WriteAttributeString("id", Id.ToString());
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("enabled", _enabled.ToString().ToLower());
            writer.WriteAttributeString("x", _x.ToString());
            writer.WriteAttributeString("y", _y.ToString());
            writer.WriteAttributeString("scale", ScanNo.ToString());

            OnSerialize(writer);
            writer.WriteEndElement();
        }

        public virtual void Deserialize(XmlReader reader)
        {
            OnDeserialize(reader);
        }


        #endregion
    }
}
