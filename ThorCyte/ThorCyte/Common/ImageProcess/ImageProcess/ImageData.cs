
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ImageProcess
{
    public partial class ImageData : SafeHandleZeroOrMinusOneIsInvalid, ICloneable
    {

        #region Fields

        private const int ElementSize = sizeof (ushort);
        private readonly bool _isGray = true;
        private readonly IntPtr _dataBuffer;

        #endregion

        #region Properties

        public uint XSize { get; private set; }
        public uint YSize { get; private set; }

        public IntPtr DataBuffer
        {
            get { return _dataBuffer; }
        }

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

        public ImageData(uint xSize, uint ySize, bool isGray = true) : base(true)
        {
            _isGray = isGray;
            XSize = xSize;
            YSize = ySize;
            int totalBytes;
            checked
            {
                totalBytes = Length*ElementSize;
            }
            _dataBuffer = Marshal.AllocHGlobal(totalBytes);
            SetHandle(_dataBuffer);
            NativeMethods.RtlZeroMemory(this, (uint) totalBytes);
            
        }

        #endregion

        #region Methods

        #region Interface

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ImageData Clone()
        {
            var data = new ImageData(XSize, YSize, IsGray);
            NativeMethods.CopyMemory(data, this, (uint) (Length*sizeof (ushort)));
            return data;
        }

        #endregion

        #region Override base class 

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(DataBuffer);
            return true;
        }

        #endregion

        #endregion
    }

    internal class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern void RtlZeroMemory(SafeHandle dst, uint length);

        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(SafeHandle dest, SafeHandle src, uint count);


    }
}
