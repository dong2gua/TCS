using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using ThorCyte.Infrastructure.Types;

namespace ROIService.Region
{
    public class PolygonRegion : MaskRegion
    {
        public IList<Point> Vertex { get;  set; }

        public PolygonRegion(int id, IEnumerable<Point> vertex) : base(id)
        {
            Vertex = new List<Point>(vertex);
            Shape = RegionShape.Polygon;
        }

        public override bool Contains(Point pt)
        {
            var n = Vertex.Count;
            if (Vertex.Count <= 2)
                return false;
            var isInside = false;
            var inner = ToInnerPoint(pt);
            var x = inner.X;
            var y = inner.Y;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((Vertex[i].Y > y) != (Vertex[j].Y > y))
                    &&
                    (x <
                     (Vertex[j].X - Vertex[i].X) * (y - Vertex[i].Y) /
                     (Vertex[j].Y - Vertex[i].Y) + Vertex[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;

        }

        public override string ToString()
        {
            var sb = new StringBuilder(500);
            foreach (var point in Vertex)
            {
                sb.Append(point.ToString(CultureInfo.CurrentCulture));
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

      

  
    }
}
