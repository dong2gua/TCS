using System;
using System.Windows.Controls;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.CarrierModule.Carrier;
using ThorCyte.CarrierModule.Views;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.CarrierModule.ViewModels
{
    public class CarrierViewModel : BindableBase
    {
        #region Static Members
        private static Carrier.Carrier _carrier;
        #endregion

        #region Fileds
        private readonly SlideView _slideView;
        private readonly PlateView _plateView;
        private TileView _tileview;
        private int _currentScanId;
        private ScanInfo _currentScanInfo;
        private string _currentCarrierType = "00000000-0000-0000-0001-000000000001";

        #endregion

        #region Properties
        private IEventAggregator _eventAggregator;
        private IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        public ScanInfo CurrentScanInfo
        {
            get { return _currentScanInfo; }
            set { _currentScanInfo = value; }
        }


        private UserControl _showingView;

        public UserControl ShowingView
        {
            get { return _showingView; }
            set { SetProperty(ref _showingView, value); }
        }

        public TileView ShowingTile
        {
            get { return _tileview; }
            set { SetProperty(ref _tileview, value); }
        }



        #endregion

        public CarrierViewModel()
        {
            CarrierDefMgr.Initialize(@"..\..\..\..\..\..\XML");
            _slideView = new SlideView();
            _plateView = new PlateView();
            _tileview = new TileView();

            var loadEvt = EventAggregator.GetEvent<ExperimentLoadedEvent>();
            loadEvt.Subscribe(RequestLoadModule);
            CreateCarrier(_currentCarrierType);
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
                ShowingView = _plateView;
                ShowingView.Tag = "PlateView";
            }
            else
            {
                _carrier = new Slide(carrierDef);
                _slideView.CurrentSlide = (Slide)_carrier;
                ShowingView = _slideView;
                ShowingView.Tag = "SlideView";
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

        private void RequestLoadModule(int scanid)
        {
            try
            {
                _currentScanId = scanid;
                _tileview.vm.SetEmptyContent();
                var exp = ServiceLocator.Current.GetInstance<IExperiment>();
                _currentCarrierType = exp.GetCarrierType();
                _currentScanInfo = exp.GetScanInfo(_currentScanId);
                _tileview.vm.CurrentScanInfo = _currentScanInfo;
                _slideView.slideCanvas.CurrentScanInfo = _currentScanInfo;
                _plateView.plateCanvas.CurrentScanInfo = _currentScanInfo;

                CreateCarrier(_currentCarrierType);
                LoadScanArea(_currentScanInfo);
            }
            catch (Exception)
            {
                CreateCarrier(_currentCarrierType);
            }
        }

    }
}
