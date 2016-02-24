using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ThorCyte.ImageViewerModule.Control
{
    public class ColorPicker : System.Windows.Controls.Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.Black, OnSelectedColorChanged));
        public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register("AvailableColors", typeof(ObservableCollection<Color>), typeof(ColorPicker));
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        public ObservableCollection<Color> AvailableColors
        {
            get { return (ObservableCollection<Color>)GetValue(AvailableColorsProperty); }
            set { SetValue(AvailableColorsProperty, value); }
        }
        SVCanvas m_ColorCanvas;
        ListView m_ColorList;
        Button m_OKButton;
        Rectangle m_ShowRect;
        Popup m_Popup;
        private byte _r;
        private byte _g;
        private byte _b;
        private double _h = 0.0;
        private double _s = 1.0;
        private double _v = 1.0;
        private string _hexColor;
        private bool _firstLoad = true;
        public string R
        {
            get { return _r.ToString(); }
            set
            {
                byte v;
                if (!byte.TryParse(value, out v)) return;
                if (v == _r) return;
                if (v > byte.MaxValue) _r = byte.MaxValue;
                else if (v < byte.MinValue) _r = byte.MinValue;
                else _r = v;
                OnRGBChanged();
                RaisePropertyChanged("R");
            }
        }
        public string G
        {
            get { return _g.ToString(); }
            set
            {
                byte v;
                if (!byte.TryParse(value, out v)) return;
                if (v == _g) return;
                if (v > byte.MaxValue) _g = byte.MaxValue;
                else if (v < byte.MinValue) _g = byte.MinValue;
                else _g = v;
                OnRGBChanged();
                RaisePropertyChanged("G");
            }
        }
        public string B
        {
            get { return _b.ToString(); }
            set
            {
                byte v;
                if (!byte.TryParse(value, out v)) return;
                if (v == _b) return;
                if (v > byte.MaxValue) _b = byte.MaxValue;
                else if (v < byte.MinValue) _b = byte.MinValue;
                else _b = v;
                OnRGBChanged();
                RaisePropertyChanged("B");
            }
        }
        public double H
        {
            get { return _h * -1; }
            set
            {
                if (value == _h * -1) return;
                _h = value * -1;
                m_ColorCanvas.UpdateColor(ColorUtilities.ConvertHsvToRgb(_h, 1, 1));
                m_ColorCanvas.RefreshRect();
                OnHSVChanged();
                RaisePropertyChanged("H");
            }
        }
        public double S
        {
            get { return _s; }
            set
            {
                if (value == _s) return;
                _s = value;
                RaisePropertyChanged("S");
            }
        }
        public double V
        {
            get { return _v; }
            set
            {
                if (value == _v) return;
                _v = value;
                RaisePropertyChanged("V");
            }
        }
        public string HexColor
        {
            get { return _hexColor; }
            set
            {
                if (value == _hexColor) return;
                _hexColor = value;
                RaisePropertyChanged("HexColor");
            }
        }
        public ColorPicker()
        {
            AvailableColors = new ObservableCollection<Color>();
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(0, 1, 1));
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(30, 1, 1));
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(60, 1, 1));
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(120, 1, 1));
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(180, 1, 1));
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(240, 1, 1));
            AvailableColors.Add(ColorUtilities.ConvertHsvToRgb(300, 1, 1));
            AvailableColors.Add(Colors.Gray);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_ColorList = GetTemplateChild("PART_ColorList") as ListView;
            m_ColorList.SelectionChanged += M_ColorList_SelectionChanged;
            m_ColorCanvas = GetTemplateChild("PART_ColorCanvas") as SVCanvas;
            m_ColorCanvas.MouseLeftButtonDown += M_ColorCanvas_MouseLeftButtonDown;
            m_ColorCanvas.MouseMove += M_ColorCanvas_MouseMove; ;
            m_OKButton = GetTemplateChild("PART_OKButton") as Button;
            m_OKButton.Click += M_OKButton_Click;
            m_ShowRect = GetTemplateChild("PART_ShowRect") as Rectangle;
            m_ShowRect.MouseLeftButtonUp += M_ShowRect_MouseLeftButtonUp;
            m_Popup = GetTemplateChild("PART_Popup") as Popup;

        }
        private static void OnSelectedColorChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var colorPicker = property as ColorPicker;
            if (colorPicker == null) return;
            colorPicker.HexColor = colorPicker.SelectedColor.ToString();
            if (colorPicker._firstLoad)
            {
                colorPicker.updateRGB();
                colorPicker.updateHSV();
                colorPicker._firstLoad = false;
            }
        }
        private void M_ShowRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_Popup.IsOpen = true;
        }
        private void M_OKButton_Click(object sender, RoutedEventArgs e)
        {
            m_Popup.IsOpen = false;
        }
        private void M_ColorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_ColorList.SelectedIndex < 0 || m_ColorList.SelectedIndex >= AvailableColors.Count) return;
            SelectedColor = AvailableColors[m_ColorList.SelectedIndex];
            updateRGB();
            updateHSV();
        }
        private void M_ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            Point p = e.GetPosition(m_ColorCanvas);
            updateSV(p);
        }
        private void M_ColorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(m_ColorCanvas);
            updateSV(p);
        }
        private void OnRGBChanged()
        {
            SelectedColor = Color.FromRgb(_r, _g, _b);
            updateHSV();
            m_ColorList.SelectedIndex = -1;
        }
        private void OnHSVChanged()
        {
            SelectedColor = ColorUtilities.ConvertHsvToRgb(_h, _s, _v);
            updateRGB();
            m_ColorList.SelectedIndex = -1;
        }
        private void updateRGB()
        {
            _r = SelectedColor.R;
            _g = SelectedColor.G;
            _b = SelectedColor.B;
            RaisePropertyChanged("R");
            RaisePropertyChanged("G");
            RaisePropertyChanged("B");
        }
        private void updateHSV()
        {
            var hsv = ColorUtilities.ConvertRgbToHsv(SelectedColor);
            if (hsv.H >= 0)
            {
                _h = hsv.H;
                RaisePropertyChanged("H");
            }
            if (hsv.S >= 0)
                _s = hsv.S;
            _v = hsv.V;

            m_ColorCanvas.UpdateColor(ColorUtilities.ConvertHsvToRgb(_h, 1, 1));
            m_ColorCanvas.RefreshRect();
            m_ColorCanvas.UpdatePoint(new Point(_s, 1 - _v));
            m_ColorCanvas.RefreshMarker();
        }
        private void updateSV(Point point)
        {
            if (point.X < 0) point.X = 0;
            else if (point.X > m_ColorCanvas.ActualWidth) point.X = m_ColorCanvas.ActualWidth;
            if (point.Y < 0) point.Y = 0;
            else if (point.Y > m_ColorCanvas.ActualHeight) point.Y = m_ColorCanvas.ActualHeight;

            _s = point.X / m_ColorCanvas.ActualWidth;
            _v = 1 - point.Y / m_ColorCanvas.ActualHeight;
            m_ColorCanvas.UpdatePoint(new Point(_s, 1 - _v));
            m_ColorCanvas.RefreshMarker();
            OnHSVChanged();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class SVCanvas : Canvas
    {
        private DrawingVisual _markerVisual;
        private DrawingVisual _rectVisual;
        private Point _point = new Point(0, 0);
        private Color _color = Colors.Red;
        public SVCanvas()
        {
            _markerVisual = new DrawingVisual();
            _rectVisual = new DrawingVisual();
            this.AddVisualChild(_rectVisual);
            this.AddVisualChild(_markerVisual);
            this.SizeChanged += SVCanvas_SizeChanged;
        }
        private void SVCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshRect();
            RefreshMarker();
        }
        protected override int VisualChildrenCount { get { return 2; } }
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0) return _rectVisual;
            else if (index == 1) return _markerVisual;
            else return null;
        }
        public void UpdatePoint(Point point)
        {
            _point = point;
        }
        public void UpdateColor(Color color)
        {
            _color = color;
        }
        public void RefreshMarker()
        {
            var drawingContext = _markerVisual.RenderOpen();
            var pen = new Pen(Brushes.Black, 1);
            drawingContext.DrawEllipse(null, pen, new Point(_point.X * this.ActualWidth, _point.Y * this.ActualHeight), 1, 1);
            drawingContext.DrawEllipse(null, pen, new Point(_point.X * this.ActualWidth, _point.Y * this.ActualHeight), 2.5, 2.5);
            drawingContext.Close();
        }
        public void RefreshRect()
        {
            var drawingContext = _rectVisual.RenderOpen();
            var sBrush = new LinearGradientBrush(Color.FromArgb(255, 255, 255, 255), _color, 0);
            var vBrush = new LinearGradientBrush(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 0, 0, 0), 90);
            drawingContext.DrawRectangle(sBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            drawingContext.DrawRectangle(vBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            drawingContext.Close();
        }
    }
    public class SpectrumSlider : Slider
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_SpectrumDisplay = GetTemplateChild("PART_SpectrumDisplay") as Rectangle;
            if (m_SpectrumDisplay == null) return;
            createSpectrum();
        }
        Rectangle m_SpectrumDisplay;
        private void createSpectrum()
        {
            var pickerBrush = new LinearGradientBrush();
            pickerBrush.StartPoint = new Point(0.5, 0);
            pickerBrush.EndPoint = new Point(0.5, 1);
            pickerBrush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
            List<Color> colorsList = new List<Color>(8);
            for (int i = 0; i < 29; i++)
                colorsList.Add(ColorUtilities.ConvertHsvToRgb(i * 12, 1, 1));
            colorsList.Add(ColorUtilities.ConvertHsvToRgb(0, 1, 1));
            double stopIncrement = (double)1 / colorsList.Count;
            for (int i = 0; i < colorsList.Count; i++)
                pickerBrush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));
            pickerBrush.GradientStops[colorsList.Count - 1].Offset = 1.0;
            m_SpectrumDisplay.Fill = pickerBrush;
        }
    }
    public static class ColorUtilities
    {
        public static HsvColor ConvertRgbToHsv(Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(r, g), b);
            v = Math.Max(Math.Max(r, g), b);
            delta = v - min;

            if (v == 0.0)
            {
                s = -1;

            }
            else
                s = delta / v;

            if (s <= 0)
                h = -1;

            else
            {
                if (r == v)
                    h = (g - b) / delta;
                else if (g == v)
                    h = 2 + (b - r) / delta;
                else if (b == v)
                    h = 4 + (r - g) / delta;

                h *= 60;
                if (h < 0.0)
                    h = h + 360;

            }

            HsvColor hsvColor = new HsvColor();
            hsvColor.H = h;
            hsvColor.S = s;
            hsvColor.V = v / 255;

            return hsvColor;

        }

        // Converts an HSV color to an RGB color.
        public static Color ConvertHsvToRgb(double h, double s, double v)
        {

            double r = 0, g = 0, b = 0;

            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                int i;
                double f, p, q, t;


                if (h == 360)
                    h = 0;
                else
                    h = h / 60;

                i = (int)Math.Truncate(h);
                f = h - i;

                p = v * (1.0 - s);
                q = v * (1.0 - (s * f));
                t = v * (1.0 - (s * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    default:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }

            }



            return Color.FromArgb(255, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));

        }

    }
    public struct HsvColor
    {

        public double H;
        public double S;
        public double V;

        public HsvColor(double h, double s, double v)
        {
            this.H = h;
            this.S = s;
            this.V = v;

        }
    }
}
