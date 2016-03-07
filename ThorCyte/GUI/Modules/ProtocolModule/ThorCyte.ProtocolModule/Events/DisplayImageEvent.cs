using System;
using System.Windows.Media;
using Prism.Events;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte.ProtocolModule.Events
{
    public class DisplayImageEvent : PubSubEvent<DisplayImageEventArgs>
    {
    }

    public class DisplayImageEventArgs
    {
        public ImageSource Image;
        public string Title;

        public DisplayImageEventArgs()
        { 
            Title = string.Empty;
            Image = null;
        }

        public DisplayImageEventArgs(string title,ImageSource image)
        {
            Title = title;
            Image = image;
        }

    }

}
