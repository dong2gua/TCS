using Prism.Events;
using ThorCyte.ImageViewerModule.Model;

namespace ThorCyte.ImageViewerModule.Events
{
    public class UpdateCurrentChannelEvent : PubSubEvent<ChannelImage>
    {
    }
}
