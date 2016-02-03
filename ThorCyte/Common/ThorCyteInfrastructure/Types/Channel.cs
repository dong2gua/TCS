namespace ThorCyte.Infrastructure.Types
{
    public class Channel
    {
        #region Properties

        public string ChannelName { get; set; }
        public int ChannelId { get; set; }
        public int WaveLength { get; set; }
        public virtual bool IsvirtualChannel{get { return false; }}
        public int Brightness { set; get; }
        public double Contrast { set; get; }

        public Channel()
        {
            Brightness = 0;
            Contrast = 1.0;
        }

        #endregion

    }

    public class VirtualChannel : Channel
    {
        public override bool IsvirtualChannel { get { return true; }}

        public Channel FirstChannel { set; get; }
        public Channel SecondChannel { set; get; }
        public ImageOperator Operator { set; get; }
        public double Operand { set; get; } 
    }
}
