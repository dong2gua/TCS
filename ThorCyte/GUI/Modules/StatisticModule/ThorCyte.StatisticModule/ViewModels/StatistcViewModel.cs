using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Prism.Interactivity.InteractionRequest;
using ThorCyte.Statistic;
using ThorCyte.Statistic.Models;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using ThorCyte.Infrastructure.Interfaces;
using ComponentDataService;
using System.Linq;
using ROIService;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Statistic.Views;
using IEventAggregator = Prism.Events.IEventAggregator;

namespace ThorCyte.Statistic.ViewModels
{
    public class StatisticViewModel : BindableBase
    {
        private IExperiment ExperimentAdapter { get; set; }
        private IPopupSetupWindow PopupWinAdapter{get; set; }
        private StatisticModel ModelAdapter { get; set; }

        public StatisticViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment, StatisticModel model, IPopupSetupWindow popupwin)
        {
            SetupPopup = new InteractionRequest<StatisticDataNotification>();
            ExperimentAdapter = experiment;
            ModelAdapter = model;
            PopupWinAdapter = popupwin;
            IsWellStatisticShow = false;
            IsRegionStatisticShow = false;
            ChartRangeLimit = new DoubleRange();
            var loadEvt = eventAggregator.GetEvent<ExperimentLoadedEvent>();
            loadEvt.Subscribe(RequestUpdateIExperiment);
        }

        //to do: ExperimentLoadedEvent has been changed
        private void RequestUpdateIExperiment(int scanId)
        {
            ExperimentAdapter = ServiceLocator.Current.GetInstance<IExperiment>();
        }

        public InteractionRequest<StatisticDataNotification> SetupPopup { get; private set; }

        public RunFeature CurrentRunFeature
        {
            get { return ModelAdapter.SelectedRunFeature; }
        }

        public bool IsWellStatisticShow { get; set; }
        public bool IsRegionStatisticShow { get; set; }

        public ICommand SetupStatisticCommand
        {
            get
            {
                #region Testdata
                //var sm = new StatisticModel();
                //var cl = new List<Component>() { new Component { Name = "AAA" }, new Component { Name = "BBB" } };
                //var sml = new List<StatisticMethod>() { new StatisticMethod { Name = "ADD" }, new StatisticMethod { Name = "SUM" } };
                //var fl = new List<Feature>() { new Feature { Name = "Feature1" }, new Feature { Name = "Feature2" } };
                //var chl = new List<Channel>() { new Channel { Name = "Channel1" }, new Channel { Name = "Channel2" } };
                //var rl = new List<CyteRegion>() { new CyteRegion { Name = "R1" }, new CyteRegion { Name = "R2" } };
                //var sr = new RunFeature() { Name = "S1", ComponentContainer = cl, StatisticMethodContainer = sml, FeatureContainer = fl, ChannelContainer = chl, RegionContainer = rl };
                //var sr2 = new RunFeature() { Name = "S2", ComponentContainer = cl, StatisticMethodContainer = sml, FeatureContainer = fl, ChannelContainer = chl, RegionContainer = rl };
                //var crl = new List<RunFeature>() { sr, sr2 };
                //sm.RunFeatureContainer = crl;
                //sm.ComponentContainer = cl;
                //statisticNotify.SelectedComponent = craaa;
                //statisticNotify.SelectedStatisticRecord = sr;
                #endregion  
                return new DelegateCommand(() => {
                    IsWellStatisticShow = false;
                    IsRegionStatisticShow = false;
                    OnPropertyChanged(() => IsWellStatisticShow);
                    OnPropertyChanged(() => IsRegionStatisticShow);
                    //Get Component
                    
                    var path = ExperimentAdapter.GetExperimentInfo().AnalysisPath;
                    var runfeaturelist = new List<RunFeature>();
                    if (Directory.Exists(path + StatisticModel.StatisticsPath))
                    {
                        //Directory.CreateDirectory(path + StatisticModel.StatisticsPath);
                        var statisticFiles = Directory.GetFiles(path + StatisticModel.StatisticsPath);
                        var serializer = new XmlSerializer(typeof(RunFeature));
                        runfeaturelist = statisticFiles.Select(x =>
                        {
                            RunFeature rf;
                            using (var stream = File.OpenRead(x))
                            {
                                rf = (RunFeature)serializer.Deserialize(stream);
                            }
                            return rf;
                        }).ToList();
                    }

                    var components = ComponentDataManager.Instance.GetComponentNames();
                    ModelAdapter.ComponentContainer =
                        components.Select(x =>
                        new ComponentRunFeature()
                        {
                            CurrentComponent = new Component() { Name = x },
                            RunFeatureContainer = runfeaturelist.Where( y => y.ComponentContainer.Exists( z => z.Name == x)).ToList()
                        }).ToList();
                    var wellComponent = new ComponentRunFeature() 
                        {
                            CurrentComponent = new Component() { Name = "Well" },
                            RunFeatureContainer = runfeaturelist
                        };
                    ModelAdapter.ComponentContainer.Add(wellComponent);
                    ModelAdapter.ComponentContainer.Reverse();
                    ModelAdapter.SelectedComponent = wellComponent;
                    if (PopupWinAdapter.PopupWindow())
                    {
                        OnPropertyChanged(() => WellStatisticCommand);
                        OnPropertyChanged(() => RegionStatisticCommand);
                    }
                });
            }
        }

