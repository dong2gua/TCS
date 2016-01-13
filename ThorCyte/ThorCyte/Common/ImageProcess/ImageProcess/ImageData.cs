
using System;
using System.Runtime.InteropServices;

namespace ImageProcess
{

    public class ImageData : IDisposable, ICloneable
    {

        [DllImport("kernel32.dll")]
        private static extern void RtlZeroMemory(IntPtr dst, int length);
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);


        #region Fields

        private const int ElementSize = sizeof (ushort);
        private readonly bool _isGray = true;
        private bool _disposed;

        #endregion

        #region Properties

        public uint XSize { get; private set; }
        public uint YSize { get; private set; }

        public IntPtr DataBuffer { get; private set; }

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

        public ushort this[int index]
        {
            get { return (ushort) Marshal.ReadInt16(DataBuffer, index*ElementSize); }
            set { Marshal.WriteInt16(DataBuffer, index*ElementSize, (short) value); }
        }

        public ushort this[long index]
        {
            get { return (ushort) Marshal.ReadInt16(DataBuffer, (int) (index*ElementSize)); }
            set { Marshal.WriteInt16(DataBuffer, (int) (index*ElementSize), (short) value); }
        }

        public int Length
        {
            get { return (int) (Channels*XSize*YSize); }
        }

        #endregion

        #region Constructors

        public ImageData(uint xSize, uint ySize, bool isGray = true)
        {
            _isGray = isGray;
            XSize = xSize;
            YSize = ySize;
            int totalBytes;
            checked
            {
                totalBytes = (int) (Channels*xSize*ySize*ElementSize);
            }
            DataBuffer = Marshal.AllocHGlobal(totalBytes);
            RtlZeroMemory(DataBuffer, totalBytes);
        }


      
        #endregion


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // Free any other managed objects here.

                }

                // Free any unmanaged objects here.
                if (DataBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(DataBuffer);
                    DataBuffer = IntPtr.Zero;
                }

            }
        }

        ~ImageData()
        {
            Dispose(false);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ImageData Clone()
        {
            var data = new ImageData(XSize, YSize, IsGray);
            CopyMemory(data.DataBuffer, DataBuffer, (uint) (XSize*YSize*Channels*sizeof (ushort)));
            return data;
        }
    }
}
