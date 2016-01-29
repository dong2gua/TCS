using ThorCyte.Statistic.Views;
using Prism.Modularity;
using Prism.Events;
using Prism.Regions;
using Microsoft.Practices.Unity;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;
using System.Collections.Generic;
using ThorCyte.Statistic.Models;
using ThorCyte.Statistic;
using Prism.Commands;

namespace ThorCyte
{
    public class StatisticModule : IModule
    {
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        //private StatisticView _statisticView;

        public StatisticModule(IRegionManager regionManager, IEventAggregator eventAggregator, IUnityContainer container)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            //_statisticView = container.Resolve<StatisticView>();
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.StatisticRegion, typeof(StatisticView));
            //_eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(EventHandler, ThreadOption.UIThread, true);
        }

        //public void EventHandler(string pModuleName)
        //{ 
        //    if (pModuleName == "StatisticModule")
        //    {
        //        IRegion mainRegion = _regionManager.Regions[RegionNames.MainRegion];
        //        foreach (object view in new List<object>(mainRegion.Views))
        //        {
        //            mainRegion.Remove(view);
        //        }
        //        mainRegion.Add(_statisticView);
        //        mainRegion.Activate(_statisticView);
        //    }
        //}
    }
}
