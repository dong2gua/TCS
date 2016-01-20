using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Converts;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Controls
{
    public abstract class GraphicUcBase : UserControl
    {
        #region Fields

        private string _id;

        #endregion

        #region Properties

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RegionPanel.Id = value;
            }
        }

        public bool IsLoading { get; set; }

        public GraphicVmBase GraphicVm { get; set; }

        public RegionCanvas RegionPanel { get; set; }

        public NumericAxis XAxis { get; set; }

        public NumericAxis YAxis { get; set; }

        public LogarithmicNumericAxis XLogAxis { get; set; }

        public int RenderableSeriesCount { get; set; }

        public XyDataSeries<double, double>[] DataSeriesArray { get; set; }

        #endregion

        #region Constrcutor

        protected GraphicUcBase(int renderableSeriesCount, RegionCanvas canvas)
        {
            IsLoading = true;
            RegionPanel = canvas;
            RenderableSeriesCount = renderableSeriesCount;
            DataSeriesArray = new XyDataSeries<double, double>[RenderableSeriesCount];
            Loaded += OnLoad;
            RegionPanel.PreviewMouseLeftButtonUp += OnCanvasLeftUp;
            RegionPanel.SizeChanged += OnSizeChanged;
        }

        #endregion

        #region Methods

        public abstract void InitRenderableSeries();

        public abstract void SetAxis();

        public abstract void OnLoad(object sender, RoutedEventArgs e);

        public virtual void Init()
        {
            for (var index = 0; index < RenderableSeriesCount; index++)
            {
                DataSeriesArray[index] = new XyDataSeries<double, double> {AcceptsUnsortedData = true};
            }
            InitRenderableSeries();
            InitAxis();
        }

        public virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            GraphicVm.SetSize((int)RegionPanel.ActualWidth, (int)RegionPanel.ActualHeight);
        }

        public virtual void OnCanvasLeftUp(object sender, MouseButtonEventArgs e)
        {
            var canvas = (RegionCanvas)sender;
            var vm = canvas.DataContext as GraphicVmBase;
            if (vm != null)
            {
                var enabled = false;
                foreach (var graphic in canvas.Selection)
                {
                    if (graphic is GraphicsLine)
                    {
                        continue;
                    }
                    if (graphic.IsSelected)
                    {
                        enabled = true;
                        break;
                    }
                }
                vm.IsColorListEnabled = enabled;
                if (!enabled)
                {
                    vm.SelectedRegionColor = GraphicVmBase.ColorRegionList[0];
                }
            }
        }

        public virtual void InitAxis()
        {
            XAxis = new NumericAxis
            {
                GrowBy = new DoubleRange(0.1, 0.1),
                AutoRange = AutoRange.Never,
                DrawMinorTicks = true,
                DrawMinorGridLines = false,
                DrawMajorGridLines = false,
                AxisAlignment = AxisAlignment.Bottom,
            };
            YAxis = new NumericAxis
            {
                GrowBy = new DoubleRange(0.1, 0.1),
                AutoRange = AutoRange.Never,
                DrawMinorTicks = true,
                DrawMinorGridLines = false,
                DrawMajorGridLines = false,
                AxisAlignment = AxisAlignment.Left,
            };
            XLogAxis = new LogarithmicNumericAxis
            {
                AxisAlignment = AxisAlignment.Bottom,
                AutoRange = AutoRange.Never,
                GrowBy = new DoubleRange(0.1, 0.1),
                DrawMinorTicks = true,
                DrawMinorGridLines = false,
                DrawMajorGridLines = false,
                DrawMajorBands = false
            };
        }

        public virtual void SetBindings()
        {
            RegionPanel.SetBinding(RegionCanvas.ToolProperty,
                new Binding { Path = new PropertyPath("(0)", typeof(GraphicVmBase).GetProperty("RegionToolType")), Mode = BindingMode.TwoWay, NotifyOnSourceUpdated = true });
            RegionPanel.SetBinding(RegionCanvas.RegionColorProperty,
                new Binding("SelectedRegionColor") { Source = GraphicVm, Mode = BindingMode.TwoWay, Converter = new RegionColorConvert() });
            var xaxisBinding = new Binding("Title") { Source = GraphicVm.XAxis };
            XAxis.SetBinding(AxisBase.AxisTitleProperty, xaxisBinding);
            XLogAxis.SetBinding(AxisBase.AxisTitleProperty, xaxisBinding);
        }

        #endregion
    }
}
