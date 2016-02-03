
using System.Collections.Generic;
using System.Windows.Media;

namespace ThorCyte.Infrastructure.Types
{
    public class ComputeColor
    {
        public Dictionary<Channel, Color> ComputeColorDictionary;
        public string Name { get; set; }
        public int Brightness { set; get; }
        public double Contrast { set; get; }

        public ComputeColor()
        {
            ComputeColorDictionary = new Dictionary<Channel, Color>();
            Brightness = 0;
            Contrast = 1.0;
        }
    }
}
