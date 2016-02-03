using System;
using System.Windows;
using ImageProcess;

namespace ThorCyte.Infrastructure.Interfaces
{
    public class ThorOCTData : IData
    {
        public void SetExperimentInfo(IExperiment experiment)
        {
            throw new NotImplementedException();
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int planeId, int timingFrameId)
        {
            throw new NotImplementedException();
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int planeId, int timingFrameId, double scale,
            Int32Rect regionRect)
        {
            throw new NotImplementedException();
        }

        public ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int planeId, int tileId,
            int timingFrameId)
        {
            throw new NotImplementedException();
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId)
        {
            throw new NotImplementedException();
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId, double scale, Int32Rect regionRect)
        {
            throw new NotImplementedException();
        }

        public ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int tileId, int timingFrameId)
        {
            throw new NotImplementedException();
        }

        public ImageData GetData(int scanId, int scanRegionId, int channelId)
        {
            throw new NotImplementedException();
        }
    }
}
