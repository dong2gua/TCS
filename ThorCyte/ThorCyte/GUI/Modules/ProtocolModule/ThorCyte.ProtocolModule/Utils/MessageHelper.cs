using System;

namespace ThorCyte.ProtocolModule.Utils
{
    public class MessageHelper
    {
        public delegate void StatusMessageHandler(string msg);
        public static StatusMessageHandler SetMessage;

        public static void PostMessage(string msg)
        {
            if (SetMessage == null) return;
            msg = DateTime.Now.ToString("HH:mm:ss  ") + msg;
            SetMessage.BeginInvoke(msg, null, null);
        }
    }
}
