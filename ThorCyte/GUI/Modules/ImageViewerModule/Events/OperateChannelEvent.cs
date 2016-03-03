using Prism.Events;

namespace ThorCyte.ImageViewerModule.Events
{
    public class OperateChannelEvent:PubSubEvent<OperateChannelArgs>
    {
    }
    public class OperateChannelArgs
    {
        public string ChannelName { get; set; }
        public bool IsComputeColor { get; set; }
        public int Operator { get; set; }
    }
}
