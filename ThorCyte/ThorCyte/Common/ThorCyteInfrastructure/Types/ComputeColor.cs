
using System.Collections.Generic;
using System.Windows.Media;

namespace ThorCyte.Infrastructure.Types
{
    public class ComputeColor
    {
        public Dictionary<Channel, Color> ComputeColorDictionary;
        public string Name { get; set; }

        public ComputeColor()
        {
            ComputeColorDictionary = new Dictionary<Channel, Color>();
        }
    }
}
