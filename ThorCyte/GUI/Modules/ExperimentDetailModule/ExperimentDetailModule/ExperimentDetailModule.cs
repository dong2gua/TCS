using Prism.Modularity;
using Prism.Regions;
using ThorCyte.ExperimentDetailModule.Views;
using ThorCyte.Infrastructure.Commom;

namespace ThorCyte.ExperimentDetailModule
{
    public class ExperimentDetailModule : IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;

        public ExperimentDetailModule(IRegionViewRegistry regionViewRegistry)
        {
            _regionViewRegistry = regionViewRegistry;
        }
        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ScanInfoRegion, typeof(ExperimentInfoView));
        }
    }
}
