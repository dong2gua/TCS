
using System.Collections.Generic;
using Prism.Events;

namespace ThorCyte.Infrastructure.Events
{
    public class SelectRegions : PubSubEvent<List<int>>
    {
    }
}
