using System;
using System.Collections.Generic;
using ROIService.Region;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Events
{
    public class GraphUpdateArgs
    {
        public string Id { get; set; }

        public GraphUpdateArgs(string id)
        {
            Id = id;
        }
    }

    public class RegionUpdateArgs
    {
        public string GraphicId { get; set; }

        public IEnumerable<MaskRegion> RegionList { get; set; }

        public RegionUpdateType UpdateType { get; set; }

        public RegionUpdateArgs(string graphicId, IEnumerable<MaskRegion> regionList, RegionUpdateType type)
        {
            GraphicId = graphicId;
            RegionList = regionList;
            UpdateType = type;
        }
    }
}
