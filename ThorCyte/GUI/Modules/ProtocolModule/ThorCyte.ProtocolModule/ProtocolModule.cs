using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Views;

namespace ThorCyte.ProtocolModule
{
    public class ProtocolModule : IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;

        public ProtocolModule()
        {
            _regionViewRegistry = ServiceLocator.Current.GetInstance<IRegionViewRegistry>();
        }

        public void Initialize()
        {
            var moduleinfos = ModuleInfoMgr.Instance;
            var macro = Macro.Instance;
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ProtocolRegion, typeof(MacroEditor));
        }
    }
}
