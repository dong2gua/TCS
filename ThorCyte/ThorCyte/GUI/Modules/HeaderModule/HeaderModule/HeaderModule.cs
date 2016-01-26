using Prism.Modularity;
using Prism.Regions;
using ThorCyte.HeaderModule.ViewModels;
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
            var viewModel = new TabViewModel();
            var headerView = new HeaderView();
            var tabView = new TabView();
            headerView.DataContext = viewModel;
            tabView.DataContext = viewModel;

            _regionViewRegistry.RegisterViewWithRegion(RegionNames.HeadRegion,()=> { return headerView;});
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.TabRegion, () => { return tabView;});
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.MainRegion, typeof(InitialView));
        }
    }
}
