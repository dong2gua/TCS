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
    public class CarrierModule: IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;

        #region Static Members
        private static CarrierModule _uniqueInstance;
        #endregion

        #region Constructor
        public CarrierModule(IRegionViewRegistry regionViewRegistry, IEventAggregator eventAggregator, IUnityContainer container)
        {
            _regionViewRegistry = regionViewRegistry;
            _eventAggregator = eventAggregator;
            _container = container;
        }
        #endregion


        #region Methods

        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ReviewCarrierRegion, typeof(CarrierView));
            //_regionViewRegistry.RegisterViewWithRegion(RegionNames.AnalysisCarrierRegion, typeof (CarrierView));
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance(this);
            _eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(ShowRegionEventHandler, ThreadOption.UIThread, true);
        }

        private void ShowRegionEventHandler(string moduleName)
        {
            switch (moduleName)
            {
                case "ReviewModule":
                    


                    break;
                case "AnalysisModule":
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
