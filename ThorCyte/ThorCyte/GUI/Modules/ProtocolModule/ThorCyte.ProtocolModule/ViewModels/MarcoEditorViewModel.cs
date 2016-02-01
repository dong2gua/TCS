using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;

namespace ThorCyte.ProtocolModule.ViewModels
{
    /// <summary>
    /// The view-model for the main window.
    /// </summary>
    public class MarcoEditorViewModel : BindableBase
    {
        #region Properties
        public List<int> SelectModuleOrder = new List<int>(); 

        public ICommand SaveMacroCommand { get; private set; }
        public ICommand MacroCommnad { get; private set; }
        public ICommand AlignCommnad { get; private set; }

        private static MarcoEditorViewModel _mainWindowWm = new MarcoEditorViewModel();

        public static MarcoEditorViewModel Instance
        {
            get { return _mainWindowWm; }
        }

        /// <summary>
        /// This is the PannelVm that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private PannelViewModel _pannelVm;

        public PannelViewModel PannelVm
        {
            get { return _pannelVm; }
            set { SetProperty(ref _pannelVm, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        private int _totalProgress;
        public int TotalProgress
        {
            get { return _totalProgress; }
            set { SetProperty(ref _totalProgress, value); }
        }

        private int _regionProgress;
        public int RegionProgress
        {
            get { return _regionProgress; }
            set { SetProperty(ref _regionProgress, value); }
        }

        private int _regionCount;
        public int RegionCount
        {
            get { return _regionCount; }
            set { SetProperty(ref _regionCount, value); }
        }

        private int _tileCount;
        public int TileCount
        {
            get { return _tileCount; }
            set { SetProperty(ref _tileCount, value); }
        }

        private bool _isRuning;
        public bool IsRuning
        {
            get { return _isRuning; }
            set { SetProperty(ref _isRuning, value); }
        }

        private string _imgSource;
        public string ImgSource
        {
            get { return _imgSource; }
            set { SetProperty(ref _imgSource, value); }
        }

        private string _tipStr;
        public string TipStr
        {
            get { return _tipStr; }
            set { SetProperty(ref _tipStr, value); }
        }

        #endregion

        #region Contructors

        private MarcoEditorViewModel()
        {
            MessageHelper.SetMessage += SetMessage;
            MessageHelper.SetProgress += SetProgress;
            MessageHelper.SetRuning += SetRuning;
            MacroCommnad = new DelegateCommand(SetMacroCommand);
            SaveMacroCommand = new DelegateCommand(Macro.Save);

            AlignCommnad = new DelegateCommand<string>(SetModulesAlign);

            _pannelVm = new PannelViewModel();

            RegionCount = 10;
            TileCount = 10;

            _imgSource = IsRuning ? "../Resource/Images/stop.png" : "../Resource/Images/play.png";
            _tipStr = IsRuning ? "Stop Run" : "Start Run";
        }

        private void SetModulesAlign(string alnType)
        {
            try
            {
                int vx;
                int vy;

                if (SelectModuleOrder.Count == 0) return;

                if (PannelVm.Modules.Any(md => md.IsSelected))
                {
                    var module = PannelVm.Modules.FirstOrDefault(md => md.Id == SelectModuleOrder[0]);
                    if (module == null) return;
                    vx = module.X;
                    vy = module.Y;
                }
                else
                {
                    return;
                }
                
                foreach (var m in PannelVm.Modules.Where(md => md.IsSelected))
                {
                    switch (alnType)
                    {
                        case "V":
                            m.Y = vy;
                            break;

                        case "H":
                            m.X = vx;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error occurred in SetModulesAlign" + ex.Message);
            }
        }

        #endregion

        #region Methods

        private void SetMacroCommand()
        {
            if (IsRuning)
            {
                Macro.Stop();
            }
            else
            {
                Macro.Run();
            }
        }

        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new _connection.
        /// </summary>
        public ConnectorModel ConnectionDragStarted(PortModel draggedOutPort, Point curDragPoint)
        {
            // Create a new connection to add to the view-model.

            var connection = new ConnectorModel();

            if (draggedOutPort.PortType == PortType.InPort && draggedOutPort.AttachedConnections.Count > 0)
            {
                //1 connetion at most
                connection = draggedOutPort.AttachedConnections[0];
                PannelVm.Connections.Remove(connection);
                connection.DestPortHotspot = curDragPoint;
                PannelVm.Connections.Add(connection);
            }
            else if (draggedOutPort.PortType == PortType.OutPort)
            {
                // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                connection.SourcePort = draggedOutPort;
                connection.DestPortHotspot = curDragPoint;
                PannelVm.Connections.Add(connection);
            }

            // Add the new connection to the view-model.
            return connection;
        }

        /// <summary>
        /// Called as the user continues to drag the _connection.
        /// </summary>
        public void ConnectionDragging(ConnectorModel connection, Point curDragPoint)
        {
            // Update the destination _connection hotspot while the user is dragging the _connection.
            connection.DestPortHotspot = curDragPoint;
        }

        /// <summary>
        /// determine whether connectable between startPort port and endPort port
        /// </summary>
        private bool IsConnectable(PortModel startPort, PortModel endPort)
        {
            var isConnect = false;

            if (endPort == null)
            {
                return false;
            }

            if (endPort.DataType == PortDataType.Image)
            {
                if (startPort.DataType == PortDataType.BinaryImage || startPort.DataType == PortDataType.GrayImage
                    || startPort.DataType == PortDataType.MultiChannelImage || startPort.DataType == PortDataType.Image)
                {
                    if (!HaveConnetion(endPort))
                    {
                        isConnect = true;
                    }
                }
            }
            else
            {
                if (startPort.DataType == endPort.DataType)
                {
                    if (!HaveConnetion(endPort))
                    {
                        isConnect = true;
                    }
                }
            }
            return isConnect;
        }

        /// <summary>
        /// Judge Connections contain endPort,one ModuleVmBase only one Output
        /// </summary>
        public bool HaveConnetion(PortModel endPort)
        {
            return PannelVm.Connections.Any(connection => connection.DestPortHotspot == endPort.HotSpot);
        }

        /// <summary>
        /// Called when the user has finished dragging out the new _connection.
        /// </summary>
        public void ConnectionDragCompleted(ConnectorModel newConnection, PortModel portDraggedOut, PortModel portDraggedOver)
        {
            if (portDraggedOut.PortType == PortType.InPort) portDraggedOut = newConnection.SourcePort;
            
            if (!IsConnectable(portDraggedOut, portDraggedOver))
            {
                // The connection was unsuccessful. Maybe the user dragged it out and dropped it in empty space.
                PannelVm.Connections.Remove(newConnection);
                newConnection.SourcePort = null;
                newConnection.DestPort = null;
                return;
            }

            // Only allow connections from output connector to input connector (ie each connector must have a different Type).
            // Also only allocation from one node to another, never one node back to the same node.
            bool connectionOk = IsConnectionOK(newConnection, portDraggedOut, portDraggedOver);

            if (!connectionOk)
            {
                // Connections between connectors that have the same Type,eg input -> input or output -> output, are not allowed,
                // Remove the connection.
                PannelVm.Connections.Remove(newConnection);
                newConnection.SourcePort = null;
                newConnection.DestPort = null;
                return;
            }

            // The user has dragged the connection on top of another valid connector.Remove any existing connection between the same two connectors.
            var existingConnection = FindConnection(newConnection, portDraggedOut, portDraggedOver);

            if (existingConnection != null)
            {
                return;
            }

            // Finalize the connection by attaching it to the connector that the user dragged the mouse over.
            newConnection.DestPort = portDraggedOver;

        }

        private bool IsConnectionOK(ConnectorModel newConnection, PortModel portDraggedOut, PortModel portDraggedOver)
        {
            var res = true;

            if (newConnection.SourcePort == null) return false;

            switch (portDraggedOut.PortType)
            {
                case PortType.InPort:
                    res = newConnection.SourcePort.ParentModule != portDraggedOver.ParentModule && newConnection.SourcePort.PortType != portDraggedOver.PortType;
                    break;
                case PortType.OutPort:
                    res = portDraggedOut.ParentModule != portDraggedOver.ParentModule && portDraggedOut.PortType != portDraggedOver.PortType;
                    break;
                case PortType.None:
                    res = false;
                    break;
            }

            return res;
        }

        /// <summary>
        /// Delete the currently selected _nodes from the view-model.
        /// </summary>
        public void DeleteSelectedModules()
        {
            // Take a copy of the _nodes list so we can delete _nodes while iterating.
            var modulesCopy = PannelVm.Modules.ToArray();

            foreach (var module in modulesCopy)
            {
                if (module.IsSelected)
                {
                    DeleteModule(module);
                }
            }
        }

        /// <summary>
        /// Delete the moduleVm from the view-model.Also deletes any connections to or from the moduleVm.
        /// </summary>
        public void DeleteModule(ModuleBase moduleVm)
        {
            // Remove all connections attached to the moduleVm.
            PannelVm.Connections.RemoveRange(moduleVm.AttachedConnections);
            foreach (var c in moduleVm.AttachedConnections)
            {
                c.DestPort = null;
                c.SourcePort = null;
            }
            // Remove the moduleVm from the PannelVm.
            PannelVm.Modules.Remove(moduleVm);
        }


        public ModuleBase GetSelectedModule()
        {
            ModuleBase module = null;
            var count = 0;
            for (var i = PannelVm.Modules.Count - 1; i >= 0; i--)
            {
                if (PannelVm.Modules[i].IsSelected)
                {
                    count++;
                    module = PannelVm.Modules[i];
                }
            }

            if (count == 1)
            {
                SelectModuleOrder.Clear();
                SelectModuleOrder.Add(module.Id);
            }

            return 1 == count ? module : null;
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.Returns null if there is no connection between the connectors.
        /// </summary>
        public ConnectorModel FindConnection(ConnectorModel newConnection, PortModel port1, PortModel port2)
        {
            Trace.Assert(port1.PortType != port2.PortType);

            // Figure out which one is the source connector and which one is the destination connector based on their connector types.
            var sourcePort = port1.PortType == PortType.OutPort ? port1 : port2;
            var destPort = port1.PortType == PortType.OutPort ? port2 : port1;

            // Now we can just iterate attached connections of the source and see if it each one is attached to the destination connector.
            return sourcePort.AttachedConnections.FirstOrDefault(connection => connection.DestPort == destPort);
        }

        private void SetMessage(string msg)
        {
            StatusMessage = msg;
        }

        private void SetProgress(string type, int max, int value)
        {
            switch (type)
            {
                case "Region":
                    if (max > 0)
                        RegionCount = max;
                    TotalProgress = value;

                    break;
                case "Tile":
                    if (max > 0)
                        TileCount = max;
                    RegionProgress = value;

                    break;
            }

        }

        private void SetRuning(bool isRuning)
        {
            IsRuning = isRuning;
            ImgSource = isRuning ? "../Resource/Images/stop.png" : "../Resource/Images/play.png";
            TipStr = isRuning ? "Stop Run" : "Start Run";
        }

        #endregion
    }
}
