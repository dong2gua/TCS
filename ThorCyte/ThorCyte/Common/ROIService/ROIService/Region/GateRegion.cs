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

        public override bool Contains(System.Windows.Point pt)
        {
            return (pt.X >= MinValue && pt.X <= MaxValue);
        }

      
    }
}
