using System;
using System.Windows;
using ThorCyte.Infrastructure.Types;

namespace ROIService.Region
{
    public class EllipseRegion : MaskRegion
    {
        public Size Axis { get; set; }
        public Point Center { get; set; }


        public EllipseRegion(int id, Size axis, Point center) : base(id)
        {
            Axis = axis;
            Center = center;
            Shape = RegionShape.Ellipse;
        }

        public override bool Contains(Point pt)
        {           
            var innerPoint = ToInnerPoint(pt);
            var rx = Axis.Width / 2;
            var ry = Axis.Height / 2;
            var x = innerPoint.X;
            var y = innerPoint.Y;
            var value = Math.Pow((x - Center.X)/rx, 2) + Math.Pow((y - Center.Y)/ry, 2);
            return value <= 1.0;
        }


    }
}
