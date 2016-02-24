using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.GraphicModule.ViewModels;
using IEventAggregator = Prism.Events.IEventAggregator;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for HistogramView.xaml
    /// </summary>
    public partial class HistogramView
    {

        Timer resizeTimer = new Timer(500) { Enabled = false };

        #region Constructor

        public HistogramView()
            : base(8, new Histogram { Background = Brushes.Transparent })
        {
            InitializeComponent();
            Init();
            SciChart.YAxis = YAxis;
            resizeTimer.Elapsed += OnResize;
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<GraphUpdateEvent>().Subscribe(Update);
        }

        #endregion

        #region Methods

        public override void SetBindings()
        {
            base.SetBindings();

            RegionPanel.SetBinding(WidthProperty, new Binding("ActualWidth") { Source = SciChart.AnnotationUnderlaySurface });

            RegionPanel.SetBinding(HeightProperty, new Binding("ActualHeight") { Source = SciChart.AnnotationUnderlaySurface });

            SetBinding(HeightProperty, new Binding("ActualWidth") { Source = this });

            YAxis.SetBinding(AxisBase.AxisTitleProperty, new Binding("Title") { Source = GraphicVm.YAxis });
        }

        public override void InitRenderableSeries()
        {
            for (var i = 0; i < ConstantHelper.ColorTable.Length ; i++)
            {
                SciChart.RenderableSeries.Add(new StackedColumnRenderableSeries
                {
                    AntiAliasing = false,
                    SeriesColor = ConstantHelper.ColorTable[i],
                    FillBrush = new SolidColorBrush(ConstantHelper.ColorTable[i]),
                });
            }
            SciChart.RenderableSeries.Add(new FastLineRenderableSeries { SeriesColor = Colors.White });
        }

        private void ClearDataSeriesArray()
        {
            for (var index = 0; index < RenderableSeriesCount; index++)
            {
                DataSeriesArray[index].Clear();
            }
        }

        private void ClearRenderableSeries()
        {
            for (var i = 0; i < RenderableSeriesCount; i++)
            {
                if (SciChart.RenderableSeries[i].DataSeries != null)
                {
                    SciChart.RenderableSeries[i].DataSeries.XValues.Clear();
                    SciChart.RenderableSeries[i].DataSeries.YValues.Clear();
                }
            }
            if (SciChart.RenderableSeries.Count > RenderableSeriesCount)
            {
                var count = SciChart.RenderableSeries.Count - RenderableSeriesCount;
                for (var index = 0; index < count; index++)
                {
                    SciChart.RenderableSeries.RemoveAt(RenderableSeriesCount);
                }
            }
        }

        private void Update(GraphUpdateArgs args)
        {
            if (args.Id != Id)
            {
                return;
            }
            ClearDataSeriesArray();
            ClearRenderableSeries();
            SetAxis();
            DrawGraph();
        }

        public override void SetAxis()
        {
            if (!GraphicVm.XAxis.IsLogScale)
            {
                SciChart.XAxis = XAxis;
                SciChart.XAxis.TextFormatting = GraphicVm.XAxis.MaxValue >= Math.Pow(10, 6) ? ConstantHelper.AxisMaxTextFormat : string.Empty;
            }
            else
            {
                SciChart.XAxis = XLogAxis;
            }

            SciChart.XAxis.VisibleRange = new DoubleRange(GraphicVm.XAxis.MinValue, GraphicVm.XAxis.MaxValue);
            SciChart.YAxis.VisibleRange = new DoubleRange(GraphicVm.YAxis.MinValue, GraphicVm.YAxis.MaxValue);
            SciChart.YAxis.TextFormatting = GraphicVm.YAxis.MaxValue > Math.Pow(10, 6) ? ConstantHelper.AxisMaxTextFormat : string.Empty;
        }

        private void DrawGraph()
        {
            var xscale =  (GraphicVm.XAxis.MaxRange - GraphicVm.XAxis.MinRange)/(GraphicVm.Width -1);
            var outlineSeries = new XyDataSeries<double, double>();
            var histogramVm = (HistogramVm)GraphicVm;

            for (var index = 0; index < GraphicVm.Width; index++)
            {
                var x = GraphicVm.XAxis.IsLogScale ? GraphicVm.XAxis.GetActualValue(index) : index * xscale + GraphicVm.XAxis.MinRange;
                if (GraphicVm.GraphType == GraphStyle.BarChart)
                {
                    int acc = 0;
                    double from = 0;

                    for (int c = 0; c < ConstantHelper.ColorCount; c++)
                    {
                        acc += histogramVm.ColorDataList[c][index];
                        var value = (acc - from) > double.Epsilon ? (acc - from) : double.NaN;
                        DataSeriesArray[c].Append(x, value);
                        from = acc;
                    }

                    var rest = histogramVm.HistoryDataList[index] - acc;
                    DataSeriesArray[ConstantHelper.ColorCount].Append(x, rest > double.Epsilon ? rest : double.NaN);
                }
                else
                {
                    outlineSeries.Append(x, histogramVm.HistoryDataList[index]);
                }
            }


            using (SciChart.SuspendUpdates())
            {
                if (GraphicVm.GraphType == GraphStyle.Outline)
                {
                    if (outlineSeries.YMax.CompareTo(GraphicVm.YAxis.MinValue) > 0 )
                    {
                        SciChart.RenderableSeries[RenderableSeriesCount - 1].DataSeries = outlineSeries;
                    }
                }
                else
                {
                    var isValid = false;
                    for (var i = 0; i < RenderableSeriesCount - 1; i++)
                    {
                        if (DataSeriesArray[i].YMax.CompareTo(GraphicVm.YAxis.MinValue) > 0)
                        {
                            isValid = true;
                            break;
                        }
                    }
                    for (var i = 0; i < RenderableSeriesCount - 1; i++)
                    {
                        SciChart.RenderableSeries[i].DataSeries = isValid ? DataSeriesArray[i] : null;
                    }
                }
            }

            // Draw overlays if any
            if (histogramVm.OverlayList.Count > 0 && histogramVm.IsCheckedOverlay)
            {
                var list = new List<KeyValuePair<Color, XyDataSeries<double, double>>>();
                foreach (var ovr in histogramVm.OverlayList)
                {
                    var tuple = new KeyValuePair<Color, XyDataSeries<double, double>>(ovr.OverlayColorInfo.ColorBrush.Color, new XyDataSeries<double, double>());
                    list.Add(tuple);

                    for (var i = 0; i < ovr.Datas.Count; i++)
                    {
                        tuple.Value.Append(i * xscale + GraphicVm.XAxis.MinRange, ovr.Datas[i]);
                    }
                }

                var overlayCount = histogramVm.OverlayList.Count;

                using (SciChart.SuspendUpdates())
                {
                    for (var i = 0; i < overlayCount; i++)
                    {
                        SciChart.RenderableSeries.Add(new FastLineRenderableSeries { SeriesColor = list[i].Key, DataSeries = list[i].Value });
                    }
                }
            }
        }

        public override void OnLoad(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {

                GraphicVm = (GraphicVmBase)DataContext;
                Id = GraphicVm.Id;
                SetAxis();

                SetBindings();

                SciChart.AdornerLayerCanvas.Children.Add(RegionPanel);
                GraphicVm.SetSize((int)SciChart.AnnotationUnderlaySurface.ActualWidth, (int)SciChart.AnnotationUnderlaySurface.ActualHeight);
                GraphicVm.UpdateEvents();
            }

            IsLoading = false;
        }

        public override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnSizeChanged(sender, e);
            resizeTimer.Stop();
            resizeTimer.Start();
            e.Handled = true;
        }

        private void OnResize(object sender, ElapsedEventArgs e)
        {
            resizeTimer.Stop();
            if (GraphicVm == null)
            {
                return;
            }
            GraphicVm.UpdateEvents();
        }

        #endregion
    }
}
