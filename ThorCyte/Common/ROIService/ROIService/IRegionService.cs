using ComponentDataService.Types;
using ROIService.Region;
using System.Collections.Generic;

namespace ROIService
{
    public interface IRegionService
    {
        IList<BioEvent> GetEvents(string id);
        IList<BioEvent> GetEvents(string lhsId, string rhsId, OperationType type);
        void ChangeActiveWells(IList<int> activeWells);
        int GetRegionId();
        bool AddRegion(MaskRegion region);
        void SetRegion(MaskRegion region);
        bool UpdateRegions(ICollection<MaskRegion> regions);
        bool RemoveRegions(IEnumerable<string> ids);
        IEnumerable<string> GetRegionIdList();
        IEnumerable<string> GetRegionIdList(string componentName);
        MaskRegion GetRegion(string id);
        void InitRegions(IEnumerable<MaskRegion> regions);
        void Clear();
    }
}
