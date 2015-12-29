namespace ThorCyte.ProtocolModule.Models
{
    /// <summary>
    /// Summary description for Port.
    /// </summary>
    public enum PortDataType { None, BinaryImage, GrayImage, MultiChannelImage, Image, Event, Setting, Flag }

    public enum PortType { None = 0, InPort, OutPort }

    public enum OpenVirMode { None, New, Edit }

    public enum ScanModType { FieldScan = 1, MosaicScan = 2 }

    /// <summary>
    /// Summary description for ThresholdMod.
    /// </summary>
    public enum ThresholdMethod { Manual, Statistical, Otsu }

    /// <summary>
    /// Helper class to setup a static variable for MainForm
    /// Usage:
    /// 	Form mainWindow = (Form)CompuCyte.Framework.Framework.MainWindow; 
    /// </summary>
    public enum UnitType { Pixel, Micron, Mm, Inch }

    /// <summary>
    /// Summary description for PhantomDialog.
    /// </summary>
    public enum PhantomPattern { Lattice, Random }

    public enum ModuleType
    {
        None = 0,

        SmtScanDetectors = 1,
        SmtFieldScan,

        SmtOutputProcessedImage = 10,
        SmtOutputImageVIew,

        SmtFilter = 20,

        SmtContourChannel = 40,
        SmtContourThreshold,
        SmtContourContour,

        SmtEventEvent = 50,
        SmtEventPhantom,
    }

}
