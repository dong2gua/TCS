namespace ThorCyte.ProtocolModule.Models
{
    /// <summary>
    /// Summary description for Port.
    /// </summary>
    public enum PortDataType { None,BinaryImage, GrayImage, MultiChannelImage, Image, Event, Setting, Flag }

    public enum PortType { None = 0, InPort, OutPort }

    public enum OpenVirMode { None, New, Edit }

    /// <summary>
    /// Summary description for ThresholdMod.
    /// </summary>
    public enum ThresholdMethod { Manual, Otsu }

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

        //According to category
        SmtSystemCategory,
        SmtScanModule,

        SmtOutputCategory,
        SmtImageViewModule,

        SmtFilterCategory,
        SmtFilterModule,

        SmtOperationCategory,
        SmtAddModule,
        SmtSubtractModule,
        SmtInvertModule,
        SmtAndModule,
        SmtOrModule,
        SmtXorModule,
        SmtBrightContrastModule,
        SmtTransformModule,

        SmtContourCategory,
        SmtChannelModule,
        SmtThresholdModule,
        SmtContourModule,

        SmtEventCategory,
        SmtEventModule,
        SmtPhantomModule,
        SmtOverlapParentChildModule,
        SmtOverlapParentChild2Module,

        SmtExperimentalCategory,
        SmtAdvancedImageAnalysisCategory,
        SmtCustomModulesCategory,
        //SmtOutputProcessedImage = 10,
        //SmtOutputImageVIew,

        //SmtFilter = 20,

        //SmtContourChannel = 40,
        //SmtContourThreshold,
        //SmtContourContour,

        //SmtEventEvent = 50,
        //SmtEventPhantom,

        SmtCombinedModule,

    }

}
