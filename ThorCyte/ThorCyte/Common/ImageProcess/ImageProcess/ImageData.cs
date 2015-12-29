
namespace ImageProcess
{
    public class ImageData
    {
        private readonly bool _isGray = true;
        public uint XSize { get; private set; }
        public uint YSize { get; private set; }

        public ushort[] DataBuffer { get; private set; }

        public bool IsGray
        {
            get { return _isGray; }
        }

        public int BitsPerPixel 
        {
            get { return IsGray ? 16 : 48; }
        }

        public int Channels
        {
            get { return IsGray ? 1 : 3; }
        }

        public ImageData(uint xSize, uint ySize, bool isGray = true)
        {
            _isGray = isGray;
            XSize = xSize;
            YSize = ySize;
            DataBuffer = new ushort[xSize*ySize*Channels];
#if DEBUG
            for (int i = 0; i < DataBuffer.Length; i++)
                DataBuffer[i] = 0x01 << 15;
#endif
        }
    }
}
