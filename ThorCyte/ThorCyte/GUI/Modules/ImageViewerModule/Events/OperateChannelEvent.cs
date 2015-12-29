using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using System.Windows;
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
