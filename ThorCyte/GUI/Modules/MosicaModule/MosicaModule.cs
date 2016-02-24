using System.Collections.Generic;
using Microsoft.Practices.Unity;
using MosicaModule.Views;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;

namespace MosicaModule
{
    public class MosicaModule : IModule
    {
        private readonly IUnityContainer _unityContainer;
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;
        private MosicaView _mosicaView;


        public MosicaModule(IRegionManager regionManager, IUnityContainer unityContainer, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _unityContainer = unityContainer;
            _eventAggregator = eventAggregator;
        }

        public void Initialize()
        {
            _eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(ShowRegion, ThreadOption.UIThread, true);
        }

        private void ShowRegion(string moduleName)
        {
            if (moduleName == "ReviewModule")
            {
                if (_mosicaView == null)
                {
                    _mosicaView = _unityContainer.Resolve<MosicaView>();
                }
                IRegion mainRegion = _regionManager.Regions[RegionNames.MainRegion];
                foreach (object view in new List<object>(mainRegion.Views))
                {
                    mainRegion.Remove(view);
                }
                mainRegion.Add(_mosicaView);
                mainRegion.Activate(_mosicaView);
            }
        }
    }
}
