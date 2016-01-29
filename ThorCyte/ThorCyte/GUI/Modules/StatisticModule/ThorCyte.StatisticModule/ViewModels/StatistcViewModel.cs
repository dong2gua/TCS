using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Prism.Interactivity.InteractionRequest;
using ThorCyte.Statistic;
using ThorCyte.Statistic.Models;
using System.Collections.Generic;
using Prism.Events;
using Microsoft.Practices.Unity;
using ThorCyte.Infrastructure.Interfaces;
using ComponentDataService;
using System.Linq;
using ROIService;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace ThorCyte.Statistic.ViewModels
{
    public class StatisticViewModel : BindableBase
    {
        private IExperiment ExperimentAdapter { get; set; }
        public StatisticViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment)
        {
            SetupPopup = new InteractionRequest<StatisticDataNotification>();
            ExperimentAdapter = experiment;
        }

        public InteractionRequest<StatisticDataNotification> SetupPopup { get; private set; }

        public RunFeature CurrentRunFeature { get; set; }

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
                    var sm = new StatisticModel();
                    //Get Component
                    var components = ComponentDataManager.Instance.GetComponentNames();
                    sm.ComponentContainer = components.Select(x => new Component { Name = x }).ToList();
                    var wellComponent = new Component { Name = "Well" };
                    sm.ComponentContainer.Add(wellComponent);
                    sm.ComponentContainer.Reverse();
                    var statisticNotify = new StatisticDataNotification(sm) { Title = "StepUp", Content = "None", SelectedComponent=wellComponent, SelectedRunFeature=null };
                    SetupPopup.Raise(statisticNotify,
                 (result) => {
                     CurrentRunFeature = result.SelectedRunFeature;
                     OnPropertyChanged(() => WellStatisticCommand);
                     OnPropertyChanged(() => RegionStatisticCommand);
                 });
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
                    columns.Add(new DataGridColumns { DisplayColumnName = CurrentRunFeature.Name ?? "Value", BindingPropertyName = "Value", Width = CurrentRunFeature.Name.Length*6 });
                    OnPropertyChanged(() => DataCollection);
                }
                return columns;
            }
        }

        public ObservableCollection<ExpandoObject> DataCollection
        {
            get
            {
                if (WellStatisticDataSource != null)
                {
                    var GridViewRowCollection = new ObservableCollection<ExpandoObject>(WellStatisticDataSource.Select(x =>
                    {
                        var item = new ExpandoObject() as IDictionary<string, object>;
                        item.Add("Index", x.Index.ToString());
                        item.Add("Well", x.Well);
                        item.Add("Label", x.Label);
                        item.Add("Row", x.Row);
                        item.Add("Col", x.Col);
                        item.Add("Value", x.Value);
                        return (ExpandoObject)item;
                    }));
                    return GridViewRowCollection;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<WellStatisticEntry> WellStatisticDataSource { get; set; }
        
        public ICommand WellStatisticCommand { get {
            return new DelegateCommand(() => {
                int scanid = ExperimentAdapter.GetCurrentScanId();
                var WellList = ExperimentAdapter.GetScanInfo(scanid).ScanWellList;
                var num = new List<int>();
                for (int i = 0; i < WellList.Count; i++)
                {
                    num.Add(i);
                }
                if (CurrentRunFeature.ComponentContainer.FirstOrDefault() == null)
                    return;
                if (CurrentRunFeature.FeatureContainer.FirstOrDefault() == null)
                    return;
                int ChannelIndex = 0;
                if (CurrentRunFeature.ChannelContainer.FirstOrDefault() == null)
                    ChannelIndex = 0;
                else if (CurrentRunFeature.FeatureContainer[0].IsPerChannel)
                    ChannelIndex = CurrentRunFeature.ChannelContainer[0].Index;
                else
                    ChannelIndex = 0;

                WellStatisticDataSource = num.Select(x =>
                {
                    var aEvent = ComponentDataManager.Instance.GetEvents(CurrentRunFeature.ComponentContainer.FirstOrDefault().Name, x + 1);
                    return new WellStatisticEntry()
                    {
                        Index = (x+1).ToString(),
                        Col = (x % 12 == 0 ? 1 : x % 12 + 1).ToString(),
                        Row = (x / 12 + 1).ToString(),
                        Well = Convert.ToChar((x / 12 + 1) + 64).ToString() + (x % 12 == 0 ? 1 : x % 12 + 1).ToString(),
                        Label = "",
                        Value = aEvent.Average(y => y[CurrentRunFeature.FeatureContainer[0].FeatureIndex + ChannelIndex])
                    };
                }).ToList();
                //OnPropertyChanged(() => WellStatisticDataSource);
                OnPropertyChanged(() => GridViewColumns);
            }, () => CurrentRunFeature != null);
        } }

        public List<RegionStatisticEntry> RegionStatisticDataSource { get; set; }
        public ICommand RegionStatisticCommand { get {
            return new DelegateCommand(() => {
                //var testregions =
                //ComponentDataManager.Instance.GetComponentNames()
                //    .SelectMany(x => ROIManager.Instance.GetRegionIdList(x))
                //    .Distinct()
                //    .Select(x => ROIManager.Instance.GetRegion(x));
                var regionstatisticlist =   
                ComponentDataManager.Instance.GetComponentNames()
                    .SelectMany(x => ROIManager.Instance.GetRegionIdList(x))
                    .Distinct()
                    .Select(x => new {Name = x, MR = ROIManager.Instance.GetRegion(x)})
                    .SelectMany(x =>
                        {
                            var aevent = ROIManager.Instance.GetEvents(x.Name);
                            if (aevent == null || aevent.Count == 0)
                                return null;
                            else
                            {
                                return new List<RegionStatisticEntry>{
                                    new RegionStatisticEntry()
                                    {
                                        RegionName = x.Name,
                                        Label = "",
                                        Parameter = (x.MR.ChannelNumeratorX == null ?"":(x.MR.ChannelNumeratorX + " ")) + x.MR.FeatureTypeNumeratorX,
                                        MeanValue = ROIManager.Instance.GetEvents(x.Name).Average(y => y[5]),
                                        MedianValue = ROIManager.Instance.GetEvents(x.Name)[ROIManager.Instance.GetEvents(x.Name).Count / 2][5],
                                        CVValue = ROIManager.Instance.GetEvents(x.Name).Average(y => y[6])
                                   },
                                    new RegionStatisticEntry()
                                    {
                                        RegionName = x.Name,
                                        Label = "",
                                        Parameter = (x.MR.ChannelNumeratorY == null ?"":(x.MR.ChannelNumeratorY + " ")) + x.MR.FeatureTypeNumeratorY,
                                        MeanValue = ROIManager.Instance.GetEvents(x.Name).Average(y => y[5]),
                                        MedianValue = ROIManager.Instance.GetEvents(x.Name)[ROIManager.Instance.GetEvents(x.Name).Count / 2][5],
                                        CVValue = ROIManager.Instance.GetEvents(x.Name).Average(y => y[6])
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
