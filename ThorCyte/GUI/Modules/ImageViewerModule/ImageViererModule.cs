using Prism.Modularity;
using Prism.Regions;
using ThorCyte.ImageViewerModule.View;
using ThorCyte.Infrastructure.Commom;

namespace ThorCyte.ImageViewerModule
{
    public class ImageViererModule : IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;
        public ImageViererModule(IRegionViewRegistry regionViewRegistry)
        {
            _regionViewRegistry = regionViewRegistry;
        }
        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ImageViewerRegion, typeof(ImageViewerView));
        }
    }
}
