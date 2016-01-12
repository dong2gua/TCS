using Prism.Events;
using System.Windows;

namespace ThorCyte.ImageViewerModule.Events
{
    public class UpdateMousePointEvent : PubSubEvent<MousePointStatus>
    {
    }
    public class MousePointStatus
    {
        public double Scale { get; set; }
        public Point Point { get; set; }
        public double GrayValue { get; set; }
        public bool IsComputeColor { get; set; }
    }
}
