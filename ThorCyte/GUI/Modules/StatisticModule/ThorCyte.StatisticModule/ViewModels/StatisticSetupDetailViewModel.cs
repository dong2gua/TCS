using ComponentDataService;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using ROIService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Statistic.Models;
using ThorCyte.Statistic.Views;

namespace ThorCyte.Statistic.ViewModels
{
    class StatisticSetupDetailViewModel : BindableBase
    {
        //private StatisticDataNotification notification; 
        private IPopupDetailWindow DetailWinAdapter { get; set; }
        private StatisticModel ModelAdapter { get; set; }
        private IExperiment ExperimentAdapter { get; set; }
        public StatisticSetupDetailViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment, IPopupDetailWindow detailwin, StatisticModel model)
        {
            DetailWinAdapter = detailwin;
            ModelAdapter = model;
            ExperimentAdapter = experiment;
        }

        //public Action FinishInteraction { get; set; }

        ////Receieve Notification from Setup
        //public INotification Notification
        //{
        //    get
        //    {
        //        return this.notification;
        //    }
        //    set
        //    {
        //        if(value is StatisticDataNotification)
        //        {
        //            this.notification = value as StatisticDataNotification;
        //            if (this.notification.SelectedRunFeature != null && this.notification.SelectedRunFeature.Name != "")
        //            {
        //                _RunFeatureName = this.notification.SelectedRunFeature.Name;
        //                isUserDefineRunFeatureName = true;
        //            }
        //            OnPropertyChanged(() => ComponentContainer);
        //        }
        //    }
        //}

        private string _RunFeatureName = "";
        private bool isUserDefineRunFeatureName = false;

        public string RunFeatureName
        {
            get {
                if (!isUserDefineRunFeatureName)
                {
                    _RunFeatureName = (SelectedComponent == null ? "" : SelectedComponent.Name)
                        + (SelectedStatisticMethod == null ? "" : " " + SelectedStatisticMethod.Name)
                        + (SelectedFeature == null ? "" : " " + SelectedFeature.Name)
                        + (SelectedChannel == null ? "" : " " + SelectedChannel.Name)
                        + (SelectedRegion == null ? "" : " " + SelectedRegion.Name);
                }
                return _RunFeatureName; 
            }
            set { _RunFeatureName = value;
              isUserDefineRunFeatureName = true;
            }
        }

        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RunFeature rf;
                    bool IsNewFile = true;
                    string OldName = String.Empty;
                    
                    if (ModelAdapter.SelectedRunFeature != null)
                    {
                        rf = ModelAdapter.SelectedRunFeature;
                    }
                    else
                    {
                        rf = new RunFeature();
                    }
                    if (rf.Name != RunFeatureName)
                    {
                        IsNewFile = false;
                        OldName = rf.Name;    
                    }
                    rf.Name = RunFeatureName;
                    rf.ComponentContainer = new List<Component> { SelectedComponent };
                    rf.StatisticMethodContainer = new List<StatisticMethod> { SelectedStatisticMethod };
                    rf.FeatureContainer = new List<Feature> { SelectedFeature };
                    rf.ChannelContainer = new List<Channel> { SelectedChannel };
                    rf.RegionContainer = new List<CyteRegion> { SelectedRegion };
                    ModelAdapter.RunFeatureContainer.Add(rf);
                    ModelAdapter.RunFeatureContainer = ModelAdapter.RunFeatureContainer.Distinct().ToList();
                    ModelAdapter.SelectedRunFeature = rf;

                    var path = ExperimentAdapter.GetExperimentInfo().AnalysisPath;
                    if (!Directory.Exists(path + StatisticModel.StatisticsPath))
                    {
                        Directory.CreateDirectory(path + StatisticModel.StatisticsPath);
                    }

