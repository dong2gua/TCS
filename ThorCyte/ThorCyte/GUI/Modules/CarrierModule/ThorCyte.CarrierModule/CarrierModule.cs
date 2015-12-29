using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.CarrierModule.Carrier;
using ThorCyte.CarrierModule.Views;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule
{
    public class CarrierModule: IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;

        #region Static Members
        private static CarrierModule _uniqueInstance;
        private static Carrier.Carrier _carrier;
        #endregion

        #region Fileds
        private ICarrierView _carrierview;
        private readonly CarrierView _carrierViewA;
        private readonly CarrierViewC _carrierViewC;
        private readonly CarrierViewContainer _ccontainer;
        private readonly SlideView _slideView;
        private readonly PlateView _plateView;
        private readonly TileView _tileview;
        private int _currentScanId;
        private ScanInfo _currentScanInfo;
        private string _currentCarrierType = "00000000-0000-0000-0001-000000000001";

        #endregion

        #region Properties
        private IEventAggregator EventAggregator {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }

        public ScanInfo CurrentScanInfo
        {
            get { return _currentScanInfo; }
            set { _currentScanInfo = value; }
        }

        public string Title
        {
            get
            {
                return _carrier != null ? _carrier.Name : string.Empty;
            }
        }

        public ScanRegion ActiveRegion
        {
            get
            {
                return _carrier != null ? _carrier.ActiveRegion : null;
            }
        }

        public IList<ScanRegion> ActiveRegions
        {
            get
            {
                return _carrier != null ? _carrier.ActiveRegions : null;
            }
        }

        #endregion


        #region Constructor
        public CarrierModule()
        {
            _regionViewRegistry = ServiceLocator.Current.GetInstance<IRegionViewRegistry>();
            CarrierDefMgr.Initialize(@"..\..\..\..\..\..\XML");
            _carrierViewA = new CarrierView();
            _carrierViewC = new CarrierViewC();
            _slideView = new SlideView();
            _plateView = new PlateView();
            _tileview = new TileView();
            _ccontainer = new CarrierViewContainer();

            var loadEvt = EventAggregator.GetEvent<ExperimentLoadedEvent>();
            loadEvt.Subscribe(RequestLoadModule);
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance<CarrierModule>(this);
            _carrierview = _carrierViewA;
            _ccontainer.SetView((UserControl)_carrierview);
            CreateCarrier(_currentCarrierType);
        }
        #endregion


        #region Methods

        public void Initialize()
        {
            _carrierview = _carrierViewA;
            _ccontainer.SetView((UserControl)_carrierview);
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.ReviewCarrierRegion, ()=> _ccontainer);
        }

        public void AlterTemplate()
        {
            if (_carrierview is CarrierView)
            {
                _carrierViewA.grid.Children.Clear();
                _carrierViewA.gridTile.Children.Clear();
                _carrierview = _carrierViewC;
            }
            else
            {
                _carrierViewC.grid.Children.Clear();
                _carrierViewC.gridTile.Children.Clear();
                _carrierview = _carrierViewA;
            }

            if (_carrier is Slide)
            {
                _carrierview.SetView(_slideView, _tileview);
            }
            else
            {
                _carrierview.SetView(_plateView, _tileview);
            }
            
            _ccontainer.SetView((UserControl)_carrierview);

            //RequestLoadModule(_currentScanId);
        }

        private void RequestLoadModule(int scanid)
        {
            try
            {
                _currentScanId = scanid;
                _tileview.vm.SetEmptyContent();
                var exp = ServiceLocator.Current.GetInstance<IExperiment>();
                _currentCarrierType = exp.GetCarrierType();
                _currentScanInfo = exp.GetScanInfo(_currentScanId);

                CreateCarrier(_currentCarrierType);
                LoadScanArea(_currentScanInfo);
            }
            catch (Exception)
            {
                CreateCarrier(_currentCarrierType);
                //MessageBox.Show("Error Occured in CarrierModule, RequestLoadModule function.");
            }
        }

        public static CarrierModule Instance
        {
            get { return _uniqueInstance ?? (_uniqueInstance = ServiceLocator.Current.GetInstance<CarrierModule>()); }
        }


        public UserControl GetView()
        {
            return (UserControl)_carrierview;
        }

        /// <summary>
        /// Create and show carrier on UI
        /// </summary>
        /// <param name="refId"></param>
        public void CreateCarrier(string refId)
        {
            var def = CarrierDefMgr.Instance.GetCarrierDef(refId, false);
            if (def != null)
            {
                SetCarrier(def);
            }
        }

        public void SetCarrier(CarrierDef carrierDef)
        {
            if (carrierDef.Type == CarrierType.Microplate)
            {
                _carrier = new Microplate(carrierDef);
                _plateView.CurrentPlate = (Microplate)_carrier;
                _carrierview.SetView(_plateView,_tileview);
                ((UserControl)_carrierview).Tag = "PlateView";
            }
            else
            {
                _carrier = new Slide(carrierDef);
                _slideView.CurrentSlide = (Slide)_carrier;
                _carrierview.SetView(_slideView,_tileview);
                ((UserControl)_carrierview).Tag = "SlideView";
            }
        }

        public void LoadScanArea(ScanInfo info)
        {
            if (_carrier != null)
                
                _carrier.TotalRegions = info.ScanRegionList;

            if (_carrier is Slide)
            {
                _slideView.UpdateScanArea();
            }
            else
            {
                _plateView.UpdateScanArea();
            }
        }
        #endregion

    }

    public interface ICarrierView
    {
        void SetView(UserControl ctlCarri, UserControl ctlTile);
    }
}
