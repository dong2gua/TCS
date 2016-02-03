namespace ThorCyte.GraphicModule.Utils
{
    public enum ToolType { Pointer, Rectangle,Ellipse, Polygon,Line,Cross }

    public enum LineType{ Horizon,Vertical,Slash }

    public enum GateType { Normal, ParentToChild, Inclusive, Exclusive, None }

    public enum RegionColor { Default, Red, Green, Blue, Yellow, Magenta, Cyan }

    public enum RegionType { None, Rectangle, Ellipse, Polygon,PolyLine, Line}

    public enum AxesEnum { XAxis, YAxis,ZAxis }

    public enum RegionUpdateType { Update,Delete,Add,Color }

    public enum GraphStyle { DotPlot, DensityMap, ValueMap, BarChart, Outline }

    public enum ColorType { None, Normal, Customer }
}
