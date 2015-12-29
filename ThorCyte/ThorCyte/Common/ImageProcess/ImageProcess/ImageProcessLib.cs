
using System;
using System.Runtime.InteropServices;

namespace ImageProcess
{
    public class ImageProcessLib
    {
        #region DllImport
        private const string DllName = "ImageProcessLib.dll";

        [DllImport(DllName, EntryPoint = "fnipp_lib_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitImageProcessLib();

        [DllImport(DllName, EntryPoint = "fnipp_lib_resize_16uC1", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Resize16UC1(IntPtr srcBuffer, int srcWidth, int srcHeight, IntPtr dstBuffer,
            int dstWidth, int dstHeight);

        [DllImport(DllName, EntryPoint = "fnipp_lib_AddConstant_16uC1", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int AddConstant16UC1(ushort value, IntPtr srcBuffer, int width, int height,
            IntPtr dstBuffer);

        [DllImport(DllName, EntryPoint = "fnipp_lib_SubConstant_16uC1", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SubConstant16UC1(ushort value, IntPtr srcBuffer, int width, int height,
            IntPtr dstBuffer);

        [DllImport(DllName, EntryPoint = "fnipp_lib_MulConstant_16uC1", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MulConstant16UC1(ushort value, IntPtr srcBuffer, int width, int height,
            IntPtr dstBuffer);
        #endregion

        static ImageProcessLib()
        {
            InitImageProcessLib();
        }
    }
}
