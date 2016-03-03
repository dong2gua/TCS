using ImageProcess;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.Model;
namespace ThorCyte.ImageViewerModule.Control
{
    public class HistogramPanel:Panel
    {
        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(HistogramPanel), new PropertyMetadata(0.0, OnValueChanged));
        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(HistogramPanel), new PropertyMetadata(1.0, OnScaleChanged));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(HistogramPanel), new PropertyMetadata(1.0, OnValueChanged));
        public double Contrast
        {
            get { return (double)GetValue(ContrastProperty); }
            set { SetValue(ContrastProperty, value); }
        }
        private static void OnValueChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var panel = property as HistogramPanel;
            if (panel == null) return;
            panel.RefreshChangedData();
        }
        private static void OnScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var panel = property as HistogramPanel;
            if (panel == null) return;
            panel.RefreshChangedData();
            panel.RefreshRawData();
        }
        private DrawingVisual _visualRaw;
        private DrawingVisual _visualChanged;
        private ImageData _data;
        public HistogramPanel()
        {
            _visualRaw = new DrawingVisual();
            _visualChanged = new DrawingVisual();
            this.AddVisualChild(_visualRaw);
            this.AddVisualChild(_visualChanged);
            Background = Brushes.White;
            this.SizeChanged += HistogramPanel_SizeChanged;
        }
        private void HistogramPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshChangedData();
            RefreshRawData();
        }
        public void SetData(ChannelImage channelImage)
        {
            if (channelImage.ThumbnailImageData == null) channelImage.GetThumbnailData();
            _data = channelImage.ThumbnailImageData;
            RefreshChangedData();
            RefreshRawData();
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0) return _visualRaw;
            else return _visualChanged;
        }
        protected override int VisualChildrenCount
        {
            get { return 2; }
        }
        public int[] GetHistogram()
        {
            if (_data == null) return null;
            int[] hist = new int[256];
            for (int i = 0; i < _data.Length; i++)
            {
                int n =Math.Min( _data[i] >> 6,255);
                ++hist[n];
            }
            return hist;
        }
        private void RefreshRawData()
        {
            var hist = GetHistogram();
            if (hist == null) return;
            var m_max = hist.Max();
            var h = ActualHeight;
            var w = ActualWidth;
            var drawingContext = _visualRaw.RenderOpen();
            var pen = new Pen(Brushes.Black, 1);
            for (int i = 0; i < 256; i++)
            {
                var n = hist[i] * h / m_max*Scale;
                var x = i * w / 256;
                drawingContext.DrawLine(pen,new Point( x, h), new Point(x, h - n));
            }
            drawingContext.Close();
            _visualRaw.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
        }
        private void RefreshChangedData()
        {
            if (Contrast == 1 && Brightness == 0)
            {
                var dc = _visualChanged.RenderOpen();
                dc.Close();
                return;
            }
            double c = Contrast;
            double b = Brightness/64;
            var hist = GetHistogram();
            if (hist == null) return;
            var m_max = hist.Max();
            var h = ActualHeight;
             var w = ActualWidth;
            var drawingContext = _visualChanged.RenderOpen();
            var pen = new Pen(Brushes.Red, 1);
            for (int i = 0; i < 256; i++)
            {
                var n = hist[i] * h / m_max * Scale;
                var x = (i*c+ b) * w / 256;
                drawingContext.DrawLine(pen, new Point(x, h), new Point(x, h - n));
            }
            drawingContext.Close();
            _visualChanged.Clip=  new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
        }
    }
}
