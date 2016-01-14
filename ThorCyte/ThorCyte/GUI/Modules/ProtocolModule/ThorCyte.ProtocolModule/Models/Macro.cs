using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml;
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

        }
        #endregion

        #region Fields and Properties

        public delegate void ClearHandler();
        public static ClearHandler Clear;

        private static Macro _uniqueInstance;
        public static Macro Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = new Macro()); }
        }

        public static ScanInfo CurrentScanInfo { get; private set; }
        public static int CurrentScanId { get; private set; }
        public static int CurrentRegionId { get; private set; }
        public static int CurrentTileId { get; private set; }
        public static int ImageMaxBits { get; private set; }
        public static Dictionary<string, ImageData> CurrentImages { get; private set; }


        public static IData CurrentDataMgr { get; private set; }

        public static ImpObservableCollection<ConnectorModel> Connections;
        public static ImpObservableCollection<ModuleBase> Modules;
        public static ModuleBase SelectedModuleViewModel;

        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        #endregion

        #region Methods
        private void ExpLoaded(int scanId)
        {
            var exp = ServiceLocator.Current.GetInstance<IExperiment>();
            if (exp == null) return;

            Clear(); 
            CurrentScanId = scanId;
            CurrentScanInfo = exp.GetScanInfo(scanId);
            ImageMaxBits = exp.GetExperimentInfo().IntensityBits;
            CurrentDataMgr = ServiceLocator.Current.GetInstance<IData>();

            ClearImagesDic();
            CurrentImages = new Dictionary<string, ImageData>();

        }

        /// <summary>
        /// Load Marco data from xml file.
        /// </summary>
        /// <param name="reader"></param>
        public void Load(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "module":
                            var id = XmlConvert.ToInt32(reader["id"]);
                            ModuleBase module;
                            if (reader.Name == "combination-module")
                            {
                                throw new CyteException("Macro.Load","Combination module does not support yet.");
                            }
                            var info = ModuleInfoMgr.GetModuleInfo(reader["name"]);
                            if (info == null)
                                throw new CyteException("Macro.Load", string.Format("Could not create module [{0}].\nModule is not defined in modules.xml.", reader["name"]));
                            if (string.IsNullOrEmpty(info.Reference))
                            {
                                continue;
                            }
                            module = CreateModule(info);

                            module.Id = id;
                            module.Enabled = Convert.ToBoolean(reader["enabled"]);
                            module.ScanNo = Convert.ToInt32(reader["scale"]);

                            if (reader["x"] != null)
                            {
                                module.X = (int)(XmlConvert.ToInt32(reader["x"]) * 1.5);
                            }

                            if (reader["y"] != null)
                            {
                                module.Y = (int)(XmlConvert.ToInt32(reader["y"]) * 1.5);
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
            //Find the Experiment module.
            var expMod = Modules.FirstOrDefault(m => m is ChannelModVm);
            if (expMod == null) return;


            var t2 = new Thread(new ParameterizedThreadStart(TestMethod));


            foreach (var region in CurrentScanInfo.ScanRegionList)
            {
                CurrentRegionId = region.RegionId;

                foreach (var tile in region.ScanFieldList)
                {
                    CurrentTileId = tile.ScanFieldId;
                    //enable images Dic
                    GetImagesDic();
                    expMod.Execute();
                    ClearImagesDic();

                    Debug.WriteLine("Region ID " + CurrentRegionId);
                    Debug.WriteLine("Field ID " + CurrentTileId);

                }
            }
        }

        public static void TestMethod(object data)
        {
            string datastr = data as string;
            Console.WriteLine("带参数的线程函数，参数为：{0}", datastr);
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




        #endregion
    }
}
