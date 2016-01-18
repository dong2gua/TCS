using Prism.Modularity;
using Prism.Regions;
using ThorCyte.HeaderModule.Views;
using ThorCyte.Infrastructure.Commom;

namespace ThorCyte.HeaderModule
{
    public class HeaderModule :IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;

        public HeaderModule(IRegionViewRegistry regionViewRegistry)
        {
            _regionViewRegistry = regionViewRegistry;
        }
        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.HeadRegion, typeof(HeaderView));
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.TabRegion, typeof(TabView));
        }
    }
}
