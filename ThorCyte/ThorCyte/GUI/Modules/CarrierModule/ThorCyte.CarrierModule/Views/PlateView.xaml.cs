using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;
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
            buttonToolDrag.PreviewMouseDown += ToolButton_PreviewMouseDown;
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

        private void buttonFit_Checked(object sender, RoutedEventArgs e)
        {
        }

        public void UpdateScanArea()
        {
            plateCanvas.UpdateScanArea();
        }

        private void buttonAlter_Click(object sender, RoutedEventArgs e)
        {
            CarrierModule.Instance.AlterTemplate();
        }
    }
}
