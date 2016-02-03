using System.Collections.Generic;
using Prism.Events;

namespace ThorCyte.CarrierModule.Events
{
    
    /// <summary>
    /// internal use for notify tile view
    /// </summary>
    class RegionsSelected : PubSubEvent<List<int>>
    {
    }
}