        public ObservableCollection<DataGridColumns> GridViewColumns { 
            get {
                var columns = new ObservableCollection<DataGridColumns>();
                if (CurrentRunFeature != null)
                {
                    columns.Add(new DataGridColumns { DisplayColumnName = "Index", BindingPropertyName = "Index", Width = 65 });
                    columns.Add(new DataGridColumns { DisplayColumnName = "Well", BindingPropertyName = "Well", Width = 65 });
                    columns.Add(new DataGridColumns { DisplayColumnName = "Label", BindingPropertyName = "Label", Width = 65 });
                    columns.Add(new DataGridColumns { DisplayColumnName = "Row", BindingPropertyName = "Row", Width = 65 });
                    columns.Add(new DataGridColumns { DisplayColumnName = "Col", BindingPropertyName = "Col", Width = 65 });
                    //to do: add column dynamic
                    if (CurrentRunFeature.Name == null)
                    {
                        columns.Add(new DataGridColumns { DisplayColumnName = "Value", BindingPropertyName = "Value", Width = 1 });
                    }
                    else
                    {
                        columns.Add(new DataGridColumns { DisplayColumnName = CurrentRunFeature.Name, BindingPropertyName = "Value", Width = CurrentRunFeature.Name.Length*7 });
                    }
                    IsWellStatisticShow = true;
                    IsRegionStatisticShow = false;
                    OnPropertyChanged(() => IsWellStatisticShow);
                    OnPropertyChanged(() => IsRegionStatisticShow);
                    OnPropertyChanged(() => DataCollection);
                }
                return columns;
            }
        }

        private int GetDataIndex(Feature pFeature, Channel pChannel)
        {
            if (pFeature.IsPerChannel && pChannel != null)
            {
                return pFeature.FeatureIndex + pChannel.Index; 
            }
            else
                return pFeature.FeatureIndex;
        }

        //to do: get featurelist and channellist in advance, the method can be optimized.
        private int GetDataIndex(string pComponentName, string pFeature, string pChannel)
        {
            var featureList = ComponentDataManager.Instance.GetFeatures(pComponentName)
                .Select(x => new Feature() { Name = x.Name, IsPerChannel = x.IsPerChannel, FeatureIndex = x.Index }).ToList();
            var feature = featureList.Find(x => x.Name == pFeature);

            Channel channel = null;
            if (pChannel != "")
            {
                var channelList = ComponentDataManager.Instance.GetChannels(pComponentName)
                    .Select(x => new Channel() { Name = x.ChannelName }).ToList();
                for (int i = 0; i < channelList.Count; i++)
                {
                    channelList[i].Index = i + 1;
                }
                channel = channelList.Find(x => x.Name == pChannel);
            }

            if (feature != null && channel != null && feature.IsPerChannel)
            {
                return feature.FeatureIndex + channel.Index;
            }
            else if (feature != null)
                return feature.FeatureIndex;
            else
                return 0;
        }


