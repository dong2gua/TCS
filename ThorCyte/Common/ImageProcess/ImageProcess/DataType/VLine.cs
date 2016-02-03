using System.Windows;

namespace ImageProcess.DataType
{
    public struct VLine
    {
        public static readonly VLine Empty = new VLine(0, 0, 0);
        public VLine(ushort x, ushort y1, ushort y2) { X = x; Y1 = y1; Y2 = y2; }
        public VLine(int x, int y1, int y2) { X = x; Y1 = y1; Y2 = y2; }

        public VLine(double x, double y1, double y2)
        {
            X = (int) x;
            Y1 = (int) y1;
            Y2 = (int) y2;
        }

        public int X;
        public int Y1;
        public int Y2;
        public int Length { get { return Y2 - Y1; } }

        public void Offset(Point pos)
        {
            X += (int)pos.X;
            Y1 +=(int) pos.Y;
            Y2 += (int) pos.Y;
        }
    }
}