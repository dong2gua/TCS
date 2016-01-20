using System;

namespace ThorCyte.GraphicModule.Models
{
    /// <summary>
    /// Vector class
    /// </summary>
    public class Vector
    {
        const double snErr = 1.0e-12; // jcl-bdry, computing error

        public double X;
        public double Y;

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        // from p1 to p2
        public Vector(PointD p1, PointD p2)
        {
            X = p2.X - p1.X;
            Y = p2.Y - p1.Y;
        }

        public double Magnitude
        {
            get { return Math.Sqrt(X * X + Y * Y); }
        }

        public Vector UnitVector
        {
            get
            {
                double magnitude = this.Magnitude;

                double x = this.X / magnitude;
                double y = this.Y / magnitude;

                return new Vector(x, y);
            }
        }

        public bool IsVertical()
        {
            return this.X == 0.0;
        }

        // returns the angle that is from v1 to v2, unit = degree, range = [0, 360)
        // clockwise
        public static double Angle(Vector v1, Vector v2)
        {
            double cos = InnerProduct(v1, v2) / (v1.Magnitude * v2.Magnitude);
            if (cos > 1.0)
                cos = 1.0;
            if (cos < -1.0)
                cos = -1.0;
            double radian = Math.Acos(cos); // principal value is [0, pi]

            // using cross product to determine clockwise or counter clockwise
            if (CrossProductZ(v1, v2) > 0.0)
                radian = 2.0 * Math.PI - radian;

            return radian * 180.0 / Math.PI;
        }

        // Inner product of 2 vectors, v1 . v2
        public static double InnerProduct(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        // Z value of cross product of 2 vectors, v1 X v2
        // In our case, x and y component in the cross product are 0
        // used for determine how two vectors are related  
        public static double CrossProductZ(Vector v1, Vector v2)
        {
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        public static bool IsSameDirection(Vector v1, Vector v2)
        {
            if ((v1.X == 0.0 && v1.Y == 0.0) || (v2.X == 0.0 && v2.Y == 0.0)) // jcl-edge7
                return true; // ?

            Vector unitV1 = v1.UnitVector;
            Vector unitV2 = v2.UnitVector;

            double cos = InnerProduct(unitV1, unitV2);

            return Math.Abs(cos - 1.0) < snErr;
        }

        public static bool IsParallel(Vector v1, Vector v2)
        {
            if ((v1.X == 0.0 && v1.Y == 0.0) || (v2.X == 0.0 && v2.Y == 0.0)) // jcl-edge7
                return true; // ?

            Vector unitV1 = v1.UnitVector;
            Vector unitV2 = v2.UnitVector;

            double cos = InnerProduct(unitV1, unitV2);

            return Math.Abs(cos - 1.0) < snErr || Math.Abs(cos + 1.0) < snErr;
        }

        public static bool IsPointOnSegment(PointD point, PointD sPoint1, PointD sPoint2)
        {
            if (PointD.IsEqual(point, sPoint1) || PointD.IsEqual(point, sPoint2)) // jcl-edge7
                return true;

            Vector v1 = new Vector(sPoint1, point);
            Vector v2 = new Vector(sPoint1, sPoint2);

            if (IsSameDirection(v1, v2))
            {
                if (v1.Magnitude <= v2.Magnitude)
                    return true;
                else
                    return false;
            }

            return false;
        }

        // insection of segments p1-p2 and line p3->p4
        public static PointD IntersectionPoint(PointD p1, PointD p2, PointD p3, PointD p4)
        {
            Vector v1 = new Vector(p1, p2);
            Vector v2 = new Vector(p3, p4);

            if (IsParallel(v1, v2)) // this includes the cases either no intersection or infinity intersections(?)
                return null;

            // find the intersection of 2 lines (determined by p1, p2 and p3, p4
            // l1 = p1 + t * (p2 - p1)
            // l2 = p3 + s * (p4 - p3)
            Vector v31 = new Vector(p3, p1);

            double ds = v2.X * v1.Y - v2.Y * v1.X;
            double ns = v31.X * v1.Y - v31.Y * v1.X;

            if (ds == 0.0) // happens if v1, v2 parallel || v1 = 0 || v2 = 0, should not happen
                return null;

            double s = ns / ds;
            double x = p3.X + s * v2.X;
            double y = p3.Y + s * v2.Y;

            PointD point = new PointD(x, y); // intersection point of two lines but not necessary of two segments
            if (IsPointOnSegment(point, p1, p2) && IsPointOnSegment(point, p3, p4))
                return point;

            return null;
        }

        // called in ConvexPolygon to determine if 2 polygons are seperated
        // segment1 p0 -> p1, and segment2 q0 -> q1
        public static bool IsTwoSegementsIntersect(PointD p0, PointD p1, PointD q0, PointD q1)
        {
            // segement1 (or line1) equation: p(s) = p0 + s * (p1 - p0) = p0 + s * u
            // u = p1 - p0, p(s) is on segment1 if s is within range [0, 1] 
            // segement2 (or line2) equation: q(t) = q0 + t * (q1 - q0) = q0 + t * v
            // v = q1 - q0, q(t) is on segment2 if t is within range [0, 1]

            // Find intersection point of line1 and line2 if they are not parallel
            // and corresponding s and t, if they are both within range [0, 1]
            // then 2 segments intersect

            Vector u = new Vector(p0, p1);
            Vector v = new Vector(q0, q1);

            if (IsParallel(u, v))
            {
                if (IsPointOnSegment(p0, q0, q1) || IsPointOnSegment(p1, q0, q1)) // two attached
                    return true;
                else
                    return false;
            }
            else // not parallel
            {
                // At intersection, vector p(s) - q0 = w + s * u, where w = p0 - q0, is perrpendicular to vp,
                // vp is a normal vector of line 2, vp = (-v.Y, v.X). Solve vp * (w + s * u) = 0 for s,
                // we get s = (v.Y * w.X - v.X * w.Y) / (v.X * u.Y - v.Y * u.X)
                // similarly, t = (u.X * w.Y - u.Y * w.X) / (u.X * v.Y - u.Y * v.X)
                Vector w = new Vector(q0, p0);
                double s = (v.Y * w.X - v.X * w.Y) / (v.X * u.Y - v.Y * u.X);
                double t = (u.X * w.Y - u.Y * w.X) / (u.X * v.Y - u.Y * v.X);

                if (s >= 0.0 && s <= 1.0 && t >= 0.0 && t <= 1.0)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsEqual(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static double Slope(PointD p1, PointD p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0f)
                return double.MaxValue;
            else
                return dy / dx;
        }

    }
}
