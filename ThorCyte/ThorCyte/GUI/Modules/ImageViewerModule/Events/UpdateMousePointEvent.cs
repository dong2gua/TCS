using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using System.Windows;
using ThorCyte.ImageViewerModule.Model;

namespace ThorCyte.ImageViewerModule.Events
{
    public class UpdateMousePointEvent : PubSubEvent<MousePointStatus>
    {
    }
    public class MousePointStatus
    {
        public Point Point { get; set; }
        public double GrayValue { get; set; }
        public bool IsComputeColor { get; set; }

    }

}
