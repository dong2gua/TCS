using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace LogService
{
    public class LogServiceModule //: IModule
    {
        //private readonly IRegionViewRegistry _regionViewRegistry;
        private readonly IUnityContainer _container;

        public LogServiceModule(IRegionViewRegistry regionViewRegistry, IUnityContainer container)
        {
            //_regionViewRegistry = regionViewRegistry;
            _container = container;
        }

        public void Initialize()
        {
            //_regionViewRegistry.RegisterViewWithRegion(RegionNames.ReviewCarrierRegion, typeof(CarrierView));
            _container.RegisterInstance(this);
        }
    }
}
