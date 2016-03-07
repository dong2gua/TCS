using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.ProtocolModule.Events;

namespace ThorCyte.ProtocolModule.ViewModels
{
    public class ImageDisplayViewModel : BindableBase
    {
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand AspectRatioCommand { get; private set; }
        public ICommand ZoomCommand { get; private set; }
        public ICommand GetPositionCommand { get; private set; }
        public ICommand ResetScaleCommand { get; private set; }


        private static IEventAggregator _eventAggregator;
        private static IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;
                SetProperty(ref _title, value);
            }
        }

        private ImageSource _dispImage;
        public ImageSource DispImage
        {
            get { return _dispImage; }
            set
            {
                if (_dispImage == value) return;
                SetProperty(ref _dispImage, value);
            }
        }

        private bool _isZoomInEnable;
        public bool IsZoomInEnable
        {
            get { return _isZoomInEnable; }
            set
            {
                if (_isZoomInEnable == value) return;
                SetProperty(ref _isZoomInEnable, value);
            }
        }

        private bool _isZoomOutEnable;
        public bool IsZoomOutEnable
        {
            get { return _isZoomOutEnable; }
            set
            {
                if (_isZoomOutEnable == value) return;
                SetProperty(ref _isZoomOutEnable, value);
            }
        }


        private string _coordinate;
        public string Coordinate
        {
            get
            {
                return string.Format("{0:F2},{1:F2}", _x, _y);
            }
        }

        private double _x = 0.0;
        public double XPos
        {
            get { return _x; }
            set
            {
                SetProperty(ref _x, value);
                OnPropertyChanged("Coordinate");
            }
        }

        private double _y = 0.0;
        public double YPos
        {
            get { return _y; }
            set
            {
                SetProperty(ref _y, value);
                OnPropertyChanged("Coordinate");
            }
        }


        private double _tx = 0.0;
        public double TX
        {
            get { return _tx; }
            set
            {
                SetProperty(ref _tx, value);
            }
        }

        private double _ty = 0.0;
        public double TY
        {
            get { return _ty; }
            set
            {
                SetProperty(ref _ty, value);
            }
        }

        private double _scale = 1.0;
        public double Scale
        {
            get { return _scale; }
            set
            {
                SetProperty(ref _scale, value);
            }
        }


        private bool _isAspectRatio;
        public bool IsAspectRatio
        {
            get { return _isAspectRatio; }
            set
            {
                if (_isAspectRatio == value) return;
                SetProperty(ref _isAspectRatio, value);
            }
        }

        public void Initialize()
        {
            EventAggregator.GetEvent<DisplayImageEvent>().Subscribe(DisplayImage, ThreadOption.UIThread);
            ZoomInCommand = new DelegateCommand(OnZoomIn);
            ZoomOutCommand = new DelegateCommand(OnZoomOut);
            ZoomCommand = new DelegateCommand<MouseWheelEventArgs>(OnZoom);
            GetPositionCommand = new DelegateCommand<Image>(OnGetMousePosition);
            AspectRatioCommand = new DelegateCommand(OnSetAspectRatio);
            ResetScaleCommand = new DelegateCommand(OnResetScale);
        }


        private Image _imageObj;
        private Point pTemp = new Point(-1.0, -1.0);
        private void OnGetMousePosition(Image obj)
        {
            if (_imageObj == null)
                _imageObj = obj;

            var p = Mouse.GetPosition(_imageObj);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.SetCursor(Cursors.Hand);

                if (pTemp.X == -1.0 && pTemp.Y == -1.0)
                {
                    pTemp.X = p.X;
                    pTemp.Y = p.Y;
                }
                var dx = pTemp.X - p.X;
                var dy = pTemp.Y - p.Y;
                Move(dx, dy);
            }
            else
            {
                pTemp.X = -1.0;
                pTemp.Y = -1.0;
            }


            XPos = p.X;
            YPos = p.Y;
        }

        private void Move(double dx, double dy)
        {
            if (_scale >= 1)
            {
                TX += dx;
                TY += dy;
            }
        }


        private void OnZoom(MouseWheelEventArgs args)
        {

            if (_scale >= 1)
            {
                TX = XPos;
                TY = YPos;
            }
            else
            {
                TX = 0.0;
                TY = 0.0;
            }


            if (args.Delta > 0)
                Scale *= 1.1;
            else
            {
                Scale /= 1.1;
            }
            args.Handled = true;
        }

        private void DisplayImage(DisplayImageEventArgs args)
        {
            if (args.Title != _title) return;
            DispImage = args.Image;
        }

        public ImageDisplayViewModel()
        {
            Initialize();
        }

        public ImageDisplayViewModel(DisplayImageEventArgs args)
        {
            Initialize();
            Title = args.Title;
            DispImage = args.Image;
        }

        private void OnZoomIn()
        {
        }

        private void OnZoomOut()
        {
        }


        private void OnSetAspectRatio()
        {
            if (_isAspectRatio)
            {
            }
        }

        private void OnResetScale()
        {
            Scale = 1.0;
        }

    }
}
