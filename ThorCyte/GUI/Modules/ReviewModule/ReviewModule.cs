using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;
using ThorCyte.ReviewModule.Views;

namespace ThorCyte.ReviewModule
{
    public class ReviewModule : IModule
    {
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        private IUnityContainer _unityContainer;
        private ReviewView _reviewView;

        public ReviewModule(IRegionManager regionManager, IEventAggregator eventAggregator, IUnityContainer container)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _unityContainer = container;
            _reviewView = null;
        }

        public void Initialize()
        {
            //_eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(EventHandler, ThreadOption.UIThread, true);
        }

        private void EventHandler(string moduleName)
        {
            if (moduleName == "ReviewModule")
            {
                if (_reviewView == null)
                {
                    _reviewView = _unityContainer.Resolve<ReviewView>();
                }
                IRegion mainRegion = _regionManager.Regions[RegionNames.MainRegion];
                foreach (object view in new List<object>(mainRegion.Views))
                {
                    mainRegion.Remove(view);
                }
                mainRegion.Add(_reviewView);
                mainRegion.Activate(_reviewView);
            }
        }
    }
}
