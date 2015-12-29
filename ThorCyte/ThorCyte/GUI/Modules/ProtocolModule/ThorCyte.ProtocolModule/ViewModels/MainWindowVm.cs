using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.ViewModels.Modules;

namespace ThorCyte.ProtocolModule.ViewModels
{
    /// <summary>
    /// The view-model for the main window.
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        #region Properties

        readonly List<ChannelModVm> _channelModuleList = new List<ChannelModVm>();

        private static MainWindowViewModel _mainWindowWm = new MainWindowViewModel();

        public static MainWindowViewModel Instance
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
            set
            {
                _pannelVm = value;
                RaisePropertyChanged("PannelVm");
            }
        }
        #endregion

        #region Contructors

        private MainWindowViewModel()
        {
            _pannelVm = new PannelViewModel();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new _connection.
        /// </summary>
        public ConnectorModel ConnectionDragStarted(PortModel draggedOutPort, Point curDragPoint)
        {
            // Create a new connection to add to the view-model.

            var connection = new ConnectorModel();
            if (draggedOutPort.PortType == PortType.OutPort)
            {
                // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                connection.SourcePort = draggedOutPort;
                connection.DestPortHotspot = curDragPoint;

            }
            else
            {
                // The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
                connection.DestPort = draggedOutPort;
                connection.SourcePortHotspot = curDragPoint;
            }
            PannelVm.Connections.Add(connection);
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
            if (!IsConnectable(portDraggedOut, portDraggedOver))
            {
                // The connection was unsuccessful. Maybe the user dragged it out and dropped it in empty space.
                PannelVm.Connections.Remove(newConnection);
                return;
            }

            // Only allow connections from output connector to input connector (ie each connector must have a different Type).
            // Also only allocation from one node to another, never one node back to the same node.
            bool connectionOk = portDraggedOut.ParentModule != portDraggedOver.ParentModule &&
                                portDraggedOut.PortType != portDraggedOver.PortType;

            if (!connectionOk)
            {
                // Connections between connectors that have the same Type,eg input -> input or output -> output, are not allowed,
                // Remove the connection.
                PannelVm.Connections.Remove(newConnection);
                return;
            }

            // The user has dragged the connection on top of another valid connector.Remove any existing connection between the same two connectors.
            var existingConnection = FindConnection(portDraggedOut, portDraggedOver);
            if (existingConnection != null)
            {
                PannelVm.Connections.Remove(existingConnection);
            }

            // Finalize the connection by attaching it to the connector that the user dragged the mouse over.
            if (newConnection.DestPort == null)
            {
                newConnection.DestPort = portDraggedOver;
            }
            else
            {
                newConnection.SourcePort = portDraggedOver;
            }
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
        public void DeleteModule(ModuleVmBase moduleVm)
        {
            // Remove all connections attached to the moduleVm.
            PannelVm.Connections.RemoveRange(moduleVm.AttachedConnections);

            // Remove the moduleVm from the PannelVm.
            PannelVm.Modules.Remove(moduleVm);
        }


        public ModuleVmBase GetSelectedModule()
        {
            ModuleVmBase moduleVmVm = null;
            int count = 0;
            for (var i = PannelVm.Modules.Count - 1; i >= 0; i--)
            {
                if (PannelVm.Modules[i].IsSelected)
                {
                    count++;
                    moduleVmVm = PannelVm.Modules[i];
                }
            }

            return 1 == count ? moduleVmVm : null;
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.Returns null if there is no connection between the connectors.
        /// </summary>
        public ConnectorModel FindConnection(PortModel port1, PortModel port2)
        {
            Trace.Assert(port1.PortType != port2.PortType);

            // Figure out which one is the source connector and which one is the destination connector based on their connector types.
            var sourcePort = port1.PortType == PortType.OutPort ? port1 : port2;
            var destPort = port1.PortType == PortType.OutPort ? port2 : port1;

            // Now we can just iterate attached connections of the source and see if it each one is attached to the destination connector.
            return sourcePort.AttachedConnections.FirstOrDefault(connection => connection.DestPort == destPort);
        }

        public void ReAnalysisImage()
        {
            foreach (ChannelModVm ch in _channelModuleList)
            {
                ch.Execute();
            }
        }



        #endregion
    }
}
