using System.Windows;
using System.Windows.Media;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.Carrier
{
    public class CarrierRoom
    {
        #region Fileds

        public readonly RegionShape Shape;
        public readonly int No;
        public readonly Rect Rect;
        public readonly Rect ScannableRect;
        public readonly Geometry ScannableRegion;
        public readonly RegionShape ScannableShape;

        #endregion Fileds

        #region Constructors
        public CarrierRoom(int no, RegionShape shape, Rect rect, RegionShape scanShape, Rect scanRect)
        {
            No = no;
            Shape = shape;
            Rect = rect;

            ScannableRect = scanRect;
            ScannableShape = scanShape;

            if (scanShape == RegionShape.Ellipse)
            {
                ScannableRegion = new EllipseGeometry(scanRect);
            }
            else
                ScannableRegion = new RectangleGeometry(scanRect);
        }

        #endregion Constructors
    }
}
