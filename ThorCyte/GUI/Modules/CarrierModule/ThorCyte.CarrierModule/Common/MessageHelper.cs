using System;

namespace ThorCyte.CarrierModule.Common
{
    public class MessageHelper
    {
        public delegate void StreamScanHandler(bool isStreaming);
        public static StreamScanHandler SetStreaming;

        public static void SendStreamingStatus(bool isStreaming)
        {
            if (SetStreaming == null) return;
            SetStreaming.Invoke(isStreaming);
        }

    }
}
