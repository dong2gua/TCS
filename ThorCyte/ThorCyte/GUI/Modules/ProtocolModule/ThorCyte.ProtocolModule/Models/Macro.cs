using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
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

namespace ThorCyte.ProtocolModule.Models
{
    public class Macro
    {

        #region Constants
        private Macro()
        {
            EventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExpLoaded);
            _imageanalyzed += ImageAnalyzed;
            _isAborting = false;
            IsRuning = false;
        }
        #endregion

        #region Fields and Properties

        private static Thread _tAnalyzeImg;
        private static bool _isAborting;
        private static readonly object Syncobj = new object();
        private delegate void ImageAnalyzedCallBackDelegate();
        private static ImageAnalyzedCallBackDelegate _imageanalyzed;
        public delegate void ClearHandler();
        public static ClearHandler Clear;

        private static Macro _uniqueInstance;
        public static Macro Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = new Macro()); }
        }

        public static int CurrentScanId { get; private set; }
        public static int CurrentRegionId { get; private set; }
        public static int CurrentTileId { get; private set; }
        public static int ImageMaxBits { get; private set; }
        public static string AnalysisPath { get; private set; }
        public static ScanInfo CurrentScanInfo { get; private set; }
        public static Dictionary<string, ImageData> CurrentImages { get; private set; }
        public static ImpObservableCollection<ConnectorModel> Connections;
        public static ImpObservableCollection<ModuleBase> Modules;
        public static ModuleBase SelectedModuleViewModel;
        public static IData CurrentDataMgr { get; private set; }
        public static IComponentDataService CurrentConponentService { get; private set; }

        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private static string ProtocolFileName
        {
            get
            {
                return AnalysisPath + @"\Protocol.xml";
            }
        }

        public static bool IsRuning { get; private set; }

        #endregion

        #region Methods
        private void ExpLoaded(int scanId)
        {
            var exp = ServiceLocator.Current.GetInstance<IExperiment>();
            if (exp == null) return;

            if (Clear != null) Clear();
            CurrentScanId = scanId;
            CurrentScanInfo = exp.GetScanInfo(scanId);
            var expinfo = exp.GetExperimentInfo();
            ImageMaxBits = expinfo.IntensityBits;
            AnalysisPath = expinfo.AnalysisPath;
            CurrentDataMgr = ServiceLocator.Current.GetInstance<IData>();
            CurrentConponentService = ComponentDataManager.Instance;
            CurrentConponentService.Load(exp);
            ClearImagesDic();
            CurrentImages = new Dictionary<string, ImageData>();

            Load();
        }

        /// <summary>
        /// Load Marco data from xml file.
        /// </summary>
        public void Load()
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
                                throw new CyteException("Macro.Load", string.Format("Could not create module [{0}].\nModule is not defined in modules.xml.", reader["name"]));
                            if (string.IsNullOrEmpty(info.Reference))
                            {
                                continue;
                            }
                            var module = CreateModule(info);

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
            var writer = XmlWriter.Create(sr,settings);
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


        public static ModuleBase CreateModule(ModuleInfo modInfo)
        {
            try
            {
                if (modInfo.IsCombo) throw new CyteException("Macro.CreateModule", "Combination module does not support.");// create combination module
                var module = (ModuleBase)Activator.CreateInstance(Type.GetType(modInfo.Reference, true));
                module.Name = modInfo.Name;
                module.DisplayName = modInfo.DisplayName;
                Modules.Add(module);
                module.Id = Modules.Count - 1;
                module.ScanNo = CurrentScanId;
                return module;
            }
            catch (Exception ex)
            {
                throw new CyteException("Macro.CreateModule", string.Format("Could not create module [{0}].", modInfo.Reference) + ex.Message);
            }
        }

        private void CreateConnector(int inPortId, int outPortId, int inPortIndex, int outPortIndex)
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
            if (!CheckExecutable())
            {
                MessageHelper.PostMessage("Rules Violated! Macro can not execute.");
                return;
            }

            _tAnalyzeImg = new Thread(AnalyzeImage) { IsBackground = true };
            _tAnalyzeImg.Start();
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

            var vail = false;
            foreach (var m in Modules)
            {
                if (m.InputPorts.Any(p => p.AttachedConnections.Count != 0))
                {
                    vail = true;
                }

                if (vail) break;
            }

            return ret && vail;
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

        public static void AnalyzeImage()
        {
            try
            {
                IsRuning = true;
                MessageHelper.PostProgress("Region", CurrentScanInfo.ScanRegionList.Count, 0);
                foreach (var region in CurrentScanInfo.ScanRegionList)
                {
                    CurrentRegionId = region.RegionId;
                    MessageHelper.PostProgress("Tile", region.ScanFieldList.Count, 0);
                    foreach (var tile in region.ScanFieldList)
                    {
                        CurrentTileId = tile.ScanFieldId;

                        GetImagesDic();
                        //find all channel module 
                        foreach (var mod in Modules.Where(m => m is ChannelModVm))
                        {
                            mod.Execute();

                            MessageHelper.PostMessage(
                                string.Format("Current Processing - Region: {0}; Tile: {1}; Channel: {2};",
                                    CurrentRegionId, CurrentTileId, ((ChannelModVm) mod).SelectedChannel));
                        }
                        //wait all channel executed here
                        ClearImagesDic();
                        if (_isAborting)
                            break;

                        MessageHelper.PostProgress("Tile", -1, CurrentTileId);
                    }
                    if (_isAborting)
                        break;

                    MessageHelper.PostProgress("Region", -1, CurrentRegionId + 1);
                }

                if (_imageanalyzed != null) _imageanalyzed();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorr occourred in Analyze Image: " + ex.Message);
            }
            
        }

        public static void ImageAnalyzed()
        {
            if (_isAborting) _isAborting = false;
            MessageHelper.PostMessage("All images analyzed!");
            IsRuning = false;
        }

        private static void ClearImagesDic()
        {
            if (CurrentImages == null) return;

            foreach (var img in CurrentImages.Values)
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
        }

        public static ImageData GetData(Channel channel)
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
                data = getRealChData(channel);
            }
            return data;
        }

        private static void TraverseVirtualChannel(Dictionary<Channel, ImageData> dic, VirtualChannel channel)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, getRealChData(channel.FirstChannel));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, getRealChData(channel.SecondChannel));
                }
            }
        }

        private static ImageData getRealChData(Channel channelInfo)
        {
            return CurrentDataMgr.GetTileData(CurrentScanId, CurrentRegionId, channelInfo.ChannelId, 0, 0,
                            CurrentTileId, 0);
        }

        private static ImageData GetVirtualChData(VirtualChannel channel, Dictionary<Channel, ImageData> dic)
        {

            ImageData data2 = null;
            var data1 = !channel.FirstChannel.IsvirtualChannel ? dic[channel.FirstChannel] : GetVirtualChData(channel.FirstChannel as VirtualChannel, dic);

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert)
            {
                data2 = !channel.SecondChannel.IsvirtualChannel ? dic[channel.SecondChannel] : GetVirtualChData(channel.SecondChannel as VirtualChannel, dic);
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert))) return null;
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
            }
            return result;
        }

        ~Macro()
        {
            Stop();
        }

        #endregion
    }
}
