
namespace ComponentDataService.Types
{
    public class BlobDefine
    {
        public int DataExpand { get; set; }
        public int BackgroundDistance { get; set; }
        public int BackgroundWidth { get; set; }
        public int BackgroundLowBoundPercent { get; set; }
        public int BackgroundHighBoundPercent { get; set; }
        public int PeripheralDistance { get; set; }
        public int PeripheralWidth { get; set; }
        public bool IsDynamicBackground { get; set; }
        public bool IsPeripheral { get; set; }
        public bool[] DynamicBkCorrections { get; set; }
    }
}