        public ObservableCollection<ExpandoObject> DataCollection
        {
            get
            {
                try
                {
                    int scanid = ExperimentAdapter.GetCurrentScanId();
                    var WellList = ExperimentAdapter.GetScanInfo(scanid).ScanWellList;
                    var num = Enumerable.Range(0, WellList.Count);
                    //to do: get statistic list
                    if (CurrentRunFeature == null || !CurrentRunFeature.IsValid())
                        return null;

                    string ComponentName = CurrentRunFeature.ComponentContainer[0].Name;
                    StatisticMethod StatisticMethod = CurrentRunFeature.StatisticMethodContainer[0];
                    bool IsStatisticCount = false;
                    Feature CurrentFeature = null;
                    if (StatisticMethod.MethodType == EnumStatistic.Count)
                    {
                        IsStatisticCount = true;
                    }
                    else
                    {
                        IsStatisticCount = false;
                        CurrentFeature = CurrentRunFeature.FeatureContainer[0];
                    }

                    var GridViewRowCollection = new ObservableCollection<ExpandoObject>(num.Select(x =>
                    {
                        //to do: get specific region? where is the API
                        var aEvent = ComponentDataManager.Instance.GetEvents(ComponentName, x + 1);
                        var item = new ExpandoObject() as IDictionary<string, object>;
                        var statisticmthd = StatisticMethod.MethodType;
                        item.Add("Index", (x + 1).ToString());
                        item.Add("Well",
                            Convert.ToChar((x/12 + 1) + 64).ToString() + (x%12 == 0 ? 1 : x%12 + 1).ToString());
                        item.Add("Label", "");
                        item.Add("Row", (x/12 + 1).ToString());
                        item.Add("Col", (x%12 == 0 ? 1 : x%12 + 1).ToString());
                        //to do: add value dynamic
                        if (IsStatisticCount)
                        {
                            item.Add("Value", string.Format("{0:f4}", StatisticMethod.Method(aEvent.Select(y => 1.0f))));
                            return (ExpandoObject)item;
                        }
                        else
                        {
                            item.Add("Value", string.Format("{0:f4}", StatisticMethod.Method(
                                aEvent.Select(y =>
                                    y[
                                        GetDataIndex(CurrentFeature,
                                            CurrentRunFeature.HasChannel() ? CurrentRunFeature.ChannelContainer[0] : null)]))));
                            return (ExpandoObject)item;
                        }
                    }));
                    return GridViewRowCollection;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("{0}", e.Message);
                    return null;
                }
            }
        }


        public IDataSeries ChartDataSeries
        {
            get
            {
                if (DataCollection != null)
                {
                    var result = DataCollection.Select(x =>
                    {
                        var item = x as IDictionary<string, object>;
                        return
                            new
                            {
                                Index = int.Parse(item["Index"].ToString()),
                                Value = double.Parse(item["Value"].ToString())
                            };
                    });
                    var xydataSeries = new XyDataSeries<int, double>();
                    if (result != null && result.Any())
                    {
                        ChartRangeLimit = new DoubleRange(0, result.Max(x => x.Value)*1.2);
                        OnPropertyChanged(() => ChartRangeLimit);
                    }
                    result.ForEach(x => xydataSeries.Append(x.Index, x.Value));
                    return xydataSeries;
                }
                else
                {
                    return null;
                }
            }
        }

        public DoubleRange ChartRangeLimit { get; set; }

        //public List<WellStatisticEntry> WellStatisticDataSource { get; set; }
        
        public ICommand WellStatisticCommand { get {
            return new DelegateCommand(() => {
                OnPropertyChanged(() => GridViewColumns);
                OnPropertyChanged(() => ChartDataSeries);
            }, () => CurrentRunFeature != null);
        } }

