using System;
using System.Windows;
using System.Windows.Input;
using ThorCyte.CarrierModule.Carrier;
using ThorCyte.CarrierModule.Tools;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for PlateView.xaml
    /// </summary>
    public partial class PlateView
    {
        private Microplate _currentPlate;
        public Microplate CurrentPlate
        {
            set
            {
                _currentPlate = value;
                plateCanvas.Plate = _currentPlate;
            }
            get { return _currentPlate; }
        }

        public PlateView()
        {
            InitializeComponent();
        }

        public void UpdateScanArea()
        {
            plateCanvas.UpdateScanArea();
        }

    }
}
