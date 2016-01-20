using System.Windows;

namespace ThorCyte.GraphicModule.Models
{
    /// <summary>
    /// Point with double type coordinates x and y
    /// </summary>
    public class PointD
    {
        public double X;
        public double Y;

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public PointD(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
        }

        public static bool IsEqual(PointD p1, PointD p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        // pt is a ref point, after move, value is relative to pt
        public void Move(PointD pt)
        {
            X -= pt.X;
            Y -= pt.Y;
        }

        public void Scale(double sx, double sy)
        {
            X *= sx;
            Y *= sy;
        }

        public PointD Clone()
        {
            return new PointD(X, Y);
        }

    }
}
