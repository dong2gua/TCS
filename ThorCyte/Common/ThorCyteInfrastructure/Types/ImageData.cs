
namespace ThorCyte.Infrastructure.Types
{
    public class ImageData
    {
        public uint XSize { get; private set; }
        public uint YSize { get; private set; }

        public ushort[] DataBuffer{ get; private set; }

        public ImageData(uint xSize, uint ySize)
        {
            XSize = xSize;
            YSize = ySize;
            DataBuffer = new ushort[xSize*ySize];
        }
    }
}
