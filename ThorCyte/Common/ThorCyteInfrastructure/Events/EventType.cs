
namespace ThorCyte.Infrastructure.Events
{
    public class RegionTile
    {
        public int RegionId;
        public int TileId;
    }

    public class LoadExInfo
    {
        public int ScanId;
        public bool WithAnalysisResult;
    }


    public class FrameIndex
    {
        public int StreamId { set; get; }
        public int TimeId { set; get; }
        public int ThirdStepId { set; get; }
    }

    public class MacroStartEventArgs
    {
        public int WellId;
        public int RegionId;
        public int TileId;
        
        public MacroStartEventArgs()
        {
            WellId = 0;
            RegionId = 0;
            TileId = 0;
        }
    }
}
