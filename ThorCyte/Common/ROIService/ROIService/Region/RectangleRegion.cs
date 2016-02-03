using System.Windows;
using ThorCyte.Infrastructure.Types;

namespace ROIService.Region
{
    public class RectangleRegion : MaskRegion
    {
        public Size Size { get; set; }
        public Point LeftUp { get; set; }



        public RectangleRegion(int id, Size size, Point leftUp) : base(id)
        {
            Size = size;
            LeftUp = leftUp;
            Shape = RegionShape.Rectangle;
        }

        public override bool Contains(Point pt)
        {
            var inner = ToInnerPoint(pt);
            var x = inner.X;
            var y = inner.Y;
            var xInRange = (x >= LeftUp.X) && (x <= LeftUp.X + Size.Width);
            var yInRange = (y <= LeftUp.Y) && (y >= LeftUp.Y - Size.Height);
            return xInRange && yInRange;
        }

       
    }
}
