using System.Collections.Generic;
using Prism.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule
{
    //Input
    public class LoadCarrierEventArgs
    {
        public IExperiment Exp { get; set; }
        public int ScanId { get; set; }
    }

    public class LoadCarrierEvent : PubSubEvent<LoadCarrierEventArgs>
    {
    }

    //Output
    public class ActiveRegionChangedEventArgs
    {
        public IList<ScanRegion> RegionList;
    }

    public class ActiveRegionChanged : PubSubEvent<ActiveRegionChangedEventArgs>
    {
    }

    //Output
    public class ActiveFieldChangedEventArgs
    {
        public Scanfield Field;
    }
    public class ActiveFieldChanged : PubSubEvent<ActiveFieldChangedEventArgs>
    {
    }

}
