using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.Events;
using ThorCyte.ImageViewerModule.Model;

namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ProfileViewModel : BindableBase
    {
        private string _profileCf;
        public string ProfileCf
        {
            get { return _profileCf ; }
            set { SetProperty<string>(ref _profileCf, value, "ProfileCf"); }
        }

        private DoubleRange _axisYRange = new DoubleRange(0, 0x01 << 14);
        public DoubleRange AxisYRange
        {
            get { return _axisYRange; }
            set { SetProperty<DoubleRange>(ref _axisYRange, value, "AxisYRange"); }
        }
        public ObservableCollection<IChartSeriesViewModel> SeriesSource { get; private set; }
        private ChannelImage _channel; 
        private Point? _start;
        private Point? _end;
        public ProfileViewModel()
        {
            SeriesSource = new ObservableCollection<IChartSeriesViewModel>();
            var eventAggregator = ServiceLocator.Current.GetInstance<Prism.Events.IEventAggregator>();
        }
        public void Initialization(ChannelImage channel)
        {
            SeriesSource.Clear();
            _channel = channel;
            _start = null;
            _end = null;
        }
        public void OnCloseWindow()
        {
            _channel = null;
            _start = null;
            _end = null;
        }
        public void UpdateProfilePoints(Point start,Point end)
        {
            _start = start;
            _end = end;
            DisplayProfileData();
        }
        public void UpdateChannel(ChannelImage channel)
        {
            _channel = channel;
            DisplayProfileData();
        }
        private void DisplayProfileData()
        {
            if (_start == null || _end == null || _channel == null) return;
            SeriesSource.Clear();
            if (_channel.IsComputeColor)
                DisplayProfileDataInCompuColor();
            else
                DisplayProfileDataInNormalColor();
        }
        private void DisplayProfileDataInCompuColor()
        {
            if (_channel.ComputeColorDicWithData == null) return;
            foreach(var o in _channel.ComputeColorDicWithData)
            {
                FillChartData(o.Value.Item1,o.Value.Item2);
            }
        }
        private void DisplayProfileDataInNormalColor()
        {
            FillChartData(_channel.ImageData, Colors.Black);
        }
        private void FillChartData(ImageData data, Color color)
        {
            if ((int)_start.Value.X > data.XSize || (int)_start.Value.Y > data.YSize || (int)_end.Value.X > data.XSize || (int)_end.Value.Y > data.YSize) return;
            var buffer = data.GetDataInProfileLine(new Point(_start.Value.X, _start.Value.Y), new Point(_end.Value.X, _end.Value.Y));
            if (buffer == null) return;
            var n = buffer.Count();
            var xDataSeries = new double[n];
            var yDataSeries = new double[n];
            if (n == 1)
                yDataSeries[0] = buffer[0];
            else
            {
                for (var i = 0; i < n; i++)
                {
                    xDataSeries[i] = i;
                }
                for (var i = 0; i < n; i++)
                {
                    yDataSeries[i] = buffer[i];
                }
                var dataSeries = new XyDataSeries<double, double>();
                dataSeries.Append(xDataSeries, yDataSeries);
                var renderSeries = new FastLineRenderableSeries
                {
                    SeriesColor = color
                };

                SeriesSource.Add(new ChartSeriesViewModel(dataSeries, renderSeries));
                ProfileCf = ComputeContrastFactor(buffer.ToArray()).ToString("0.00")+"%";
            }
        }
        private double ComputeContrastFactor(ushort[] sorted)
        {
            const double tolerance = 1e-6;
            Array.Sort(sorted);
            double total = 0;
            double max = 0;
            double min = 0;
            var count = 10;
            if (sorted.Length < 11)
                count = 1;
            for (var i = 0; i < sorted.Length; i++)
            {
                total += sorted[i];

                if (i < count)
                    min += sorted[i];
                if (i >= sorted.Length - count)
                    max += sorted[i];
            }
            if (Math.Abs(total) < tolerance)
                return 0;
            var factor = (max - min) / count;
            return factor * sorted.Length / total;
        }
    }
}
