using ComponentDataService.Types;
using ImageProcess;
using ImageProcess.DataType;
using System.Collections.Generic;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ComponentDataService
{
  
    public interface IComponentDataService
    {
        void Load(IExperiment experiment);
        IList<string> GetComponentNames();
        IList<Blob> GetBlobs(string componentName, int wellId, int tileId, BlobType type);
        IList<BioEvent> GetEvents(string componentName, int wellId);
        IList<Feature> GetFeatures(string componentName);
        void AddComponent(string componentName, IList<Feature> features);
        void ClearComponents();
        int GetFeatureIndex(string componentName, FeatureType type, string channelName = null);
        void Save(string fileFolder);
        IList<Blob> CreateContourBlobs(string componentName, int scanId, int wellId, int tileId,
            ImageData data, double minArea, double maxArea);

        IList<BioEvent> CreateEvents(string componentName, int scanId, int wellId, int tileId, 
            IDictionary<string, ImageData> imageDict, BlobDefine define);
        int GetComponentScanId(string componentName);
        void Association(string masterComponentName, string slaveComponentName);
        void Association(string masterComponentName, string firstSlaveComponentName, string secondSlaveComponentName);
        IList<Channel> GetChannels(string componentName);
    }
}
