using System;
using System.Windows;
using System.Windows.Input;
using ThorCyte.CarrierModule.Carrier;
using ThorCyte.CarrierModule.Tools;

namespace ThorCyte.CarrierModule.Views
{
    /// <summary>
    /// Interaction logic for SlideView.xaml
    /// </summary>
    public partial class SlideView
    {
        private Slide _currentSlide;
        public Slide CurrentSlide
        {
            set
            {
                _currentSlide = value;
                slideCanvas.SlideMod = _currentSlide;
            }
            get { return _currentSlide; }
        }

        public SlideView()
        {
            InitializeComponent();

            buttonToolSelect.PreviewMouseDown += ToolButton_PreviewMouseDown;
            buttonToolDrag.PreviewMouseDown += ToolButton_PreviewMouseDown;
            buttonToolPointer.PreviewMouseDown += ToolButton_PreviewMouseDown;
        }

        void ToolButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            slideCanvas.Tool = (ToolType)Enum.Parse(typeof(ToolType),
                ((System.Windows.Controls.Primitives.ButtonBase)sender).Tag.ToString());
            e.Handled = true;
        }

        void buttonSelectAll_Click(object sender, RoutedEventArgs args)
        {
            slideCanvas.SelectAllGraphics();
        }

        void buttonZoomIn_Click(object sender, RoutedEventArgs args)
        {
            slideCanvas.ZoomIn();
        }

        void buttonZoomOut_Click(object sender, RoutedEventArgs args)
        {
            slideCanvas.ZoomOut();
        }

        /// <summary>
        /// Auto zoom to proper size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFit_Checked(object sender, RoutedEventArgs e)
        {
        }

        public void UpdateScanArea()
        {
            slideCanvas.UpdateScanArea();
        }
    }
}
