using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using ThorCyte.ProtocolModule.Controls;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.Views;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

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
        public ICommand CopyModulesCommnad { get; private set; }
        public ICommand PasteModulesCommnad { get; private set; }
        public ICommand MoveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand SaveTemplateCommand { get; private set; }

        private List<ModuleBase> _clipboard = new List<ModuleBase>();
        private Dictionary<string, ImageDisplayView> _dispImgDic;

        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
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

        private bool _isAlignEnable;
        public bool IsAlignEnable
        {
            get { return _isAlignEnable; }
            set { SetProperty(ref _isAlignEnable, value); }
        }

        private bool _isPasteEnable;
        public bool IsPasteEnable
        {
            get { return _isPasteEnable; }
            set
            {
                if (_isPasteEnable == value) return;
                SetProperty(ref _isPasteEnable, value);
            }
        }

        private bool _isRemoveEnable;
        public bool IsRemoveEnable
        {
            get { return _isRemoveEnable; }
            set
            {
                if (_isRemoveEnable == value) return;
                SetProperty(ref _isRemoveEnable, value);
            }
        }

        #endregion

        #region Contructors

        public MarcoEditorViewModel()
        {
            MessageHelper.SetMessage += SetMessage;
            MessageHelper.SetProgress += SetProgress;
            MessageHelper.SetRuning += SetRuning;
            Module.OnSelectionChanged += ModuleOnOnSelectionChanged;
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
            EventAggregator.GetEvent<DisplayImageEvent>().Subscribe(DisplayImage, ThreadOption.UIThread);

            MacroCommnad = new DelegateCommand(SetMacroCommand);
            SaveMacroCommand = new DelegateCommand(Macro.Save);

            AlignCommnad = new DelegateCommand<string>(SetModulesAlign);

            CopyModulesCommnad = new DelegateCommand(CopyModules);
            PasteModulesCommnad = new DelegateCommand<object>(PasteModules);

            DeleteCommand = new DelegateCommand(DeleteModules);
            MoveCommand = new DelegateCommand<string>(MoveModules);
            SelectAllCommand = new DelegateCommand(SelectAllModules);
            SaveTemplateCommand = new DelegateCommand(SaveTemplate);

            _pannelVm = new PannelViewModel();
            _dispImgDic = new Dictionary<string, ImageDisplayView>();
            RegionCount = 10;
            TileCount = 10;

            _imgSource = IsRuning ? "../Resource/Images/stop.png" : "../Resource/Images/play.png";
            _tipStr = IsRuning ? "Stop Run" : "Start Run";
            _isAlignEnable = !IsRuning;
        }

        private void SelectAllModules()
        {
            foreach (var m in PannelVm.Modules)
            {
                m.IsSelected = true;
            }

            foreach (var c in PannelVm.Connections)
            {
                c.IsSelected = true;
            }

        }

        private void DeleteModules()
        {
            DeleteSelectedModules();
            DeleteSelectedConnectors();
        }


        private void ExpLoaded(int scanid)
        {
            SelectModuleOrder.Clear();
        }

        private void ModuleOnOnSelectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var module = sender as Module;

            if (module == null) return;

            var mb = module.Content as ModuleBase;

            if (mb == null) return;

            //SelectModuleIndex.
            if ((bool)e.NewValue)
            {
                SelectModuleOrder.Add(mb.Id);
            }
            else
            {
                SelectModuleOrder.Remove(mb.Id);
            }

            IsRemoveEnable = SelectModuleOrder.Count > 0 && _isAlignEnable;
            PannelVm.SelectedModuleViewModel = SelectModuleOrder.Count == 1 ? PannelVm.Modules.FirstOrDefault(md => md.Id == SelectModuleOrder[0]) : null;
        }

        private void SetModulesAlign(string alnType)
        {
            try
            {
                if (!PannelVm.Modules.Any(md => md.IsSelected)) return;
                var module = SelectModuleOrder.Count > 0 ?
                    PannelVm.Modules.FirstOrDefault(md => md.Id == SelectModuleOrder[0]) : PannelVm.Modules.FirstOrDefault(md => md.IsSelected);

                if (module == null) return;
                var vx = module.X;
                var vy = module.Y;

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
                SelectModuleOrder.Clear();
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("Error occurred in SetModulesAlign", ex);
                MessageBox.Show("Error occurred in SetModulesAlign" + ex.Message, "ThorCyte", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void MoveModules(string direction)
        {
            try
            {
                var space = 5;

                foreach (var m in PannelVm.Modules.Where(md => md.IsSelected))
                {
                    switch (direction)
                    {
                        case "Up":
                            m.Y -= space;
                            if (m.Y < 0) m.Y = 0;

                            break;
                        case "Down":
                            m.Y += space;
                            break;
                        case "Left":
                            m.X -= space;
                            if (m.X < 0) m.X = 0;
                            break;
                        case "Right":
                            m.X += space;
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Macro.Logger.Write("Error occurred in MoveModules", ex);
                MessageBox.Show(Application.Current.MainWindow, "Error occurred in MoveModules" + ex.Message, "ThorCyte", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
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

            if (draggedOutPort.PortType == PortType.InPort)
            {

                if (draggedOutPort.AttachedConnections.Count > 0)
                {
                    //1 connetion at most
                    connection = draggedOutPort.AttachedConnections[0];
                    PannelVm.Connections.Remove(connection);
                    connection.DestPort = null;
                    connection.DestPortHotspot = draggedOutPort.HotSpot;
                    PannelVm.Connections.Add(connection);
                }
                else
                {
                    connection.DestPort = draggedOutPort;
                    connection.SourcePortHotspot = draggedOutPort.HotSpot;
                    PannelVm.Connections.Add(connection);
                }
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

            if (startPort == null || endPort == null)
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
        private bool HaveConnetion(PortModel endPort)
        {
            return PannelVm.Connections.Any(connection => connection.DestPortHotspot == endPort.HotSpot);
        }

        /// <summary>
        /// Called when the user has finished dragging out the new _connection.
        /// </summary>
        public void ConnectionDragCompleted(ConnectorModel newConnection, PortModel portDraggedOut, PortModel portDraggedOver)
        {
            try
            {
                if (portDraggedOut.PortType == PortType.InPort)
                {

                    if (newConnection.SourcePort == null)
                    {
                        //new connection switch dragout and drag over port.
                        var tempPort = portDraggedOut;
                        portDraggedOut = portDraggedOver;
                        portDraggedOver = tempPort;
                    }
                    else
                    {
                        //Drag current connection
                        portDraggedOut = newConnection.SourcePort;
                    }

                }

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
                if (newConnection.SourcePort == null)
                {
                    newConnection.SourcePort = portDraggedOut;
                    newConnection.DestPortHotspot = portDraggedOver.HotSpot;
                }
                else
                {
                    newConnection.DestPort = portDraggedOver;
                }
            }
            catch (Exception ex)
            {
                Macro.Logger.Write("MacroEditorViewModel:ConnectionDragCompleted error: ", ex);
                Debug.WriteLine("MacroEditorViewModel:ConnectionDragCompleted error: " + ex.Message);
            }

        }

        private bool IsConnectionOK(ConnectorModel newConnection, PortModel portDraggedOut, PortModel portDraggedOver)
        {
            var res = true;

            //if (newConnection.SourcePort == null) return false;

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

        public void DeleteSelectedConnectors()
        {
            var rmvConns = PannelVm.Connections.Where(conn => conn.IsSelected).ToList();
            foreach (var rmvc in rmvConns)
            {
                rmvc.SourcePort = null;
                rmvc.DestPort = null;
                PannelVm.Connections.Remove(rmvc);
            }
        }

        /// <summary>
        /// Delete the moduleVm from the view-model.Also deletes any connections to or from the moduleVm.
        /// </summary>
        private void DeleteModule(ModuleBase moduleVm)
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
            return 1 == count ? module : null;
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.Returns null if there is no connection between the connectors.
        /// </summary>
        private ConnectorModel FindConnection(ConnectorModel newConnection, PortModel port1, PortModel port2)
        {
            if (port1.PortType == port2.PortType)
            {
                return null;
            }
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
            IsAlignEnable = !isRuning;
        }


        /// <summary>
        /// Copy all Selected modules into clipboard.
        /// </summary>
        private void CopyModules()
        {
            _clipboard.Clear();
            foreach (var m in PannelVm.Modules.Where(md => md.IsSelected))
            {
                _clipboard.Add(m);
            }

            IsPasteEnable = _clipboard.Count > 0 && _isAlignEnable;
        }

        /// <summary>
        /// Paste modules in clipboard on the pannel.
        /// </summary>
        private void PasteModules(object o)
        {
            if (_clipboard.Count == 0) return;
            var tempPoint = new Point(0.0, 0.0);
            var pOffset = new Point(0.0, 0.0);

            if (o == null)
            {
                //click toolbar button
                pOffset.X = 100.0;
                pOffset.Y = 100.0;
            }
            else
            {
                //right cilck mouse
                pOffset = Mouse.GetPosition(o as UIElement);
                var pt = Mouse.GetPosition(null);
                pOffset.X -= pt.X;
                pOffset.Y -= pt.Y;

                tempPoint = new Point(_clipboard[0].X, _clipboard[0].Y);
                //var l = Math.Pow(_clipboard[0].X, 2) + Math.Pow(_clipboard[0].Y, 2);

                //find least length point
                foreach (var m in _clipboard)
                {
                    //var ln = Math.Pow(m.X, 2) + Math.Pow(m.Y, 2);

                    //if (ln < l)
                    //{
                    //    l = ln;
                    //    tempPoint.X = m.X;
                    //    tempPoint.Y = m.Y;
                    //}

                    if (m.X < tempPoint.X)
                        tempPoint.X = m.X;

                    if (m.Y < tempPoint.Y)
                        tempPoint.Y = m.Y;

                }
            }

            var refdic = new Dictionary<int, int>();
            foreach (var m in _clipboard)
            {
                var ma = m.Clone() as ModuleBase;
                refdic.Add(m.Id, ma.Id);
                ma.X = ma.X - (int)tempPoint.X + (int)pOffset.X;
                ma.Y = ma.Y - (int)tempPoint.Y + (int)pOffset.Y;
                m.IsSelected = false;
                ma.IsSelected = true;
                PannelVm.Modules.Add(ma);
            }

            // find connections on these modules and create them
            foreach (var m in _clipboard)
            {
                foreach (var c in m.OutputPort.AttachedConnections)
                {
                    if (_clipboard.Contains(c.DestPort.ParentModule))
                    {
                        var outMod = m;
                        var inMod = c.DestPort.ParentModule;
                        var inportIdx = inMod.InputPorts.IndexOf(c.DestPort);
                        Macro.CreateConnector(refdic[inMod.Id], refdic[outMod.Id], inportIdx, 0);
                    }
                }
            }
        }

        private void SaveTemplate()
        {
            var popupWnd = new CustomWindow
            {
                Content = new SaveTemplateView(),
                Title = "Save as Template",
                MinWidth = 300,
                MinHeight = 300,
                MaxWidth = 600,
                MaxHeight = 600,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            //choosed modules
            popupWnd.ShowDialog();
        }


        private void DisplayImage(DisplayImageEventArgs args)
        {
            if (!_dispImgDic.ContainsKey(args.Title))
            {
                var view = new ImageDisplayView(args);
                var wnd = new CustomWindow
                {
                    Name = args.Title,
                    Content = view,
                    Icon = Application.Current.MainWindow.Icon,
                    Title = args.Title,
                    //SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    ShowInTaskbar = true,
                    ResizeMode = ResizeMode.NoResize,
                    Height = 600,
                    Width = 800
                };
                wnd.Show();
                _dispImgDic.Add(args.Title, view);
            }
            else
            {
                var windtitleLst = (from Window w in Application.Current.Windows select w.Title).ToList();

                if (!windtitleLst.Contains(args.Title))
                {
                    var wnd = new CustomWindow
                    {
                        Name = args.Title,
                        Content = _dispImgDic[args.Title],
                        Icon = Application.Current.MainWindow.Icon,
                        Title = args.Title,
                        //SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        ShowInTaskbar = true,
                        ResizeMode = ResizeMode.NoResize,
                        Height = 600,
                        Width = 800
                    };

                    wnd.Show();
                }
            }
        }
        #endregion
    }

}
