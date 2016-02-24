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

        #endregion

        #region Constructor

        public ScattergramView()
            : base(8, new Scattergram { Background = Brushes.Transparent })
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
                        Width =1,
                        Height = 1,
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
                        Width = 1,
                        Height = 1,
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
                DrawMajorBands = false,
                TextFormatting = "#.#E+0",
                ScientificNotation = ScientificNotation.LogarithmicBase,
                VisibleRange = new DoubleRange(0,100)
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
        }

        public override void SetBindings()
        {
            base.SetBindings();

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
            if (!GraphicVm.XAxis.IsLogScale)
            {
                SciChart.XAxis = XAxis;
                SciChart.XAxis.TextFormatting = GraphicVm.XAxis.MaxValue >= Math.Pow(10, 6) ? ConstantHelper.AxisMaxTextFormat : string.Empty;
            }
            else
            {
                SciChart.XAxis = XLogAxis;
            }
            if (!GraphicVm.YAxis.IsLogScale)
            {
                SciChart.YAxis = YAxis;
                SciChart.YAxis.TextFormatting = GraphicVm.YAxis.MaxValue >= Math.Pow(10, 6) ? ConstantHelper.AxisMaxTextFormat : string.Empty;
            }
            else
            {
                SciChart.YAxis = _yLogAxis;
            }
            SciChart.XAxis.VisibleRange = new DoubleRange(GraphicVm.XAxis.MinValue, GraphicVm.XAxis.MaxValue);
            SciChart.YAxis.VisibleRange = new DoubleRange(GraphicVm.YAxis.MinValue, GraphicVm.YAxis.MaxValue);
        }

        private void DrawPlot()
        {
            var pointList = new List<Point>();
            for (var index = 0; index < SciChart.RenderableSeries.Count; index++)
            {
                SciChart.RenderableSeries[index].DataSeries = null;
            }
            if (((ScattergramVm)GraphicVm).DotTupleList.Count == 0)
            {
                ((Scattergram)RegionPanel).Update(pointList);
                return;
            }

            foreach (var tuple in ((ScattergramVm)GraphicVm).DotTupleList)
            {
                _dotDictionary[tuple.Item2].Add(tuple.Item1);
            }

            var baseIndex = ((ScattergramVm)GraphicVm).IsMapChecked ? 7 : 0; //3d plot color begin with 8
            var xscale = GraphicVm.Width / (GraphicVm.XAxis.MaxRange - GraphicVm.XAxis.MinRange);
            var yscale = GraphicVm.Height / (GraphicVm.YAxis.MaxRange - GraphicVm.YAxis.MinRange);



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
                    pointList.AddRange(item.Value);
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
                    var xList = item.Value.Select(pt => GraphicVm.XAxis.IsLogScale ? GraphicVm.XAxis.GetActualValue((int)pt.X) : pt.X / xscale + GraphicVm.XAxis.MinRange).ToList();
                    var yList = item.Value.Select(pt => GraphicVm.YAxis.IsLogScale ? GraphicVm.YAxis.GetActualValue((int)pt.Y) : pt.Y / yscale + GraphicVm.YAxis.MinRange).ToList();
                    for(var i = 0; i < xList.Count; i++)
                    {
                        pointList.Add(new Point(xList[i], yList[i]));
                    }
                    DataSeriesArray[index].Append(xList, yList);
                    SciChart.RenderableSeries[baseIndex + index].DataSeries = DataSeriesArray[index];
                }
            }
            ((Scattergram)RegionPanel).Update(pointList);
        }


        public override void OnLoad(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                GraphicVm = (GraphicVmBase)DataContext;
                Id = GraphicVm.Id;

                if (!double.IsNaN(((ScattergramVm)GraphicVm).QuadrantCenterPoint.X) &&
                    !double.IsNaN((((ScattergramVm)GraphicVm).QuadrantCenterPoint.Y)))
                {
                    ((Scattergram)RegionPanel).QuadrantCenterPoint = ((ScattergramVm)GraphicVm).QuadrantCenterPoint;
                }

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
            if (GraphicVm == null)
            {
                return;
            }
            if (((ScattergramVm)GraphicVm).IsMapChecked)
            {
                GraphicVm.UpdateEvents();
            }
            e.Handled = true;
        }

        #endregion

    }
}
