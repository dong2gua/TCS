using System.Windows;
using ThorCyte.Infrastructure.Types;

namespace ROIService.Region
{
    public class GateRegion:MaskRegion
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public GateRegion(int id, double minVal, double maxVal) : base(id)
        {
            MaxValue = maxVal;
            MinValue = minVal;
            Shape = RegionShape.Gate;
        }

        public override bool Contains(Point pt)
        {
            Point inner = ToInnerPoint(pt);
            return (inner.X >= MinValue && inner.X <= MaxValue);
        }

      
    }
}