                    var serializer = new XmlSerializer(typeof(RunFeature));
                    if (!IsNewFile)
                    {
                        File.Delete(path + StatisticModel.StatisticsPath + "/" + OldName + ".xml");
                    }
                    using (var stream = File.Open(path + StatisticModel.StatisticsPath + "/" + rf.Name + ".xml", FileMode.OpenOrCreate))
                    {
                        serializer.Serialize(stream, rf);
                    }
                    DetailWinAdapter.Close();
                });
            }
        }

        public List<Component> ComponentContainer {
            get
            {
                //to do: recursive component
                if (ModelAdapter != null && ModelAdapter.SelectedRunFeature != null)
                {
                    //to do: selected object to be list or single
                    SelectedComponent = ModelAdapter.SelectedRunFeature.ComponentContainer.FirstOrDefault();
                    SelectedStatisticMethod = ModelAdapter.SelectedRunFeature.StatisticMethodContainer.FirstOrDefault();
                    SelectedFeature = ModelAdapter.SelectedRunFeature.FeatureContainer.FirstOrDefault();
                    SelectedChannel = ModelAdapter.SelectedRunFeature.ChannelContainer.FirstOrDefault();
                    SelectedRegion = ModelAdapter.SelectedRunFeature.RegionContainer.FirstOrDefault();

                    return ModelAdapter.SelectedRunFeature.ComponentContainer;
                }
                else//new runfeature
                {
                    return ComponentDataManager.Instance.GetComponentNames().Select(x => new Component { Name = x }).ToList();
                }
            }
        }

        public List<StatisticMethod> StatisticMethodContainer {
            get
            {
                if (SelectedComponent != null)
                {
                    //to do: should not be calculate all the time
                    var result = Enum.GetNames(typeof(EnumStatistic)).Select(x =>
                            new StatisticMethod()
                            {
                                Name = x,
                                MethodType = (EnumStatistic)Enum.Parse(typeof(EnumStatistic), x),
                                Method = StatisticMethod.GetStatisticMethod((EnumStatistic)Enum.Parse(typeof(EnumStatistic), x))
                            }).ToList();
                    //to do: add try catch
                    if (SelectedStatisticMethod != null)
                    {
                        SelectedStatisticMethod = result.Find(x => x.Name == SelectedStatisticMethod.Name);
                        OnPropertyChanged(() => SelectedStatisticMethod);
                    }
                    return result;
                }
                else
                {
                    return null;   
                }
            }
        }

        public List<Feature> FeatureContainer
        {
            get
            {
                if (SelectedComponent != null)
                {
                    var result = ComponentDataManager.Instance.GetFeatures(SelectedComponent.Name)
                        .Select(
                            x => new Feature() { Name = x.Name, IsPerChannel = x.IsPerChannel, FeatureIndex = x.Index })
                        .ToList();
                    if (SelectedFeature != null)
                    {
                        SelectedFeature = result.Find(x => x.Name == SelectedFeature.Name);
                        OnPropertyChanged(() => SelectedFeature);
                    }
                    return result;
                }
                else
                    return null;
            }
        }

        public List<Channel> ChannelContainer
        {
            get
            {
                if (SelectedComponent != null && SelectedFeature != null && SelectedFeature.IsPerChannel)
                {
                    var cc = ComponentDataManager.Instance.GetChannels(SelectedComponent.Name)
                        .Select(x => new Channel() { Name = x.ChannelName }).ToList();
                    for (int i = 0; i < cc.Count; i++)
                    {
                        cc[i].Index = i+1;
                    }
                    if (SelectedChannel != null)
                    {
                        //to do: add try catch
                        SelectedChannel = cc.Find(x => x.Name != SelectedChannel.Name);
                        OnPropertyChanged(() => SelectedChannel);
                    }
                    return cc;
                }
                else
                    return null;    
            }        
        }

        public List<CyteRegion> RegionContainer
        {
            get
            {
                if (SelectedComponent != null)
                {
                    var regionNames = ROIManager.Instance.GetRegionIdList(SelectedComponent.Name);
                    if (regionNames != null && regionNames.Any())
                    {
                        var regions = regionNames.Select(x => new CyteRegion() { Name = x }).ToList();
                        var None = new CyteRegion() { Name = "None" };
                        regions.Add(None);
                        regions.Reverse();

                        if (SelectedRegion != null)
                        {
                            SelectedRegion = regions.Find(x => x.Name != SelectedRegion.Name);
                        }
                        else
                        {
                            SelectedRegion = None;
                        }
                        OnPropertyChanged(() => SelectedRegion);
                        return regions;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
        }

        private Component _SelectedComponent;

        public Component SelectedComponent
        {
            get { return _SelectedComponent; }
            set
            {
                _SelectedComponent = value;
                OnPropertyChanged(() => StatisticMethodContainer);
                OnPropertyChanged(() => RegionContainer);
                OnPropertyChanged(() => RunFeatureName);
            }
        }
        private StatisticMethod _SelectedStatisticMethod;

        public StatisticMethod SelectedStatisticMethod
        {
            get { return _SelectedStatisticMethod; }
            set
            {
                _SelectedStatisticMethod = value;
                OnPropertyChanged(() => FeatureContainer);
            }
        }
        private Feature _SelectedFeature;

        public Feature SelectedFeature
        {
            get { return _SelectedFeature; }
            set
            {
                _SelectedFeature = value;
                OnPropertyChanged(() => ChannelContainer);
                OnPropertyChanged(() => RunFeatureName);
            }
        }
        private Channel _SelectedChannel;

        public Channel SelectedChannel
        {
            get { return _SelectedChannel; }
            set
            {
                _SelectedChannel = value;
                OnPropertyChanged(() => RunFeatureName);
            }
        }
        private CyteRegion _SelectedRegion;

        public CyteRegion SelectedRegion
        {
            get { return _SelectedRegion; }
            set
            {
                _SelectedRegion = value;
                OnPropertyChanged(() => RunFeatureName);
            }
        }
    }
}
