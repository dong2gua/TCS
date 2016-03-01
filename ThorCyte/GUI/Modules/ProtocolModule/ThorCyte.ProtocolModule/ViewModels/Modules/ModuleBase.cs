using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public abstract class ModuleBase : BindableBase, ICloneable
    {
        #region Properties

        private static int _modIdCount;

        public int ScanNo { get; set; }
        public ModuleType ModType { get; set; }

        public abstract bool Executable { get; }

        private static IEventAggregator _eventAggregator;
        private static IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get
            {
                return _displayName.Trim() == string.Empty ? _name : _displayName;
            }
            set
            {
                if (_displayName == value) return;
                SetProperty(ref _displayName, value);
            }
        }


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
                _view.DataContext = this;
            }
        }

        private ImageData _inputImage;
        public ImageData InputImage
        {
            get { return _inputImage; }
        }

        private bool _hasImage;
        public bool HasImage
        {
            get { return _hasImage; }
            set
            {
                if (_hasImage == value) return;
                SetProperty(ref _hasImage, value);
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private PortModel _outputPort;
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
                if (value < 0) _x = 0;
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
                if (value < 0) _y = 0;
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

        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (value > _modIdCount) _modIdCount = value;                
                if (_id == value) return;
                SetProperty(ref _id, value);
            }
        }

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

        public virtual string CaptionString { get; set; }

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
        public bool OutPortExists
        {
            get { return _outputPort != null; }
        }

        #endregion

        #region Constructors

        static ModuleBase()
        {
            _modIdCount = 0;
        }

        protected ModuleBase()
        {
            EventAggregator.GetEvent<ShowRegionEvent>().Subscribe(ShowRegionEventHandler, ThreadOption.UIThread, true);
            _outputPort = new PortModel(PortType.OutPort);
            _enabled = true;
            _hasImage = false;
        }

        #endregion

        #region Virtual Methods and Properties
        public abstract object Clone();

        public virtual void OnSerialize(XmlWriter writer) { }
        public virtual void OnDeserialize(XmlReader reader) { }
        public virtual void OnExecute() { }
        public virtual void Initialize() { }
        public virtual void Refresh() { }
        public virtual void InitialRun() { }
        public virtual void UpdateChannels() { }
        #endregion

        #region Methods
        private void ShowRegionEventHandler(string moduleName)
        {
            try
            {
                switch (moduleName)
                {
                    case "ReviewModule":
                        break;

                    case "ProtocolModule":
                    case "AnalysisModule":
                        UpdateChannels();
                        break;
                }
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("Error Occurred in Module ShowRegionEventHandler", ex);
                MessageHelper.PostMessage("Error Occurred in Module ShowRegionEventHandler " + ex.Message);
            }
        }

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
            if (!Enabled) return;

            if (_inputPorts.Any(p => !p.DataExists))
            {
                Debug.WriteLine("Data not exist return execute");
                return;
            }

            try
            {

                if (_hasImage)    // set the input image from the first image input port
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
                //determine how many connections attached on the out port.
                ConnectionsTransfer();
            }
        }

        private void ConnectionsTransfer()
        {
            switch (_outputPort.AttachedConnections.Count)
            {
                case 0:
                    if (_outputPort.Image == null) return;
                    _outputPort.Image.Dispose();
                    break;
                case 1:
                    _outputPort.AttachedConnections[0].TransferExecute();
                    break;
                default:
                    foreach (var connection in _outputPort.AttachedConnections)
                    {
                        connection.TransferExecute(_outputPort.Image);
                    }
                    if (_outputPort.Image != null)
                        _outputPort.Image.Dispose();

                    break;
            }
        }


        public PortModel GetInPort(int index)
        {
            return _inputPorts.Count > index ? _inputPorts[index] : null;
        }

        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("module");
            writer.WriteAttributeString("id", Id.ToString());
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("enabled", _enabled.ToString().ToLower());
            writer.WriteAttributeString("x", _x.ToString());
            writer.WriteAttributeString("y", _y.ToString());
            writer.WriteAttributeString("scanId", ScanNo.ToString());

            OnSerialize(writer);
            writer.WriteEndElement();
        }

        public virtual void Deserialize(XmlReader reader)
        {
            OnDeserialize(reader);
        }

        protected void SetOutputImage(ImageData img)
        {
            if (img != null)
            {
                _outputPort.Image = img;
            }

        }

        protected void SetOutputComponent(string componetName)
        {
            if (componetName != string.Empty)
            {
                _outputPort.ComponentName = componetName;
            }

        }

        public static int GetNextModId()
        {
            try
            {
                return ++_modIdCount;
            }
            catch (OverflowException)
            {
                _modIdCount = 0;
                return _modIdCount;
            }

        }

        #endregion
    }
}
