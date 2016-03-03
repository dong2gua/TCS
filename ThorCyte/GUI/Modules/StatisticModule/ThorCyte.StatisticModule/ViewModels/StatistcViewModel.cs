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
using DependencyAttribute =  Microsoft.Practices.Unity.DependencyAttribute ;

namespace ThorCyte.Statistic.ViewModels
{

    public class ChartSeriesData
    {
        public string Name{get; set; }
        public IDataSeries Data{get; set; }
    }

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
                return new DelegateCommand(() => {
                    //IsWellStatisticShow = false;
                    //IsRegionStatisticShow = false;
                    //OnPropertyChanged(() => IsWellStatisticShow);
                    //OnPropertyChanged(() => IsRegionStatisticShow);
                    ////Get Component
                    
                    //var path = ExperimentAdapter.GetExperimentInfo().AnalysisPath;
                    //var runfeaturelist = new List<RunFeature>();
                    //if (Directory.Exists(path + StatisticModel.StatisticsPath))
                    //{
                    //    //Directory.CreateDirectory(path + StatisticModel.StatisticsPath);
                    //    var statisticFiles = Directory.GetFiles(path + StatisticModel.StatisticsPath);
                    //    var serializer = new XmlSerializer(typeof(RunFeature));
                    //    runfeaturelist = statisticFiles.Select(x =>
                    //    {
                    //        RunFeature rf;
                    //        using (var stream = File.OpenRead(x))
                    //        {
                    //            rf = (RunFeature)serializer.Deserialize(stream);
                    //        }
                    //        return rf;
                    //    }).ToList();
                    //}

                    //var components = ComponentData.GetComponentNames();
                    //ModelAdapter.ComponentContainer =
                    //    components.Select(x =>
                    //    new ComponentRunFeature()
                    //    {
                    //        CurrentComponent = new Component() { Name = x },
                    //        RunFeatureContainer = runfeaturelist.Where( y => y.ComponentContainer.Exists( z => z.Name == x)).ToList()
                    //    }).ToList();
                    //var wellComponent = new ComponentRunFeature() 
                    //    {
                    //        CurrentComponent = new Component() { Name = "Well" },
                    //        RunFeatureContainer = runfeaturelist
                    //    };
                    //ModelAdapter.ComponentContainer.Add(wellComponent);
                    //ModelAdapter.ComponentContainer.Reverse();
                    //ModelAdapter.SelectedComponent = wellComponent;
                    //if (PopupWinAdapter.PopupWindow())
                    //{
                    //    OnPropertyChanged(() => WellStatisticCommand);
                    //    OnPropertyChanged(() => RegionStatisticCommand);
                    //}
                });
            }
        }

        public ObservableCollection<DataGridColumns> GridViewColumns { 
            get {
                var columns = new ObservableCollection<DataGridColumns>();
                if (CurrentRunFeature != null)
                {
                    ModelAdapter.GetColumnGroup().ForEach(x => columns.Add(new DataGridColumns{DisplayColumnName = x, BindingPropertyName = x, Width = x.Length<10?65:x.Length * 7}));
                    //columns.Add(new DataGridColumns { DisplayColumnName = "Index", BindingPropertyName = "Index", Width = 65 });
                    //columns.Add(new DataGridColumns { DisplayColumnName = "Well", BindingPropertyName = "Well", Width = 65 });
                    //columns.Add(new DataGridColumns { DisplayColumnName = "Label", BindingPropertyName = "Label", Width = 65 });
                    //columns.Add(new DataGridColumns { DisplayColumnName = "Row", BindingPropertyName = "Row", Width = 65 });
                    //columns.Add(new DataGridColumns { DisplayColumnName = "Col", BindingPropertyName = "Col", Width = 65 });
                    ////to do: add column dynamic
                    //if (CurrentRunFeature.Name != null)
                    //{
                    //    foreach (var runFeature in ModelAdapter.RunFeatureContainer)
                    //    {
                    //        columns.Add(new DataGridColumns { DisplayColumnName = runFeature.Name, BindingPropertyName =runFeature.Name, Width = runFeature.Name.Length * 7 });
                    //    }
                    //}
                    IsWellStatisticShow = true;
                    IsRegionStatisticShow = false;
                    OnPropertyChanged(() => IsWellStatisticShow);
                    OnPropertyChanged(() => IsRegionStatisticShow);
                    OnPropertyChanged(() => DataCollection);
                }
                return columns;
            }
        }

 

        public ObservableCollection<ExpandoObject> DataCollection
        {
            get
            {
                try
                {
                    int scanid = ExperimentAdapter.GetCurrentScanId();
                    var WellList = ExperimentAdapter.GetScanInfo(scanid).ScanWellList;
                    //to do: get statistic list
                    if (CurrentRunFeature == null || !CurrentRunFeature.IsValid())
                        return null;

                    string ComponentName = CurrentRunFeature.ComponentContainer[0].Name;
                    var GridViewRowCollection = ModelAdapter.GetTableData(ComponentName, WellList.Count);
                    return GridViewRowCollection;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("{0}", e.Message);
                    return null;
                }
            }
        }

        internal class ChartData
        {
            public int Index;
            public double Value;
        }



        private bool IsNeedCalculate = true;
        private List<ChartSeriesData> _ChartDataSeries; 

        public ObservableCollection<ChartSeriesData> ChartDataSeries
        {
            get
            {
                if (IsNeedCalculate)
                {
                    //Debug.WriteLine("Begin");
                    //Debug.WriteLine(DateTime.Now.Millisecond);
                    _ChartDataSeries = ModelAdapter.RunFeatureContainer.Select(runf =>
                    {
                        if (DataCollection != null)
                        {
                            var result = DataCollection.Select(x =>
                            {
                                var item = x as IDictionary<string, object>;
                                return
                                    new ChartData()
                                    {
                                        Index = int.Parse(item["Index"].ToString()),
                                        Value = double.Parse(item[runf.Name].ToString())
                                    };
                            });
                            var tSeq = new List<ChartData>()
                            {
                                new ChartData() {Index = result.Min(x => x.Index) - 1, Value = 0},
                                new ChartData() {Index = result.Max(x => x.Index) + 1, Value = 0}
                            };
                            result = result.Concat(tSeq).OrderBy(x => x.Index);
                            var xydataSeries = new XyDataSeries<int, double>();
                            //if (result != null && result.Any())
                            //{
                            //    ChartRangeLimit = new DoubleRange(0, result.Max(x => x.Value) * 1.2);
                            //    OnPropertyChanged(() => ChartRangeLimit);
                            //}
                            //xydataSeries.Append(result.Select(x => x.Index), result.Select(x => x.Value));
                            result.ForEach(x => xydataSeries.Append(x.Index, x.Value));
                            //Debug.WriteLine("Inner");
                            //Debug.WriteLine(DateTime.Now.Millisecond);
                            return new ChartSeriesData() { Name = runf.Name, Data = (IDataSeries)xydataSeries };
                        }
                        else
                        {
                            return null;
                        }
                    }).ToList();
                    IsNeedCalculate = false;
                }
                //var head = Enumerable.Range(0, _ChartDataSeries.Count - 1);
                //var rslt = head.Select(x => new ChartSeriesData());
                //rslt.Reverse();
                //rslt = rslt.Concat(_ChartDataSeries.GetRange(0, 1));
                //rslt.Reverse();
                Debug.WriteLine("End");
                Debug.WriteLine(DateTime.Now.Millisecond);
                return new ObservableCollection<ChartSeriesData>(_ChartDataSeries);
            }
        }

        public ICommand PrevChartCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (_ChartDataSeries.Any())
                    {
                        var item = _ChartDataSeries.Last();
                        _ChartDataSeries.Remove(item);
                        _ChartDataSeries.Reverse();
                        _ChartDataSeries.Add(item);
                        _ChartDataSeries.Reverse();
                        OnPropertyChanged(() => ChartDataSeries);
                    }
                });
            }
        }

        public ICommand NextChartCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (_ChartDataSeries.Any())
                    {
                        var item = _ChartDataSeries.First();
                        _ChartDataSeries.Remove(item);
                        _ChartDataSeries.Add(item);
                        OnPropertyChanged(() => ChartDataSeries);
                    }
                });
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
            //    IsWellStatisticShow = false;
            //    IsRegionStatisticShow = true;
            //    OnPropertyChanged(() => IsWellStatisticShow);
            //    OnPropertyChanged(() => IsRegionStatisticShow);
            //    var regionstatisticlist =   
            //    ComponentDataManager.Instance.GetComponentNames()
            //        .SelectMany(x => ROIManager.Instance.GetRegionIdList(x).Select(i => new {ComponentName = x, RegionName=i}))//get region list
            //        .Distinct()
            //        .Select(x => new {ComponentName=x.ComponentName, RegionName = x.RegionName, MarkRegion = ROIManager.Instance.GetRegion(x.RegionName)})//get mark region
            //        .SelectMany(x =>
            //            {
            //                var aevent = ROIManager.Instance.GetEvents(x.RegionName);
            //                if (aevent == null || aevent.Count == 0)
            //                {
            //                    return new List<RegionStatisticEntry>{ 
            //                    new RegionStatisticEntry() 
            //                    {  
            //                        RegionName = x.RegionName,
            //                        Label = "", 
            //                        Parameter =  (x.MarkRegion.ChannelNumeratorX == null ?"":(x.MarkRegion.ChannelNumeratorX + " ")) + x.MarkRegion.FeatureTypeNumeratorX,
            //                        MeanValue = 0,
            //                        MedianValue = 0,
            //                        CVValue = 0
            //                    },
            //                    new RegionStatisticEntry() 
            //                    {  
            //                        RegionName = x.RegionName,
            //                        Label = "", 
            //                        Parameter =  (x.MarkRegion.ChannelNumeratorY == null ?"":(x.MarkRegion.ChannelNumeratorY + " ")) + x.MarkRegion.FeatureTypeNumeratorY,
            //                        MeanValue = 0,
            //                        MedianValue = 0,
            //                        CVValue = 0
            //                    }
            //                    };
            //                }   
            //                else
            //                {
            //                    var tData = ROIManager.Instance.GetEvents(x.RegionName);
            //                    return new List<RegionStatisticEntry>{      
            //                        new RegionStatisticEntry()
            //                        {
            //                            RegionName = x.RegionName,
            //                            Label = "",
            //                            Parameter = (x.MarkRegion.ChannelNumeratorX == null ?"":(x.MarkRegion.ChannelNumeratorX + " ")) + x.MarkRegion.FeatureTypeNumeratorX,
            //                            MeanValue =  StatisticMethod.GetStatisticMethod(EnumStatistic.Mean)(
            //                                tData.Select( y =>
            //                                    y[GetDataIndex(x.ComponentName, 
            //                                                   Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorX), 
            //                                                   x.MarkRegion.ChannelNumeratorX)])),
            //                            MedianValue = StatisticMethod.GetStatisticMethod(EnumStatistic.Median)(
            //                                tData.Select( y =>
            //                                    y[GetDataIndex(x.ComponentName, 
            //                                                   Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorX), 
            //                                                   x.MarkRegion.ChannelNumeratorX)])),
            //                            CVValue = StatisticMethod.GetStatisticMethod(EnumStatistic.CV)(
            //                                tData.Select( y =>
            //                                    y[GetDataIndex(x.ComponentName, 
            //                                                   Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorX), 
            //                                                   x.MarkRegion.ChannelNumeratorX)])),
            //                       },
            //                        new RegionStatisticEntry()
            //                        {
            //                            RegionName = x.RegionName,
            //                            Label = "",
            //                            Parameter = (x.MarkRegion.ChannelNumeratorY == null ?"":(x.MarkRegion.ChannelNumeratorY + " ")) + x.MarkRegion.FeatureTypeNumeratorY,
            //                            MeanValue = StatisticMethod.GetStatisticMethod(EnumStatistic.Mean)(
            //                                tData.Select( y =>
            //                                    y[GetDataIndex(x.ComponentName, 
            //                                                   Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorY), 
            //                                                   x.MarkRegion.ChannelNumeratorY)])),
            //                            MedianValue = StatisticMethod.GetStatisticMethod(EnumStatistic.Median)(
            //                                tData.Select( y =>
            //                                    y[GetDataIndex(x.ComponentName, 
            //                                                   Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorY), 
            //                                                   x.MarkRegion.ChannelNumeratorY)])),
            //                            CVValue = StatisticMethod.GetStatisticMethod(EnumStatistic.CV)(
            //                                tData.Select( y =>
            //                                    y[GetDataIndex(x.ComponentName, 
            //                                                   Enum.GetName(typeof(ComponentDataService.Types.FeatureType), x.MarkRegion.FeatureTypeNumeratorY), 
            //                                                   x.MarkRegion.ChannelNumeratorY)])),
            //                       },
            //                    };
            //                }
                //        });
                
                //RegionStatisticDataSource = regionstatisticlist.Where(x => x != null).ToList();
                //OnPropertyChanged(() => RegionStatisticDataSource);
            }, () => CurrentRunFeature != null);
        } }

    }
}
