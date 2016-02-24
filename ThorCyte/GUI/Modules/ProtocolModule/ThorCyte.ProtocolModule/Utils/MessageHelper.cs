using System;

namespace ThorCyte.ProtocolModule.Utils
{
    public class MessageHelper
    {
        public delegate void StatusMessageHandler(string msg);
        public static StatusMessageHandler SetMessage;

        public delegate void ProgressMessageHandler(string type, int max, int value);
        public static ProgressMessageHandler SetProgress;

        public delegate void MacroRunningHandler(bool isRuning);
        public static MacroRunningHandler SetRuning;

        public delegate void SetSelectViewItemHandler(object item);
        public static SetSelectViewItemHandler SetSelectViewItem;

        public static SetSelectViewItemHandler UnSelectViewItem;

        public static void PostMessage(string msg)
        {
            if (SetMessage == null) return;
            msg = DateTime.Now.ToString("HH:mm:ss  ") + msg;
            SetMessage.BeginInvoke(msg, null, null);
        }

        public static void PostProgress(string type, int max, int value)
        {
            if (SetProgress == null) return;
            SetProgress.BeginInvoke(type, max, value, null, null);
        }

        public static void SendMacroRuning(bool isRunding)
        {
            if(SetRuning == null) return;
            SetRuning.Invoke(isRunding);
        }

        public static void SetSelectItem(object item)
        {
            if (SetSelectViewItem == null) return;
            SetSelectViewItem.Invoke(item);
        }

        public static void UnSelectItem(object item)
        {
            if (UnSelectViewItem == null) return;
            UnSelectViewItem.Invoke(item);
        }
    }
}
