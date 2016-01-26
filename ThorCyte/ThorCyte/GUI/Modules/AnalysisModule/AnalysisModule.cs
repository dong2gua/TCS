using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.AnalysisModule.Views;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.AnalysisModule
{
    public class AnalysisMoudel : IModule
    {
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        private IUnityContainer _unityContainer;
        private AnalysisView _analysisView;

        public AnalysisMoudel(IRegionManager regionManager, IEventAggregator eventAggregator, IUnityContainer container)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _unityContainer = container;
            _analysisView = null;
        }

        public void Initialize()
        {
            //_eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(EventHandler, ThreadOption.UIThread, true);
        }

        private void EventHandler(string moduleName)
        {
            if (moduleName == "AnalysisModule")
            {
                if (_analysisView == null)
                {
                    _analysisView = _unityContainer.Resolve<AnalysisView>();
                }
                IRegion mainRegion = _regionManager.Regions[RegionNames.MainRegion];
                foreach (object view in new List<object>(mainRegion.Views))
                {
                    mainRegion.Remove(view);
                }
                mainRegion.Add(_analysisView);
                mainRegion.Activate(_analysisView);
            }
        }
    }
}
