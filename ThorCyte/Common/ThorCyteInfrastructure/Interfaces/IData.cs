using System.Windows;
using ImageProcess;

namespace ThorCyte.Infrastructure.Interfaces
{
    public interface IData
    {
        void SetExperimentInfo(IExperiment experiment);

        //For Mode3D, Mode3DTiming model
        ImageData GetData(int scanId, int scanRegionId, int channelId, int planeId, int timingFrameId);

        ImageData GetData(int scanId, int scanRegionId, int channelId, int planeId, int timingFrameId,
            double scale, Int32Rect regionRect);

        //For all types 3D stream scan and other type 3D scan to get tile detail
        ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int planeId, int tileId, int timingFrameId);

        //For Mode2D, Mode2DTiming model
        ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId);

        // For Moe2D, Scale
        ImageData GetData(int scanId, int scanRegionId, int channelId, int timingFrameId,
            double scale, Int32Rect regionRect);

        //For all types 2D stream scan and other type 2D scan to get tile detail
        ImageData GetTileData(int scanId, int scanRegionId, int channelId, int streamFrameId, int tileId, int timingFrameId);

        //1D Data
        ImageData GetData(int scanId, int scanRegionId, int channelId);

    }
}
