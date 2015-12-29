using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using System.Windows;
namespace ThorCyte.ImageViewerModule.Events
{
    public class UpdateProfilePointsEvent:PubSubEvent<ProfilePoints>
    {
    }
    public class ProfilePoints
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
    }
}