        public List<RegionStatisticEntry> RegionStatisticDataSource { get; set; }
        public ICommand RegionStatisticCommand { get {
            return new DelegateCommand(() => {
                IsWellStatisticShow = false;
                IsRegionStatisticShow = true;
                OnPropertyChanged(() => IsWellStatisticShow);
                OnPropertyChanged(() => IsRegionStatisticShow);
                var regionstatisticlist =   
                ComponentDataManager.Instance.GetComponentNames()
                    .SelectMany(x => ROIManager.Instance.GetRegionIdList(x).Select(i => new {ComponentName = x, RegionName=i}))//get region list
                    .Distinct()
                    .Select(x => new {ComponentName=x.ComponentName, RegionName = x.RegionName, MarkRegion = ROIManager.Instance.GetRegion(x.RegionName)})//get mark region
                    .SelectMany(x =>
                        {
                            var aevent = ROIManager.Instance.GetEvents(x.RegionName);
                            if (aevent == null || aevent.Count == 0)
                            {
                                return new List<RegionStatisticEntry>{ 
                                new RegionStatisticEntry() 
                                {  
                                    RegionName = x.RegionName,
                                    Label = "", 
                                    Parameter =  (x.MarkRegion.ChannelNumeratorX == null ?"":(x.MarkRegion.ChannelNumeratorX + " ")) + x.MarkRegion.FeatureTypeNumeratorX,
                                    MeanValue = 0,
                                    MedianValue = 0,
                                    CVValue = 0
                                },
                                new RegionStatisticEntry() 
                                {  
                                    RegionName = x.RegionName,
                                    Label = "", 
                                    Parameter =  (x.MarkRegion.ChannelNumeratorY == null ?"":(x.MarkRegion.ChannelNumeratorY + " ")) + x.MarkRegion.FeatureTypeNumeratorY,
                                    MeanValue = 0,
                                    MedianValue = 0,
                                    CVValue = 0
                                }
                                };
                            }   
                            else
                            {
                                var tData = ROIManager.Instance.GetEvents(x.RegionName);
                                return new List<RegionStatisticEntry>{      
                                    new RegionStatisticEntry()
                                    {
                                        RegionName = x.RegionName,
                                        Label = "",
                                        Parameter = (x.MarkRegion.ChannelNumeratorX == null ?"":(x.MarkRegion.ChannelNumeratorX + " ")) + x.MarkRegion.FeatureTypeNumeratorX,
                                        MeanValue =  StatisticMethod.GetStatisticMethod(EnumStatistic.Mean)(
                                            tData.Select( y =>
                                                y[GetDataIndex(x.ComponentName, 
                                                               Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorX), 
                                                               x.MarkRegion.ChannelNumeratorX)])),
                                        MedianValue = StatisticMethod.GetStatisticMethod(EnumStatistic.Median)(
                                            tData.Select( y =>
                                                y[GetDataIndex(x.ComponentName, 
                                                               Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorX), 
                                                               x.MarkRegion.ChannelNumeratorX)])),
                                        CVValue = StatisticMethod.GetStatisticMethod(EnumStatistic.CV)(
                                            tData.Select( y =>
                                                y[GetDataIndex(x.ComponentName, 
                                                               Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorX), 
                                                               x.MarkRegion.ChannelNumeratorX)])),
                                   },
                                    new RegionStatisticEntry()
                                    {
                                        RegionName = x.RegionName,
                                        Label = "",
                                        Parameter = (x.MarkRegion.ChannelNumeratorY == null ?"":(x.MarkRegion.ChannelNumeratorY + " ")) + x.MarkRegion.FeatureTypeNumeratorY,
                                        MeanValue = StatisticMethod.GetStatisticMethod(EnumStatistic.Mean)(
                                            tData.Select( y =>
                                                y[GetDataIndex(x.ComponentName, 
                                                               Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorY), 
                                                               x.MarkRegion.ChannelNumeratorY)])),
                                        MedianValue = StatisticMethod.GetStatisticMethod(EnumStatistic.Median)(
                                            tData.Select( y =>
                                                y[GetDataIndex(x.ComponentName, 
                                                               Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorY), 
                                                               x.MarkRegion.ChannelNumeratorY)])),
                                        CVValue = StatisticMethod.GetStatisticMethod(EnumStatistic.CV)(
                                            tData.Select( y =>
                                                y[GetDataIndex(x.ComponentName, 
                                                               Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorY), 
                                                               x.MarkRegion.ChannelNumeratorY)])),
                                   },
                                };
                            }
                        });
                
                RegionStatisticDataSource = regionstatisticlist.Where(x => x != null).ToList();
                OnPropertyChanged(() => RegionStatisticDataSource);
            }, () => CurrentRunFeature != null);
        } }

    }
}
