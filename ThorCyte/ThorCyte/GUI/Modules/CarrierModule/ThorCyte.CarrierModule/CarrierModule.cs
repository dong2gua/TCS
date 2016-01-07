using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.CarrierModule.Views;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.CarrierModule
{
    public class CarrierModule : IModule
    {

        #region Static Members
        private static CarrierModule _uniqueInstance;
        private readonly IRegionViewRegistry _regionViewRegistry;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;

        #endregion

        #region Constructor
        public CarrierModule(IRegionViewRegistry regionViewRegistry, IEventAggregator eventAggregator, IUnityContainer container, IRegionManager regionManager)
        {
            _regionViewRegistry = regionViewRegistry;
            _eventAggregator = eventAggregator;
            _container = container;
            _regionManager = regionManager;
        }
        #endregion


        #region Methods

        public void Initialize()
        {
            _container.RegisterInstance(this);
            _eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(ShowRegionEventHandler, ThreadOption.UIThread, true);
        }

        private void ShowRegionEventHandler(string moduleName)
        {
            object theView;
            switch (moduleName)
            {
                case "ReviewModule":
                    if (_regionManager.Regions[RegionNames.ReviewCarrierRegion].GetView("CarrierView") != null)
                    {
                        return;
                    }
                    theView = _regionManager.Regions[RegionNames.AnalysisCarrierRegion].GetView("CarrierView");
                    if (theView == null)
                    {
                        theView = new CarrierView();
                    }
                    else
                    {
                        _regionManager.Regions[RegionNames.AnalysisCarrierRegion].Remove(theView);
                    }
                    _regionManager.Regions[RegionNames.ReviewCarrierRegion].Add(theView, "CarrierView");
                    break;
                case "AnalysisModule":
                    if (_regionManager.Regions[RegionNames.AnalysisCarrierRegion].GetView("CarrierView") != null)
                    {
                        return;
                    }
                    theView = _regionManager.Regions[RegionNames.ReviewCarrierRegion].GetView("CarrierView");
                    if (theView == null)
                    {
                        theView = new CarrierView();
                    }
                    else
                    {
                        _regionManager.Regions[RegionNames.ReviewCarrierRegion].Remove(theView);
                    }
                    _regionManager.Regions[RegionNames.AnalysisCarrierRegion].Add(theView, "CarrierView");
                    break;
                default:
                    break;
            }
        }

        public static CarrierModule Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = ServiceLocator.Current.GetInstance<CarrierModule>()); }
        }


        #endregion

    }
}
