using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using ThorCyte.Statistic.Views;
using ThorCyte.Statistic.Models;
using ThorCyte.Infrastructure.Interfaces;
using Prism.Events;
using Microsoft.Practices.Unity;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using ComponentDataService;
using Prism.Commands;
using ROIService;

namespace ThorCyte.Statistic.ViewModels
{
    class PropertyBarViewModel:BindableBase
    {
        private IPopupDetailWindow DetailWinAdapter { get; set; }
        private StatisticModel ModelAdapter { get; set; }
        private IExperiment ExperimentAdapter { get; set; }
        public PropertyBarViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment, IPopupDetailWindow detailwin, StatisticModel model)
        {
            DetailWinAdapter = detailwin;
            ModelAdapter = model;
            ExperimentAdapter = experiment;
        }
        //internal change name use this field
        private string _RunFeatureName = "";
        //private bool isUserDefineRunFeatureName = false;

        public string RunFeatureName
        {
            get {
                if ((SelectedRunFeature == null && _RunFeatureName == "")||(SelectedRunFeature != null && !SelectedRunFeature.IsUserDefineName))
                {
                    _RunFeatureName = (SelectedComponent == null ? "" : SelectedComponent.Name)
                        + (SelectedStatisticMethod == null ? "" : " " + SelectedStatisticMethod.Name)
                        + (SelectedFeature == null ? "" : " " + SelectedFeature.Name)
                        + (SelectedChannel == null ? "" : " " + SelectedChannel.Name)
                        + (SelectedRegion == null ? "" : " " + SelectedRegion.Name);
                }
                return _RunFeatureName; 
            }
            set {
                if (SelectedRunFeature == null || (SelectedRunFeature != null && !SelectedRunFeature.IsUserDefineName))
                {
                    if(_RunFeatureName != value)
                    {
                        if (SelectedRunFeature != null)
                        {
                            SelectedRunFeature.IsUserDefineName = true;
                        }
                        else
                        {
                            _IsChangeNewFileName = true;
                        }
                    }
                }
                _RunFeatureName = value;
            }
        }

