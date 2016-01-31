using ThorCyte.CarrierModule.Carrier;

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
        }

        public void UpdateScanArea()
        {
            slideCanvas.UpdateScanArea();
        }
    }
}
