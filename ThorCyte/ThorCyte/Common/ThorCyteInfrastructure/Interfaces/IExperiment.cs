
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using ThorCyte.Infrastructure.Types;
namespace ThorCyte.Infrastructure.Interfaces
{
    public struct ExperimentInfo
    {
        public string Name { set; get; }
        public string Date { set; get; }
        public string UserName { set; get; }
        public string ComputerName { set; get; }
        public string InstrumentType { set; get; }
        public string SoftwareVersion { set; get; }
        public string Notes { set; get; }
        public int IntensityBits { set; get; }
        public string AnalysisPath { set; get; }
    }

    public class ScanInfo
    {
        public int ScanId { set; get; }
        public string ObjectiveType { set; get; }
        public CaptureMode Mode { set; get; }
        public ScanPathType ScanPathMode { set; get; }
        public string DataPath { set; get; }
        public ImageFileFormat ImageFormat { set; get; }

        public IList<Channel> ChannelList { private set; get; }
        public IList<VirtualChannel> VirtualChannelList { private set; get; }
        public IList<ComputeColor>  ComputeColorList { private set; get; }

        public double XPixcelSize { set; get; }
        public double YPixcelSize { set; get; }
        public double ZPixcelSize { set; get; }

        public ResUnit ResolutionUnit;

        public int TileWidth { set; get; }
        public int TiledHeight { set; get; }

        public int StreamFrameCount { set; get; }    // For ThorImage: stream scan; For OCT time sequence or Doppler scan
        public int FlybackFrameCount { set; get; }       // For ThorImage: stram fast Z scan;
        public int ThirdDimensionSteps { set; get; }   // For ThorImage: Z Steps; For ThorOCT Y Steps;

        public int TimingFrameCount { set; get; }
        public double TimeInterval { set; get; }

        public IList<ScanRegion> ScanRegionList {private set; get; }
        public IList<Well> ScanWellList { private set; get; } 

        public IList<int> TimePointList { private set; get;}

        public ScanInfo()
        {
            ChannelList = new List<Channel>();
            VirtualChannelList = new List<VirtualChannel>();
            ScanRegionList = new List<ScanRegion>();
            TimePointList = new List<int>();
            ComputeColorList = new List<ComputeColor>();
            ScanWellList = new List<Well>();
        }
    }

    public interface IExperiment
    {
        //Initalize
        void Load(string experimentPath);

        // Experiment
        ExperimentInfo GetExperimentInfo();

        int GetScanCount();

        ScanInfo GetScanInfo(int scanId);

        // Carrier
        string GetCarrierType();

        // only for OCT
        BitmapSource GetCameraView();

        
    }
}
