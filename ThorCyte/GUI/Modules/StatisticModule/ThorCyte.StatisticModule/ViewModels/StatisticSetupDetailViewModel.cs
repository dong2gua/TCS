using ComponentDataService;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using ROIService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        public StatisticSetupDetailViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment, IPopupDetailWindow detailwin, StatisticModel model)
        {
            DetailWinAdapter = detailwin;
            ModelAdapter = model;
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
                    var rf = new RunFeature();
                    rf.Name = RunFeatureName;
                    rf.ComponentContainer = new List<Component> { SelectedComponent };
                    rf.StatisticMethodContainer = new List<StatisticMethod> { SelectedStatisticMethod };
                    rf.FeatureContainer = new List<Feature> { SelectedFeature };
                    rf.ChannelContainer = new List<Channel> { SelectedChannel };
                    rf.RegionContainer = new List<CyteRegion> { SelectedRegion };
                    ModelAdapter.RunFeatureContainer.Add(rf);
                    ModelAdapter.RunFeatureContainer = ModelAdapter.RunFeatureContainer.Distinct().ToList();
                    ModelAdapter.SelectedRunFeature = rf;
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
                    return Enum.GetNames(typeof(EnumStatistic)).Select(x =>
                            new StatisticMethod()
                            {
                                Name = x,
                                MethodType = (EnumStatistic)Enum.Parse(typeof(EnumStatistic), x),
                                Method = StatisticMethod.GetStatisticMethod((EnumStatistic)Enum.Parse(typeof(EnumStatistic), x))
                            }).ToList();
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
                    return ComponentDataManager.Instance.GetFeatures(SelectedComponent.Name)
                        .Select(x => new Feature() { Name = x.Name, IsPerChannel = x.IsPerChannel, FeatureIndex = x.Index }).ToList();
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

                        SelectedRegion = None;
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
            set { _SelectedStatisticMethod = value;
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
