using Prism.Events;
using System.Collections.Generic;

namespace ThorCyte.Infrastructure.Events
{
    public class RemoveRegionsEvent : PubSubEvent<IEnumerable<string>>
    {
    }
}
