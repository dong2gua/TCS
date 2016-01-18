
namespace ThorCyte.Infrastructure.Types
{
    public enum CaptureMode { Mode1D, Mode2D, Mode3D, Mode2DTimingStream, Mode2DTiming, Mode2DStream, Mode3DTimingStream, Mode3DTiming, Mode3DStream, Mode3DFastZStream};
    public enum RegionShape { None, Point, Line, Rectangle, Ellipse, Polygon, Gate }
    public enum ResUnit { None, Inch, Centimeter, Millimetre, Micron, Nanometer, Picometer }
    public enum ScanPathType { None, Serpentine, LeftToRight, RightToLeft }
    public enum ImageOperator { Add, Subtract, Invert, Min, Max, Multiply, And, Or, Xor, LinearTx, ShiftPeak, MultAdd, Matrix }
    public enum ImageFileFormat { None, Jpeg, Flat, Tiff, Raw }
    public enum RegionColorIndex { Red, Green, Blue, Yellow, Magenta, Cyan, Black, White }
}
