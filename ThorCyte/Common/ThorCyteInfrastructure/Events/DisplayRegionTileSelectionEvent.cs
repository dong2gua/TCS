using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace ThorCyte.Infrastructure.Events
{
    public class DisplayRegionTileSelectionEvent : PubSubEvent<RegionTile>
    {
    }
}
