using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using ComponentDataService;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.ViewModels.Modules;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThorCyte.ProtocolModule.Models
{
    public class Macro
    {
        #region Constructor

        static Macro()
        {
            Syncobj = new object();
            FrmErrMsg = new MessageBox();
        }

        private Macro()
        {
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
            EventAggregator.GetEvent<SaveAnalysisResultEvent>().Subscribe(Save);


            _imageanalyzed += ImageAnalyzed;
            _isAborting = false;
            Connections = new ImpObservableCollection<ConnectorModel>();
            Modules = new ImpObservableCollection<ModuleBase>();
            CurrentImages = new Dictionary<string, ImageData>();
        }
        #endregion

        #region Fields and Properties

        private static Thread _tAnalyzeImg;
        private static bool _isAborting;

        private static readonly object Syncobj;
        private static readonly MessageBox FrmErrMsg;

        private delegate void ImageAnalyzedCallBackDelegate();
        private static ImageAnalyzedCallBackDelegate _imageanalyzed;
        public delegate void ClearHandler();
        public static ClearHandler Clear;
        private static IExperiment _exp;

        private static Macro _uniqueInstance;

        public static Macro Instance
        {
            get
            {
                return _uniqueInstance ?? (_uniqueInstance = new Macro());
            }
        }

        public static int CurrentScanId { get; private set; }
        public static int CurrentRegionId { get; private set; }
        public static int CurrentTileId { get; private set; }
        public static int ImageMaxBits { get; private set; }
        private static string AnalysisPath { get; set; }
        public static ScanInfo CurrentScanInfo { get; private set; }
        public static Dictionary<string, ImageData> CurrentImages { get; private set; }
        public static ImpObservableCollection<ConnectorModel> Connections;
        public static ImpObservableCollection<ModuleBase> Modules;
        public static ModuleBase SelectedModuleViewModel;
        private static IData CurrentDataMgr { get; set; }
        public static IComponentDataService CurrentConponentService { get; private set; }

        private static IEventAggregator _eventAggregator;

        private static IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private static ILog _logger;

        public static ILog Logger
        {
            get
            {
                return _logger ?? (_logger = ServiceLocator.Current.GetInstance<ILog>());
            }
        }

        private static string ProtocolFileName
        {
            get
            {
                return AnalysisPath + @"\Protocol.xml";
            }
        }

        #endregion

        #region Methods
        private void ExpLoaded(int scanid)
        {
            try
            {
                if (scanid < 1) return;
                _exp = ServiceLocator.Current.GetInstance<IExperiment>();

                if (Clear != null)
                {
                    Clear.Invoke();
                }
                CurrentScanId = scanid;
                ResetRegionTileId();
                CurrentScanInfo = _exp.GetScanInfo(scanid);
                var expinfo = _exp.GetExperimentInfo();
                ImageMaxBits = expinfo.IntensityBits;
                AnalysisPath = expinfo.AnalysisPath;
                CurrentDataMgr = ServiceLocator.Current.GetInstance<IData>();
                CurrentConponentService = ComponentDataManager.Instance;
                ClearImagesDic();
                CurrentImages = new Dictionary<string, ImageData>();
                Load();
            }
            catch (Exception ex)
            {
                Logger.Write("Error occourd in Macro.ExpLoaded ", ex);
                MessageHelper.PostMessage("Error occourd in Macro.ExpLoaded: " + ex.Message);
            }
        }

        /// <summary>
        /// Load Marco data from xml file.
        /// </summary>
        private void Load()
        {
            try
            {
                if (!File.Exists(ProtocolFileName))
                {
                    MessageHelper.PostMessage("New protocol edit!");
                    return;
                }

                MessageHelper.PostMessage("Loading...");
                var streamReader = new StreamReader(ProtocolFileName);
                var settings = new XmlReaderSettings();
                var reader = XmlReader.Create(streamReader, settings);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "module":
                                var id = XmlConvert.ToInt32(reader["id"]);
                                if (reader.Name == "combination-module")
                                {
                                    throw new CyteException("Macro.Load", "Combination module does not support yet.");
                                }
                                var info = ModuleInfoMgr.GetModuleInfo(reader["name"]);
                                if (info == null)
                                    throw new CyteException("Macro.Load",
                                        string.Format("Could not create module [{0}].\nModule is not defined in modules.xml.", reader["name"]));
                                if (string.IsNullOrEmpty(info.Reference))
                                {
                                    continue;
                                }
                                var modulelst = CreateModule(info);
                                var module = modulelst[0];
                                module.Id = id;
                                module.Enabled = Convert.ToBoolean(reader["enabled"]);
                                module.ScanNo = Convert.ToInt32(reader["scale"]);

                                if (reader["x"] != null)
                                {
                                    module.X = XmlConvert.ToInt32(reader["x"]);
                                }

                                if (reader["y"] != null)
                                {
                                    module.Y = XmlConvert.ToInt32(reader["y"]);
                                }
                                module.Initialize();
                                module.Deserialize(reader);
                                break;
                            case "connector":
                                if (reader["inport-module-id"] != null && reader["outport-module-id"] != null)
                                {
                                    var inPortId = XmlConvert.ToInt32(reader["inport-module-id"]);
                                    var outPortId = XmlConvert.ToInt32(reader["outport-module-id"]);
                                    var inPortIndex = 0;
                                    if (reader["inport-index"] != null)
                                    {
                                        inPortIndex = XmlConvert.ToInt32(reader["inport-index"]);
                                    }
                                    var outPortIndex = 0;
                                    if (reader["outport-index"] != null)
                                    {
                                        outPortIndex = XmlConvert.ToInt32(reader["outport-index"]);
                                    }
                                    CreateConnector(inPortId, outPortId, inPortIndex, outPortIndex);
                                }
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "macro")
                        {
                            break;
                        }
                    }
                }
                reader.Close();
                streamReader.Close();
                MessageHelper.PostMessage("All Component Loaded!");
            }
            catch (Exception ex)
            {
                Logger.Write("Error occourd in Macro.Load: ", ex);
                MessageHelper.PostMessage("Error occourd in Macro.Load: " + ex.Message);
            }
        }

        private static void Save(int i)
        {
            try
            {
                var expinfo = _exp.GetExperimentInfo();
                AnalysisPath = expinfo.AnalysisPath;
                Save();
            }
            catch (Exception ex)
            {
                Logger.Write("Error occourd in Macro.Save", ex);
                MessageHelper.PostMessage("Error occourd in Macro.Save: " + ex.Message);
            }
        }

        /// <summary>
        /// Save Protocol data.
        /// </summary>
        public static void Save()
        {
            //Find file directory
            if (!Directory.Exists(CurrentScanInfo.DataPath))
                throw new CyteException("Macro.Save", string.Format("Destination directory ({0}) not found!", CurrentScanInfo.DataPath));

            if (!Directory.Exists(AnalysisPath))
                Directory.CreateDirectory(AnalysisPath);

            var sr = new StreamWriter(AnalysisPath + @"\Protocol.xml");
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };
            var writer = XmlWriter.Create(sr, settings);
            writer.WriteStartDocument(false);
            writer.WriteStartElement("Macro");
            writer.WriteStartElement("modules");
            foreach (var m in Modules)
            {
                m.Serialize(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("connectors");
            foreach (var c in Connections)
            {
                c.Serialize(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            sr.Close();

            MessageHelper.PostMessage("Save Completed!");
        }


        public static List<ModuleBase> CreateModule(ModuleInfo modInfo)
        {
            try
            {
                if (modInfo.IsCombo)
                {
                    // create combination module                    
                    var cmbmod = ModuleInfoMgr.CombinationModuleDefs.FirstOrDefault(cm => cm.Name == modInfo.Name && cm.Category == modInfo.Category);
                    var addModLst = new List<ModuleBase>();
                    if (cmbmod != null)
                    {
                        var refdic = new Dictionary<int, int>();

                        foreach (var m in cmbmod.SubModules)
                        {
                            var mc = m.Clone() as ModuleBase;
                            refdic.Add(m.Id, mc.Id);
                            Modules.Add(mc);
                            addModLst.Add(mc);
                        }

                        // find connections on these modules and create them
                        foreach (var m in cmbmod.SubModules)
                        {
                            foreach (var c in m.OutputPort.AttachedConnections)
                            {
                                if (cmbmod.SubModules.Contains(c.DestPort.ParentModule))
                                {
                                    var outMod = m;
                                    var inMod = c.DestPort.ParentModule;
                                    var inportIdx = inMod.InputPorts.IndexOf(c.DestPort);
                                    CreateConnector(refdic[inMod.Id], refdic[outMod.Id], inportIdx, 0);
                                }
                            }
                        }
                    }

                    return addModLst;
                }
                else
                {
                    var module = (ModuleBase)Activator.CreateInstance(Type.GetType(modInfo.Reference, true));
                    module.Name = modInfo.Name;
                    if (!string.IsNullOrEmpty(modInfo.ViewReference))
                    {
                        module.View = (UserControl)Activator.CreateInstance(Type.GetType(modInfo.ViewReference, true));
                    }
                    module.Id = ModuleBase.GetNextModId();
                    module.DisplayName = modInfo.DisplayName;
                    module.ScanNo = CurrentScanId;
                    Modules.Add(module);
                    return new List<ModuleBase>() { module };
                }

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Macro.CreateModule Could not create module [{0}].", modInfo.Reference), ex);
                throw new CyteException("Macro.CreateModule",
                    string.Format("Could not create module [{0}]." + ex.Message, modInfo.Reference));
            }
        }

        public static void CreateConnector(int inPortId, int outPortId, int inPortIndex, int outPortIndex)
        {
            ModuleBase inModule = null;
            ModuleBase outModule = null;

            foreach (var module in Modules)
            {
                if (module.Id == inPortId)
                {
                    inModule = module;
                }
                else if (module.Id == outPortId)
                {
                    outModule = module;
                }

                if (inModule != null && outModule != null)
                {
                    break;
                }
            }

            if (inModule == null || outModule == null)
            {
                return;
            }
            var connector = new ConnectorModel(outModule.OutputPort, inModule.InputPorts[inPortIndex]);
            Connections.Add(connector);
        }

        public static void Run()
        {
            try
            {
                if (!CheckExecutable())
                {
                    MessageHelper.PostMessage("Rules Violated! Macro can not execute.");
                    return;
                }
                //SetConponent post event

                CurrentConponentService.ClearComponents();
                foreach (var m in Modules)
                {
                    m.InitialRun();
                }
                EventAggregator.GetEvent<MacroRunEvent>().Publish(CurrentScanId);
                AnalyzeWorkDispacher();
                _tAnalyzeImg = new Thread(AnalyzeImage) { IsBackground = true };
                _tAnalyzeImg.Start();
            }
            catch (Exception ex)
            {
                MessageHelper.PostMessage("Error occourd in Macro.Run: " + ex.Message);
                Logger.Write("Error occourd in Macro.Run", ex);
            }
        }

        private static bool CheckExecutable()
        {
            var ret = !(_tAnalyzeImg != null && _tAnalyzeImg.IsAlive);
            if (!Modules.Any(m => m is ChannelModVm))
            {
                return false;
            }

            if (Modules.Any(m => !m.Executable))
            {
                return false;
            }

            var vail = true;
            foreach (var m in Modules)
            {
                if (m.InputPorts.Any(p => p.AttachedConnections.Count == 0 && p.DataType != PortDataType.None))
                {
                    vail = false;
                }

                if (!vail) break;
            }

            return ret && vail;
        }

        public static void Pause()
        {
        }

        public static void Stop()
        {
            if (_tAnalyzeImg == null) return;
            if (_tAnalyzeImg.IsAlive)
            {
                lock (Syncobj)
                {
                    _isAborting = true;
                }
            }
        }


        private static void AnalyzeImage_bak()
        {
            try
            {
                MessageHelper.SendMacroRuning(true);
                MessageHelper.PostProgress("Region", CurrentScanInfo.ScanRegionList.Count, 0);
                foreach (var region in CurrentScanInfo.ScanRegionList)
                {
                    CurrentRegionId = region.RegionId;
                    MessageHelper.PostProgress("Tile", region.ScanFieldList.Count, 0);
                    foreach (var tile in region.ScanFieldList)
                    {
                        CurrentTileId = tile.ScanFieldId;
                        EventAggregator.GetEvent<MacroStartEvnet>()
                            .Publish(new MacroStartEventArgs { WellId = region.WellId, RegionId = CurrentRegionId, TileId = CurrentTileId });


                        ExecuteImage();


                        if (_isAborting)
                            break;
                        MessageHelper.PostProgress("Tile", -1, CurrentTileId);
                    }

                    if (_isAborting)
                        break;

                    MessageHelper.PostProgress("Region", -1, CurrentRegionId + 1);
                }

            }
            catch (Exception ex)
            {
                Logger.Write("Error occourd in Analyze Image", ex);
                FrmErrMsg.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    FrmErrMsg.Text = "Errorr occourred in Analyze Image: " + ex.Message;
                    FrmErrMsg.Caption = "ThorCyte";
                    FrmErrMsg.OkButtonContent = "OK";
                    FrmErrMsg.ShowDialog();
                }));
            }
            finally
            {
                if (_imageanalyzed != null)
                {
                    _imageanalyzed.Invoke();
                }
            }
        }

        private static void AnalyzeImage()
        {
            try
            {
                MessageHelper.SendMacroRuning(true);
                MessageHelper.PostProgress("Region", CurrentScanInfo.ScanRegionList.Count, 0);
                var fieldCount = -1;
                while (true)
                {
                    if (_isAborting)
                    {
                        break;
                    }

                    lock (_taskQueue)
                    {
                        if (!_taskQueue.Any())
                        {
                            break;
                        }
                        var rt = _taskQueue.Dequeue();


                        var srgn = CurrentScanInfo.ScanRegionList.FirstOrDefault(sr => sr.RegionId == rt.RegionId);


                        if (srgn != null)
                        {
                            CurrentRegionId = rt.RegionId;


                            if (fieldCount != srgn.ScanFieldList.Count)
                            {
                                MessageHelper.PostProgress("Tile", srgn.ScanFieldList.Count, 0);
                                fieldCount = srgn.ScanFieldList.Count;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        var stile = srgn.ScanFieldList.FirstOrDefault(st => st.ScanFieldId == rt.TileId);
                        if (stile != null)
                        {
                            CurrentTileId = rt.TileId;
                        }
                        else
                        {
                            continue;
                        }


                        EventAggregator.GetEvent<MacroStartEvnet>().Publish(new MacroStartEventArgs
                        {
                            WellId = srgn.WellId,
                            RegionId = CurrentRegionId,
                            TileId = CurrentTileId
                        });

                        ExecuteImage();
                        MessageHelper.PostProgress("Tile", -1, CurrentTileId);
                        MessageHelper.PostProgress("Region", -1, CurrentRegionId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error occourd in Analyze Image", ex);
                FrmErrMsg.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    FrmErrMsg.Text = "Errorr occourred in Analyze Image: " + ex.Message;
                    FrmErrMsg.Caption = "ThorCyte";
                    FrmErrMsg.OkButtonContent = "OK";
                    FrmErrMsg.ShowDialog();
                }));
            }
            finally
            {
                if (_imageanalyzed != null)
                {
                    _imageanalyzed.Invoke();
                }
            }
        }


        private static Queue<RegionTile> _taskQueue = new Queue<RegionTile>();
        private static void AnalyzeWorkDispacher(RegionTile rt = null)
        {
            if (rt == null)
            {
                //Start from beginning.
                rt = new RegionTile
                {
                    RegionId = int.MinValue,
                    TileId = int.MinValue
                };
            }

            foreach (var region in CurrentScanInfo.ScanRegionList)
            {
                if (region.RegionId < rt.RegionId) continue;

                foreach (var tile in region.ScanFieldList)
                {
                    if (tile.ScanFieldId < rt.TileId) continue;

                    var task = new RegionTile
                    {
                        RegionId = region.RegionId,
                        TileId = tile.ScanFieldId
                    };

                    lock (_taskQueue)
                    {
                        _taskQueue.Enqueue(task);
                    }

                }
            }

        }


        private static void ExecuteImage()
        {
            GetImagesDic();
            //find all channel module 
            foreach (var mod in Modules.Where(m => m is ChannelModVm))
            {
                mod.Execute();
                MessageHelper.PostMessage(
                    string.Format("Current Processed - Region: {0}; Tile: {1}; Channel: {2};", CurrentRegionId, CurrentTileId, ((ChannelModVm)mod).SelectedChannel));
            }
            //wait all channel executed here
            ClearImagesDic();
        }



        private static void ImageAnalyzed()
        {
            if (_isAborting) _isAborting = false;
            MessageHelper.PostMessage("All images analyzed!");
            MessageHelper.SendMacroRuning(false);
            EventAggregator.GetEvent<MacroFinishEvent>().Publish(CurrentScanId);
            ResetRegionTileId();
        }

        private static void ClearImagesDic()
        {
            if (CurrentImages == null) return;

            foreach (var img in CurrentImages.Values.Where(img => img != null))
            {
                img.Dispose();
            }

            CurrentImages.Clear();
        }

        private static void GetImagesDic()
        {
            ClearImagesDic();
            foreach (var channel in CurrentScanInfo.ChannelList)
            {
                CurrentImages.Add(channel.ChannelName, GetData(channel));
            }

            foreach (var channel in CurrentScanInfo.VirtualChannelList)
            {

                CurrentImages.Add(channel.ChannelName, GetData(channel));
            }
        }

        private static ImageData GetData(Channel channel)
        {
            ImageData data;

            if (channel.IsvirtualChannel)
            {
                var dic = new Dictionary<Channel, ImageData>();
                TraverseVirtualChannel(dic, channel as VirtualChannel);
                data = GetVirtualChData(channel as VirtualChannel, dic);
            }
            else
            {
                data = GetRealChData(channel);
            }
            return data;
        }

        private static void TraverseVirtualChannel(IDictionary<Channel, ImageData> dic, VirtualChannel channel)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, GetRealChData(channel.FirstChannel));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, GetRealChData(channel.SecondChannel));
                }
            }
        }

        private static ImageData GetRealChData(Channel channelInfo)
        {
            var data = CurrentDataMgr.GetTileData(CurrentScanId, CurrentRegionId, channelInfo.ChannelId, 0, CurrentTileId, 0);
            if (data == null)
                throw new CyteException("Macro.GetRealChData", "Get 2D data filed");
            return data;
        }

        private static ImageData GetVirtualChData(VirtualChannel channel, Dictionary<Channel, ImageData> dic)
        {

            ImageData data1 = null, data2 = null;
            if (!channel.FirstChannel.IsvirtualChannel)
            {
                data1 = dic[channel.FirstChannel];
            }
            else
            {
                data1 = GetVirtualChData(channel.FirstChannel as VirtualChannel, dic);
            }

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert && channel.Operator != ImageOperator.ShiftPeak)
            {
                if (!channel.SecondChannel.IsvirtualChannel)
                {
                    data2 = dic[channel.SecondChannel];
                }
                else
                {
                    data2 = GetVirtualChData(channel.SecondChannel as VirtualChannel, dic);
                }
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert && channel.Operator != ImageOperator.ShiftPeak))) return null;
            var result = new ImageData(data1.XSize, data1.YSize);
            switch (channel.Operator)
            {
                case ImageOperator.Add:
                    result = data1.Add(data2, ImageMaxBits);
                    break;
                case ImageOperator.Subtract:
                    result = data1.Sub(data2);
                    break;
                case ImageOperator.Invert:
                    result = data1.Invert(ImageMaxBits);
                    break;
                case ImageOperator.Max:
                    result = data1.Max(data2);
                    break;
                case ImageOperator.Min:
                    result = data1.Min(data2);
                    break;
                case ImageOperator.Multiply:
                    result = data1.MulConstant(channel.Operand, ImageMaxBits);
                    break;
                case ImageOperator.ShiftPeak:
                    result = data1.ShiftPeak((ushort)channel.Operand, ImageMaxBits);
                    break;
                default:
                    break;
            }
            return result;
        }


        private static void SetRegionTileId(int regionId, int tileId)
        {
            CurrentRegionId = int.MinValue;
            CurrentTileId = int.MinValue;
        }

        private static void ResetRegionTileId()
        {
            CurrentRegionId = int.MinValue;
            CurrentTileId = int.MinValue;
        }

        ~Macro()
        {
            Stop();
        }

        #endregion
    }
}
