using System.Windows.Controls;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.CarrierModule.Views;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.CarrierModule
{
    public class CarrierModule : IModule
    {

        #region Static Members
        private readonly IRegionViewRegistry _regionViewRegistry;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;
        private readonly CarrierView _carrierView;
        private readonly UserControl _reviewCarrierView;
        private readonly UserControl _analysisCarrierView;

        #endregion

        #region Constructor
        public CarrierModule(IRegionViewRegistry regionViewRegistry ,IEventAggregator eventAggregator, IUnityContainer container)
        {
            _regionViewRegistry = regionViewRegistry;
            _eventAggregator = eventAggregator;
            _container = container;
            _carrierView = new CarrierView();
            _reviewCarrierView = new UserControl();
            _analysisCarrierView = new UserControl();
            ShowRegionEventHandler("ReviewModule");
        }
        #endregion


        #region Methods

        public void Initialize()
        {
            _container.RegisterInstance(this);
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ReviewCarrierRegion, () => _reviewCarrierView);
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.AnalysisCarrierRegion, () => _analysisCarrierView);
            _eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(ShowRegionEventHandler, ThreadOption.UIThread, true);
        }

        private void ShowRegionEventHandler(string moduleName)
        {
            object theView;
            switch (moduleName)
            {
                case "ReviewModule":
                    if (_analysisCarrierView.Content != null)
                    {
                        _analysisCarrierView.Content = null;
                    }
                    
                    _reviewCarrierView.Content = _carrierView;

                    break;
                case "AnalysisModule":
                    if (_reviewCarrierView.Content != null)
                    {
                        _reviewCarrierView.Content = null;
                    }

                    _analysisCarrierView.Content = _carrierView;

                    break;
            }
        }


        #endregion

    }
}
