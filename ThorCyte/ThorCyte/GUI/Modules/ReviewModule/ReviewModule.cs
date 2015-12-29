﻿using System.Collections.Generic;
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
        private ReviewView _reviewView;

        public ReviewModule(IRegionManager regionManager, IEventAggregator eventAggregator, IUnityContainer container)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _reviewView = container.Resolve<ReviewView>();
        }

        public void Initialize()
        {
            _eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(EventHandler, ThreadOption.UIThread, true);
        }

        private void EventHandler(string moduleName)
        {
            if (moduleName == "ReviewModule")
            {
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
