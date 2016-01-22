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

            buttonToolSelect.PreviewMouseDown += ToolButton_PreviewMouseDown;
            buttonToolPointer.PreviewMouseDown += ToolButton_PreviewMouseDown;
        }

        void ToolButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            plateCanvas.Tool = (ToolType)Enum.Parse(typeof(ToolType),
                ((System.Windows.Controls.Primitives.ButtonBase)sender).Tag.ToString());
            e.Handled = true;
        }

        private void ButtonSelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            plateCanvas.SelectAllGraphics();
        }

        private void buttonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            plateCanvas.ZoomIn();
        }

        private void buttonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            plateCanvas.ZoomOut();
        }

        public void UpdateScanArea()
        {
            plateCanvas.UpdateScanArea();
        }

    }
}
