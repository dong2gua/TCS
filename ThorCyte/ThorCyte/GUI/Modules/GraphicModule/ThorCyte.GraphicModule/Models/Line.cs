namespace ThorCyte.GraphicModule.Models
{

    /// <summary>
    /// Directional line, described by 
    /// a point that is on the the line, and 
    /// a vector that is parallelled to the line
    /// </summary>
    public class Line
    {
        public enum CalipersOrder { Identical, Right, Left };

        PointD m_point;
        Vector m_vector;

        public Line(PointD point, Vector vector)
        {
            m_point = point;
            m_vector = vector;
        }

        // overloaded constructor, determined by 2 points (2 vertices)
        public Line(PointD point1, PointD point2)
        {
            m_point = point1;
            m_vector = new Vector(point1, point2);
        }

        // overloaded constructor, passing through a point and parallel to a line
        public Line(PointD point1, Line line)
        {
            m_point = point1;
            m_vector = line.Vector;
        }

        public Line Clone()
        {
            Line line = new Line(this.Point, this.Vector);
            return line;
        }

        public Vector Vector { get { return m_vector; } }
        public PointD Point { get { return m_point; } }

        // This function is for checking the ordering of two calipers (l1 and l2, they are always in same direction
        ////		// If l2 is right to l1 return 1, l2 = l1 return 0, l2 is left to l1 return -1
        public static CalipersOrder GetCalipersOrder(Line l1, Line l2)
        {
            if (!IsSameDirection(l1, l2))
            {
                //throw new CyteException("ConvexPolygon.GetCalipersOrder", "Not parallel");
            }

            if (IsEqual(l1, l2))
                return CalipersOrder.Identical; // 0;
            else
            {
                if (IsPointOnRight(l2.Point, l1))
                    return CalipersOrder.Right; // 1;
                else
                    return CalipersOrder.Left; // -1;
            }
        }

        private static bool IsSameDirection(Line l1, Line l2)
        {
            Vector v1 = l1.Vector;
            Vector v2 = l2.Vector;

            return Vector.IsSameDirection(v1, v2);
        }

        public static double Angle(Line l1, Line l2)
        {
            Vector v1 = l1.Vector;
            Vector v2 = l2.Vector;

            return Vector.Angle(v1, v2);
        }

        public static bool IsPointOnLine(PointD point, Line line)
        {
            if (PointD.IsEqual(line.Point, point))
                return true;

            Vector v = new Vector(line.Point, point);

            return Vector.IsParallel(line.Vector, v);
        }

        public static bool IsPointOnRight(PointD point, Line line)
        {
            Vector v = new Vector(line.Point, point);

            return Vector.CrossProductZ(line.Vector, v) < 0.0;
        }

        public static bool IsEqual(Line l1, Line l2)
        {
            PointD p1 = l1.Point;

            return IsPointOnLine(p1, l2) && IsSameDirection(l1, l2);
        }

        public static double Distance(Line l1, Line l2) // jcl-7386
        {
            return 0d;
        }
    }
}
