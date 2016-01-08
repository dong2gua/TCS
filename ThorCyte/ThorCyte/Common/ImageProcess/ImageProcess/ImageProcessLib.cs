
using System;
using System.Runtime.InteropServices;

namespace ImageProcess
{
    public class ImageProcessLib
    {
        #region DllImport
        private const string DllName = "ImageProcessLib.dll";
        public const int IppStsNoErr = 0;

        [DllImport(DllName, EntryPoint = "fnipp_lib_getCpuClocks", CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 GetCpuClocks();
        [DllImport(DllName, EntryPoint = "fnipp_lib_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitImageProcessLib();

        [DllImport(DllName, EntryPoint = "fnipp_lib_resize_16u", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Resize16U(IntPtr srcBuffer, int srcWidth, int srcHeight, IntPtr dstBuffer,
            int dstWidth, int dstHeight, int channels);

        [DllImport(DllName, EntryPoint = "fnipp_lib_addConstant_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int AddConstant16U(ushort value, IntPtr srcBuffer, int width, int height,
            IntPtr dstBuffer, int channels, ushort maxValue);

        [DllImport(DllName, EntryPoint = "fnipp_lib_add_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Add16U(IntPtr srcBuffer1, IntPtr srcBuffer2, int width, int height,
            IntPtr dstBuffer, int channels, ushort maxValue);

        [DllImport(DllName, EntryPoint = "fnipp_lib_subConstant_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SubConstant16U(ushort value, IntPtr srcBuffer, int width, int height,
            IntPtr dstBuffer, int channels);

        [DllImport(DllName, EntryPoint = "fnipp_lib_sub_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Sub16U(IntPtr minuendBuffer, IntPtr subtracterBuffer, int width, int height,
            IntPtr dstBuffer, int channels);

        [DllImport(DllName, EntryPoint = "fnipp_lib_mulConstant_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MulConstant16U(ushort value, IntPtr srcBuffer, int width, int height,
            IntPtr dstBuffer, int channels, ushort maxValue);

        [DllImport(DllName, EntryPoint = "fnipp_lib_max_16u",CallingConvention=CallingConvention.Cdecl)]
        internal static extern int Max16U(IntPtr srcBuffer, int width, int height, int channels, out ushort maxValue);

        [DllImport(DllName, EntryPoint = "fnipp_lib_min_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Min16U(IntPtr srcBuffer, int width, int height, int channels, out ushort minValue);

        [DllImport(DllName, EntryPoint = "fnipp_lib_maxEvery_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MaxEvery16U(IntPtr firstBuffer, IntPtr secondBuffer, int width, int height,
            int channels, IntPtr dstBuffer);

        [DllImport(DllName, EntryPoint = "fnipp_lib_minEvery_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MinEvery16U(IntPtr firstBuffer, IntPtr secondBuffer, int width, int height,
            int channels, IntPtr dstBuffer);

        [DllImport(DllName, EntryPoint = "fnipp_lib_invert_16u", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Invert16U(IntPtr srcBuffer, int width, int height, int channels, ushort maxValue,
            IntPtr dstBuffer);
        #endregion

        static ImageProcessLib()
        {
            InitImageProcessLib();
        }
    }
}
