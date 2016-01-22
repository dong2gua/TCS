using System;
using System.Collections.Generic;
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
        private GraphicContainerVm _graphicContainerVm;

        #region Constructor

        public HistogramView() : base(8, new Histogram {Background = Brushes.Transparent})
        {
            InitializeComponent();
            Init();
            SciChart.YAxis = YAxis;
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
            for (var i = ConstantHelper.ColorTable.Length - 1; i >= 0; i--)
            {
                SciChart.RenderableSeries.Add(new StackedColumnRenderableSeries
                {
                    AntiAliasing = false,
                    SeriesColor = ConstantHelper.ColorTable[i],
                    FillBrush = new SolidColorBrush(ConstantHelper.ColorTable[i]),
                });
            }
            SciChart.RenderableSeries.Add(new FastLineRenderableSeries { SeriesColor = Colors.Black });
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
            var count = SciChart.RenderableSeries.Count;
            if (count > RenderableSeriesCount)
            {
                for (var index = 0; index < (count - RenderableSeriesCount); index++)
                {
                    SciChart.RenderableSeries.RemoveAt(RenderableSeriesCount);
                }
            }
            if (GraphicVm.GraphType == GraphStyle.Outline)
            {
                for (var i = 0; i < RenderableSeriesCount; i++)
                {
                    SciChart.RenderableSeries[i].DataSeries.XValues.Clear();
                    SciChart.RenderableSeries[i].DataSeries.YValues.Clear();
                }
            }
            else
            {
                SciChart.RenderableSeries[7].DataSeries = null;
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
            if (GraphicVm.XAxis.IsLogScale)
            {
                SciChart.XAxis = XLogAxis;
                SciChart.XAxis.VisibleRange = new DoubleRange(GraphicVm.XAxis.MinValue, GraphicVm.XAxis.MaxValue);
            }
            else
            {
                SciChart.XAxis = XAxis;
                SciChart.XAxis.VisibleRange = new DoubleRange(GraphicVm.XAxis.MinValue, GraphicVm.XAxis.MaxValue);
            }
            SciChart.XAxis.TextFormatting = GraphicVm.XAxis.MaxValue > 1000 ? ConstantHelper.AxisMaxTextFormat : ConstantHelper.AxisMinTextFormat;
            SciChart.YAxis.TextFormatting = GraphicVm.YAxis.MaxValue > 1000 ? ConstantHelper.AxisMaxTextFormat : ConstantHelper.AxisMinTextFormat;
            YAxis.VisibleRange = new DoubleRange(GraphicVm.YAxis.MinValue, GraphicVm.YAxis.MaxValue);
        }

        private void DrawGraph()
        {
            var xscale = GraphicVm.Width / (GraphicVm.XAxis.MaxRange - GraphicVm.XAxis.MinRange);
            var outlineSeries = new XyDataSeries<double, double>();

            for (var index = 0; index < GraphicVm.Width; index++)
            {
                var x = GraphicVm.XAxis.IsLogScale ? GraphicVm.XAxis.GetActualValue(index) : index / xscale;
                if (GraphicVm.GraphType == GraphStyle.BarChart)
                {
                    int acc = 0;
                    double from = 0;

                    for (int c = 0; c < ConstantHelper.ColorCount; c++)
                    {
                        acc += ((HistogramVm)GraphicVm).ColorDataList[c][index];
                        DataSeriesArray[6 - c].Append(x, acc - from);
                        from = acc;
                    }
                    DataSeriesArray[0].Append(x, ((HistogramVm)GraphicVm).HistoryDataList[index] - acc);
                }
                else
                {
                    outlineSeries.Append(x, ((HistogramVm)GraphicVm).HistoryDataList[index]);
                }
            }


            using (SciChart.SuspendUpdates())
            {
                if (GraphicVm.GraphType == GraphStyle.Outline)
                {
                    SciChart.RenderableSeries[7].DataSeries = outlineSeries;
                }
                else
                {
                    for (var i = 0; i < 7; i++)
                    {
                        SciChart.RenderableSeries[i].DataSeries = DataSeriesArray[i];
                    }
                }
            }

            // Draw overlays if any
            if (((HistogramVm)GraphicVm).OverlayList.Count > 0 && ((HistogramVm)GraphicVm).IsCheckedOverlay)
            {
                var list = new List<KeyValuePair<Color, XyDataSeries<double, double>>>();
                foreach (var ovr in ((HistogramVm)GraphicVm).OverlayList)
                {
                    var tuple = new KeyValuePair<Color, XyDataSeries<double, double>>(ovr.OverlayColorInfo.ColorBrush.Color, new XyDataSeries<double, double>());
                    list.Add(tuple);

                    for (var i = 0; i < ovr.Datas.Count; i++)
                    {
                        tuple.Value.Append(i / xscale, ovr.Datas[i]);
                    }
                }

                var overlayCount = ((HistogramVm)GraphicVm).OverlayList.Count;

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
                GraphicVm.ViewDispatcher = Dispatcher;
                Id = GraphicVm.Id;
                SetAxis();

                SetBindings();

                SciChart.AdornerLayerCanvas.Children.Add(RegionPanel);
                _graphicContainerVm = (GraphicContainerVm)Tag;
                if (!_graphicContainerVm.GraphicDictionary.ContainsKey(Id))
                {
                    _graphicContainerVm.GraphicDictionary.Add(Id, new Tuple<GraphicUcBase, GraphicVmBase>(this, GraphicVm));
                }
            }
            GraphicVm.SetSize((int)SciChart.AnnotationUnderlaySurface.ActualWidth, (int)SciChart.AnnotationUnderlaySurface.ActualHeight);
            IsLoading = false;
        }

        public override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnSizeChanged(sender, e);
            GraphicVm.UpdateEvents();
            e.Handled = true;
        }

        #endregion
    }
}
