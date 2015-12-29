using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using ThorCyte.ImageViewerModule.Viewmodel;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.ImageViewerModule.DrawTools;
using ThorCyte.Infrastructure.Interfaces;
using ImageProcess;
namespace ThorCyte.ImageViewerModule.View
{
    /// <summary>
    /// Interaction logic for ViewportView.xaml
    /// </summary>
    public partial class ViewportView : UserControl
    {
        public ViewportView()
        {
            InitializeComponent();
            var vm = new ViewportViewModel();
            this.DataContext = vm;
            drawCanvas.MousePoint += new DrawingCanvas.MousePointHandler(vm.OnMousePoint);
            drawCanvas.MoveVisualRect += new DrawingCanvas.MoveVisualRectHandler(vm.OnMoveVisualRect);
            drawCanvas.CanvasSizeChanged += new DrawingCanvas.CanvasSizeChangedHandler(vm.OnCanvasSizeChanged);
            //ThorImageExperiment experiment = new ThorImageExperiment();
            //experiment.Load("D:\\ThorCyte\\Data\\2D_1024_100Tiles\\Experiment.xml");
            //var scanInfo = experiment.GetScanInfo(1);
            //var _channels = scanInfo.ChannelList;
            //var _virtualChannels = scanInfo.VirtualChannelList;
            //var _computeColors = scanInfo.ComputeColorList;
            //var imageData = new ThorImageData();
            //imageData.SetExperimentInfo(experiment);
            //var data = imageData.GetData(1, 0, 0, 1);
            //drawCanvas.DisplayImage = data.ToBitmapSource();
        }
    }
}
