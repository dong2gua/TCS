using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Visuals.Axes;
using Abt.Controls.SciChart.Visuals.PointMarkers;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.ViewModels;
using IEventAggregator = Prism.Events.IEventAggregator;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ScattergramView 
    {
        #region Fields

        private LogarithmicNumericAxis _yLogAxis;

        private readonly Dictionary<int, List<Point>> _dotDictionary = new Dictionary<int, List<Point>>();

        public Point QuadrantCenterPoint = new Point(double.NaN, double.NaN);

        private GraphicContainerVm _graphicContainerVm;

        #endregion

        #region Constructor

        public ScattergramView() : base(8, new Scattergram {Background = Brushes.Transparent})
        {
            InitializeComponent();
            Init();
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<GraphUpdateEvent>().Subscribe(Update);
        }

        #endregion

        #region Events

        public override sealed void Init()
        {
            for (var index = 0; index < RenderableSeriesCount; index++)
            {
                _dotDictionary.Add(index, new List<Point>());
            }
            base.Init();
        }

        public override void InitRenderableSeries()
        {
            for (var i = 0; i < ConstantHelper.BrushTable.Length - 1; i++)
            {
                SciChart.RenderableSeries.Add(new XyScatterRenderableSeries
                {
                    PointMarker = new EllipsePointMarker
                    {
                        Width = 5,
                        Height = 5,
                        Stroke = ((SolidColorBrush)ConstantHelper.BrushTable[i]).Color,
                        Fill = ((SolidColorBrush)ConstantHelper.BrushTable[i]).Color
                    }
                });
            }

            for (var i = 0; i < ConstantHelper.TemperatureColors.Length; i++)
            {
                SciChart.RenderableSeries.Add(new XyScatterRenderableSeries
                {
                    PointMarker = new EllipsePointMarker
                    {
                        Width = 5,
                        Height = 5,
                        Stroke = ConstantHelper.TemperatureColors[i],
                        Fill = ConstantHelper.TemperatureColors[i]
                    }
                });
            }
        }

        public override void InitAxis()
        {
            base.InitAxis();
            _yLogAxis = new LogarithmicNumericAxis
            {
                AutoRange = AutoRange.Never,
                AxisAlignment = AxisAlignment.Left,
                GrowBy = new DoubleRange(0.1, 0.1),
                DrawMinorTicks = true,
                DrawMinorGridLines = false,
                DrawMajorGridLines = false,
                DrawMajorBands = false
            };
        }

        private void Clear()
        {
            for (var index = 0; index < DataSeriesArray.Length; index++)
            {
                DataSeriesArray[index].Clear();
            }
            for (var index = 0; index < _dotDictionary.Count; index++)
            {
                _dotDictionary[index].Clear();
            }
            if (!IsLoading)
            {
                //SciChart.GridLinesPanel.Background = ((ScattergramVm)GraphicVm).IsWhiteBackground ? Brushes.White : Brushes.Black;
            }

            //((XyScatterRenderableSeries)SciChart.RenderableSeries[6]).PointMarker.Fill =
            //   ((ScattergramVm)GraphicVm).IsWhiteBackground ? Colors.Black : Colors.White;
        }

        public override void SetBindings()
        {
            base.SetBindings();
            //RegionPanel.SetBinding(Scattergram.IsWhiteBackgroudProperty, new Binding("IsWhiteBackground") { Source = GraphicVm, NotifyOnSourceUpdated = true });

            RegionPanel.SetBinding(Scattergram.IsShowQuadrantProperty, new Binding("IsShowQuadrant") { Source = GraphicVm });

            RegionPanel.SetBinding(WidthProperty, new Binding("ActualWidth") { Source = SciChart.AnnotationUnderlaySurface });

            RegionPanel.SetBinding(HeightProperty, new Binding("ActualHeight") { Source = SciChart.AnnotationUnderlaySurface });

            SetBinding(HeightProperty, new Binding("ActualWidth") { Source = this });

            var yaxisBinding = new Binding("Title") { Source = GraphicVm.YAxis };

            YAxis.SetBinding(AxisBase.AxisTitleProperty, yaxisBinding);
            _yLogAxis.SetBinding(AxisBase.AxisTitleProperty, yaxisBinding);
        }

        private void Update(GraphUpdateArgs args)
        {
            if (args.Id != Id)
            {
                return;
            }

            Clear();
            SetAxis();
            DrawPlot();
        }

        public override sealed void SetAxis()
        {
            SciChart.XAxis = GraphicVm.XAxis.IsLogScale ? XLogAxis : XAxis;
            SciChart.YAxis = GraphicVm.YAxis.IsLogScale ? _yLogAxis : YAxis;

            SciChart.XAxis.VisibleRange = new DoubleRange(GraphicVm.XAxis.MinValue, GraphicVm.XAxis.MaxValue);
            SciChart.YAxis.VisibleRange = new DoubleRange(GraphicVm.YAxis.MinValue, GraphicVm.YAxis.MaxValue);
            //SciChart.XAxis.TextFormatting = GraphicVm.XAxis.MaxValue > 1000 ? ConstantHelper.AxisMaxTextFormat : ConstantHelper.AxisMinTextFormat;
            //SciChart.YAxis.TextFormatting = GraphicVm.YAxis.MaxValue > 1000 ? ConstantHelper.AxisMaxTextFormat : ConstantHelper.AxisMinTextFormat;
        }

        //private void DrawPlot()
        //{
        //    SciChart.XAxis.VisibleRange = new DoubleRange(112130.0, 114630.0);
        //    SciChart.YAxis.VisibleRange = new DoubleRange(10780.0, 11700.0);
        //    var xList = new List<double> { 112167.0, 113268.0, 113760.0, 112681.0, 112669.0, 113610.0, 113459.0, 113450.0, 114406.0, 112674.0, 114100.0, 113934.0, 112324.0, 113168.0, 112166.0 };
        //    var yList = new List<double> { 10950.0, 10826.0, 10843.0, 11105.0, 11131.0, 11066.0, 11130.0, 11136.0, 11113.0, 11275.0, 11228.0, 11277.0, 11440.0, 11440.0, 11591.0 };


        //    //SciChart.XAxis.VisibleRange = new DoubleRange(0, 20);
        //    //SciChart.YAxis.VisibleRange = new DoubleRange(0, 20);
        //    //var xList = new List<double> { 10, 15 };
        //    //var yList = new List<double> { 18, 12 };
        //    DataSeriesArray[0].Append(xList, yList);
        //    SciChart.RenderableSeries[0].DataSeries = DataSeriesArray[0];
        //}

        private void DrawPlot()
        {
            for (var index = 0; index < SciChart.RenderableSeries.Count; index++)
            {
                SciChart.RenderableSeries[index].DataSeries = null;
            }
            if (((ScattergramVm)GraphicVm).DotTupleList.Count == 0)
            {
                return;
            }

            foreach (var tuple in ((ScattergramVm)GraphicVm).DotTupleList)
            {
                _dotDictionary[tuple.Item2].Add(tuple.Item1);
            }

            var baseIndex = ((ScattergramVm)GraphicVm).IsMapChecked ? 7 : 0; //3d plot color begin with 8
            var xscale = GraphicVm.Width / (GraphicVm.XAxis.MaxRange - GraphicVm.XAxis.MinRange);
            var yscale = GraphicVm.Height / (GraphicVm.YAxis.MaxRange - GraphicVm.YAxis.MinRange);

            var pointList = new List<Point>();

            if (!((ScattergramVm)GraphicVm).IsMapChecked)
            {
                foreach (var item in _dotDictionary)
                {
                    if (item.Value.Count == 0)
                    {
                        continue;
                    }
                    var index = item.Key;
                    var xList = item.Value.Select(pt => pt.X).ToList();
                    var yList = item.Value.Select(pt => pt.Y).ToList();
                    var physicalList = item.Value.Select(pt => new Point(pt.X * xscale, GraphicVm.Height - pt.Y * yscale)).ToList();
                    pointList.AddRange(physicalList);
                    DataSeriesArray[index].Append(xList, yList);
                    SciChart.RenderableSeries[baseIndex + index].DataSeries = DataSeriesArray[index];
                }
            }
            else
            {

                foreach (var item in _dotDictionary)
                {
                    if (item.Value.Count == 0)
                    {
                        continue;
                    }
                    var index = item.Key;
                    var xList = item.Value.Select(pt => GraphicVm.XAxis.IsLogScale ? GraphicVm.XAxis.GetActualValue((int)pt.X) : pt.X / xscale).ToList();
                    var yList = item.Value.Select(pt => GraphicVm.YAxis.IsLogScale ? GraphicVm.YAxis.GetActualValue((int)pt.Y) : pt.Y / yscale).ToList();

                    var physicalList = item.Value.Select(pt => GraphicVm.XAxis.IsLogScale ? pt : new Point(pt.X / xscale, pt.Y / yscale)).ToList();
                    pointList.AddRange(physicalList);
                    DataSeriesArray[index].Append(xList, yList);
                    SciChart.RenderableSeries[baseIndex + index].DataSeries = DataSeriesArray[index];
                }
            }
            ((Scattergram)RegionPanel).Update(pointList);
        }


        public override void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!double.IsNaN(QuadrantCenterPoint.X) && !double.IsNaN(QuadrantCenterPoint.Y))
            {
                ((Scattergram)RegionPanel).QuadrantCenterPoint = QuadrantCenterPoint;
            }
            
            if (IsLoading)
            {
                GraphicVm = (GraphicVmBase)DataContext;
                GraphicVm.ViewDispatcher = Dispatcher;
                Id = GraphicVm.Id;
                _graphicContainerVm = (GraphicContainerVm)Tag;

                SetAxis();

                SetBindings();

                //SciChart.AnnotationUnderlaySurface.Background = ((ScattergramVm)GraphicVm).IsWhiteBackground ? Brushes.White : Brushes.Black;

                SciChart.AdornerLayerCanvas.Children.Add(RegionPanel);

                if (!_graphicContainerVm.GraphicDictionary.ContainsKey(Id))
                {
                    _graphicContainerVm.GraphicDictionary.Add(Id, new Tuple<GraphicUcBase, GraphicVmBase>(this, GraphicVm));
                    GraphicVm.UpdateEvents();
                }
            }
            GraphicVm.UpdateEvents();
            GraphicVm.SetSize((int)SciChart.AnnotationUnderlaySurface.ActualWidth, (int)SciChart.AnnotationUnderlaySurface.ActualHeight);
            IsLoading = false;
        }


        public override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnSizeChanged(sender, e);
            if (((ScattergramVm)GraphicVm).IsMapChecked)
            {
                //if (Math.Abs(e.NewSize.Width - 0) <= double.Epsilon && Math.Abs(e.NewSize.Height - 0) <= double.Epsilon)
                //{
                //    return;
                //}
                GraphicVm.UpdateEvents();
            }
            e.Handled = true;
        }

        #endregion

    }
}
