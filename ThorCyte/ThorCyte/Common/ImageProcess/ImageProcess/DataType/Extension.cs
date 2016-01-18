
using System.Windows;

namespace ImageProcess.DataType
{
    internal static class Extension
    {
        internal static bool Contains(this Int32Rect rect, Point point)
        {
            var x = (int) point.X;
            var y = (int) point.Y;
            return (x >= rect.X) && (x < rect.X + rect.Width) && (y >= rect.Y) && (y < rect.Y + rect.Height);
        }
    }


}
