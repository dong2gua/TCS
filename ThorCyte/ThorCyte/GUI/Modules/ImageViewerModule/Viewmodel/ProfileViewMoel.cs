using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThorCyte.ImageViewerModule.Model;
using Prism.Mvvm;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Collections.ObjectModel;
//using System.Drawing;
using ImageProcess;
using Xceed.Wpf.Toolkit;
using ThorCyte.ImageViewerModule.View;
using System.Windows;
using Prism.Unity;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.ImageViewerModule.Events;
namespace ThorCyte.ImageViewerModule.Viewmodel
{
    public class ProfileViewMoel : BindableBase
    {
        private ChannelImage _channel; 
        private Point? _start;
        private Point? _end;
        public ProfileViewMoel()
        {
            SeriesSource = new ObservableCollection<IChartSeriesViewModel>();
            var eventAggregator = ServiceLocator.Current.GetInstance<Prism.Events.IEventAggregator>();
            eventAggregator.GetEvent<UpdateProfilePointsEvent>().Subscribe(OnUpdateProfilePoints);
            eventAggregator.GetEvent<UpdateCurrentChannelEvent>().Subscribe(OnUpdateChannel);
        }
        public void Initialization(ChannelImage channel)
        {
            _channel = channel;
            _start = null;
            _end = null;
            SeriesSource.Clear();
        }
        private void OnUpdateProfilePoints(ProfilePoints e)
        {
            _start = e.StartPoint;
            _end = e.EndPoint;
            DisplayProfileData();
        }
        private void OnUpdateChannel(ChannelImage channel)
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
            //var buffer=data.GetDataInProfileLine(new Point(_start.Value.X, _start.Value.Y), new Point(_end.Value.X, _end.Value.Y));
            var buffer =GetDataInProfileLine(data,new Point(_start.Value.X, _start.Value.Y), new Point(_end.Value.X, _end.Value.Y));
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
                ProfileCf = ComputeContrastFactor(buffer.ToArray());
            }

        }

        public static IList<ushort> GetDataInProfileLine(ImageData data, Point start, Point end)
        {
            return GetDataInProfileLineInternal(data, (int)start.X, (int)start.Y, (int)end.X, (int)end.Y).ToList();
        }



        internal static IEnumerable<ushort> GetDataInProfileLineInternal(ImageData data, int startX, int startY, int endX,
            int endY)
        {
            bool steep = Math.Abs(endY - startY) > Math.Abs(endX - startX);
            if (steep)
            {
                int t = startX;
                startX = startY;
                startY = t;
                t = endX; // swap endX and endY
                endX = endY;
                endY = t;
            }
            if (startX > endX)
            {
                int t = startX;
                startX = endX;
                endX = t;
                t = startY; // swap startY and endY
                startY = endY;
                endY = t;
            }
            int dx = endX - startX;
            int dy = Math.Abs(endY - startY);
            int error = dx / 2;
            int ystep = (startY < endY) ? 1 : -1;
            int y = startY;
            for (int x = startX; x <= endX; x++)
            {
                int col = (steep ? y : x);
                int row = (steep ? x : y);
                yield return data.DataBuffer[row * data.XSize + col];
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
        public double ComputeContrastFactor(ushort[] sorted)
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


        private double _profileCf;
        public double ProfileCf
        {
            get { return _profileCf * 100; }
            set
            {
                SetProperty<double>(ref _profileCf, value, "ProfileCf");
            }
        }

        private DoubleRange _axisYRange = new DoubleRange(0, 0x01 << 14);
        public DoubleRange AxisYRange
        {
            get { return _axisYRange; }
            set
            {
                SetProperty<DoubleRange>(ref _axisYRange, value, "AxisYRange");
            }
        }

        public ObservableCollection<IChartSeriesViewModel> SeriesSource { get; private set; }

        public void ClearAllDataSeries()
        {
            var n = SeriesSource.Count;
            for (var i = 0; i < n; i++)
                ClearDataSeries(i);
        }

        public void ClearDataSeries(int index)
        {
            var chartSeriesViewModel = SeriesSource[index];
            var data = chartSeriesViewModel.DataSeries as XyDataSeries<double, double>;
            if (data != null)
                data.Clear();
        }

        public void AddSeriesSource(int n)
        {

            for (var i = 0; i < n; i++)
            {
                var renderSeries = new FastLineRenderableSeries
                {
                    SeriesColor = Colors.Black
                };
                var dataSeries = new XyDataSeries<double, double>();
                SeriesSource.Add(new ChartSeriesViewModel(dataSeries, renderSeries));
            }
        }

        public void SetAllVisibility(bool isVisible)
        {
            var n = SeriesSource.Count;
            for (var i = 0; i < n; i++)
                SetVisibility(i, isVisible);
        }

        public void SetVisibility(int index, bool isVisible)
        {
            var chartSeriesViewModel = SeriesSource[index];
            var renderSeries = chartSeriesViewModel.RenderSeries;
            renderSeries.IsVisible = isVisible;
        }

    }
}