        public ICommand AddStatisticCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    IsNewFile = true;
                    SelectedRunFeature = null;
                    _RunFeatureName = String.Empty;
                    OnPropertyChanged(()=>SelectedRunFeature);
                }, () => true);
            }
        }

        public ICommand DeleteRunFeatureCommand
        {
            get
            {
                return new DelegateCommand<RunFeature>((x) =>
                {
                    var OldName = x.Name;
                    var path = ExperimentAdapter.GetExperimentInfo().AnalysisPath;
                    if (Directory.Exists(path + StatisticModel.StatisticsPath))
                    {
                       File.Delete(path + StatisticModel.StatisticsPath + "/" + OldName + ".xml");
                       SelectedRunFeature = null;
                    }
                    OnPropertyChanged(() => RunFeatureContainer);
                },(x) => true);
            }
        }

        private bool _IsChangeNewFileName = false;

        private bool _IsNewFile = false;
        public bool IsNewFile
        {
            get { return _IsNewFile; }
            set { _IsNewFile = value; }
        }

        //new or edit
        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RunFeature rf;
                    string OldName = String.Empty;
                    
                    if (IsNewFile || SelectedRunFeature == null)
                    {
                        rf = new RunFeature();
                        ModelAdapter.RunFeatureContainer.Add(rf);
                    }
                    else
                    {
                        rf = SelectedRunFeature;
                        OldName = rf.Name;    
                    }
                    rf.Name = RunFeatureName;
                    rf.ComponentContainer = new List<Component> { SelectedComponent };
                    rf.StatisticMethodContainer = new List<StatisticMethod> { SelectedStatisticMethod };
                    rf.FeatureContainer = new List<Feature> { SelectedFeature };
                    rf.ChannelContainer = new List<Channel> { SelectedChannel };
                    rf.RegionContainer = new List<CyteRegion> { SelectedRegion };
                    if (_IsChangeNewFileName)
                    {
                        rf.IsUserDefineName = true; 
                    }

                    //add to file
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
                    //to do:change to some other mode
                    rf.Name = rf.Name.Replace("/", "_");
                    using (var stream = File.Open(path + StatisticModel.StatisticsPath + "/" + rf.Name + ".xml", FileMode.Create, FileAccess.ReadWrite))
                    {
                        serializer.Serialize(stream, rf);
                    }
                    SelectedRunFeature = rf;
                    OnPropertyChanged(()=> RunFeatureContainer);

                });
            }
        }

        public List<RunFeature> RunFeatureContainer
        {
            get
            {
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
                    runfeaturelist.ForEach(x => 
                        x.StatisticMethodContainer.Where(y => y != null).ToList().ForEach(y => 
                            y.Method = StatisticMethod.GetStatisticMethod(y.MethodType)));

                    //to do: get data from files for the first time, then get the value from Model
                    ModelAdapter.RunFeatureContainer = runfeaturelist;

                    if (runfeaturelist != null)
                    {
                        if (SelectedRunFeature == null)
                        {
                            SelectedRunFeature = runfeaturelist.FirstOrDefault();
                        }
                        else
                        {
                            SelectedRunFeature = runfeaturelist.Find(x => x.Name == SelectedRunFeature.Name);
                        }
                    }
                }
                OnPropertyChanged(() => SelectedRunFeature);

                return runfeaturelist;
            }
        }

        private RunFeature _SelectedRunFeature;
        public RunFeature SelectedRunFeature
        {
            get { return _SelectedRunFeature; }
            set
            {
                _SelectedRunFeature = value;
                ModelAdapter.SelectedRunFeature = _SelectedRunFeature;
                if (ModelAdapter.SelectedRunFeature == null) //new RunFeature
                {
                    SelectedStatisticMethod = null;
                    SelectedFeature = null;
                    SelectedChannel = null;
                    SelectedRegion = null;
                    SelectedComponent = null; 

                    _RunFeatureName = "";
                    
                    OnPropertyChanged(() => ComponentContainer);
                    OnPropertyChanged(() => RunFeatureName);
                }
                else
                {
                    //to do: selected object to be list or single
                    SelectedStatisticMethod = SelectedRunFeature.StatisticMethodContainer.FirstOrDefault();
                    SelectedFeature = SelectedRunFeature.FeatureContainer.FirstOrDefault();
                    SelectedChannel = SelectedRunFeature.ChannelContainer.FirstOrDefault();
                    SelectedRegion = SelectedRunFeature.RegionContainer.FirstOrDefault();
                    SelectedComponent = SelectedRunFeature.ComponentContainer.FirstOrDefault();

                    OnPropertyChanged(() => ComponentContainer);

                    _RunFeatureName = SelectedRunFeature.Name;
                    OnPropertyChanged(() => RunFeatureName);
                }
            }
        }

        public List<Component> ComponentContainer {
            get
            {
                List<Component> result;
                //to do: recursive component?
                if (SelectedRunFeature != null)
                {
                    result =  SelectedRunFeature.ComponentContainer;
                }
                else//new runfeature
                {
                    result = ComponentData.GetComponentNames().Select(x => new Component { Name = x }).ToList();
                }
                if (SelectedComponent == null)
                {
                    SelectedComponent = result.FirstOrDefault();
                }
                OnPropertyChanged(() => SelectedComponent);
                return result;
            }
        }

        public List<StatisticMethod> StatisticMethodContainer {
            get
            {
                List<StatisticMethod> result;
                if (SelectedComponent != null)
                {
                    //to do: should not be calculate all the time
                    result = Enum.GetNames(typeof(EnumStatistic)).Select(x =>
                            new StatisticMethod()
                            {
                                Name = x,
                                MethodType = (EnumStatistic)Enum.Parse(typeof(EnumStatistic), x),
                                Method = StatisticMethod.GetStatisticMethod((EnumStatistic)Enum.Parse(typeof(EnumStatistic), x))
                            }).ToList();
                }
                else
                {
                    result = null;
                }

                //to do: add try catch
                if (result != null)
                {
                    if (SelectedStatisticMethod != null)
                    {
                        SelectedStatisticMethod = result.Find(x => x.Name == SelectedStatisticMethod.Name);
                    }
                    else
                    {
                        SelectedStatisticMethod = result.FirstOrDefault();
                    }
                }
                OnPropertyChanged(() => SelectedStatisticMethod);
                return result;
            }
        }

        [Dependency]
        public Func<IEnumerable<IComponentDataService>> ComponentDataFactory { get; set;  }

        private IComponentDataService _componentData;

        public IComponentDataService ComponentData
        {
            get
            {
                if (ComponentDataFactory().Count() > 0)
                    _componentData = ComponentDataFactory().First();
                else
                {
                    _componentData = ComponentDataManager.Instance;
                }
                return _componentData;
            }
        }

        public List<Feature> FeatureContainer
        {
            get
            {
                List<Feature> result;
                if (SelectedComponent != null)
                {
                    result = ComponentData.GetFeatures(SelectedComponent.Name)
                        .Select(
                            x => new Feature() {Name = x.Name, IsPerChannel = x.IsPerChannel, FeatureIndex = x.Index})
                        .ToList();
                }
                else
                {
                    result = null;
                }

                if (result != null)
                {
                    if (SelectedFeature != null)
                    {
                        SelectedFeature = result.Find(x => x.Name == SelectedFeature.Name);
                    }
                    else
                    {
                        SelectedFeature = result.FirstOrDefault();
                    }
                    OnPropertyChanged(() => SelectedFeature);
                }
                return result; 
            }
        }

        public List<Channel> ChannelContainer
        {
            get
            {
                List<Channel> result;
                if (SelectedComponent != null && SelectedFeature != null && SelectedFeature.IsPerChannel)
                {
                    result = ComponentData.GetChannels(SelectedComponent.Name)
                        .Select(x => new Channel() {Name = x.ChannelName}).ToList();
                    for (int i = 0; i < result.Count; i++)
                    {
                        result[i].Index = i;
                    }
                }
                else
                {
                    result = null;
                }

                if (result != null)
                {
                    if (SelectedChannel != null)
                    {
                        //to do: add try catch
                        SelectedChannel = result.Find(x => x.Name == SelectedChannel.Name);
                    }
                    else
                    {
                        SelectedChannel = result.FirstOrDefault();
                    }
                    OnPropertyChanged(() => SelectedChannel);
                }
                return result;
            }        
        }

        public List<CyteRegion> RegionContainer
        {
            get
            {
                List<CyteRegion> result;
                var None = new CyteRegion() { Name = "None" };
                if (SelectedComponent != null)
                {
                    var regionNames = ROIManager.Instance.GetRegionIdList(SelectedComponent.Name);
                    if (regionNames != null && regionNames.Any())
                    {
                        result = regionNames.Select(x => new CyteRegion() { Name = x }).ToList();
                        result.Add(None);
                        result.Reverse();

                    }
                    else
                        result = null;
                }
                else
                    result = null;

                if (result != null)
                {
                    if (SelectedRegion != null)
                    {
                        SelectedRegion = result.Find(x => x.Name == SelectedRegion.Name);
                    }
                    else
                    {
                        SelectedRegion = None;
                    }
                    OnPropertyChanged(() => SelectedRegion);
                }
                return result;  
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
                OnPropertyChanged(() => RunFeatureName);
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
