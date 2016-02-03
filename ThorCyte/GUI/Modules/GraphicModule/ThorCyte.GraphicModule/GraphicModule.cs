using Prism.Modularity;
using Prism.Regions;
using ThorCyte.GraphicModule.ViewModels;
using ThorCyte.GraphicModule.Views;
using ThorCyte.Infrastructure.Commom;

namespace ThorCyte.GraphicModule
{
    public class GraphicModule : IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;

        public static GraphicManagerVm GraphicManagerVmInstance
        {
           get { return _graphicManagerVmInstance; } 
        }

        private static GraphicManagerVm _graphicManagerVmInstance;

        public GraphicModule(IRegionViewRegistry regionViewRegistry)
        {
            _regionViewRegistry = regionViewRegistry;
        }

        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.GraphicRegion, typeof(GraphicModeuleView));
        }

        public static void RegisterGaphicManager(GraphicManagerVm graphicManagerVm)
        {
            _graphicManagerVmInstance = graphicManagerVm;
        }
    }
}