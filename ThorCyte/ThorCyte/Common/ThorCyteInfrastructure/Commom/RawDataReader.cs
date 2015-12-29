using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ThorCyte.Infrastructure.Commom
{
    public class RawDataReader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReadFile(IntPtr handle, IntPtr buffer, uint numBytesToRead, out uint numBytesRead, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        unsafe public static bool ReadBuffer(string filePath, ushort[] buffer, int offset, int count)
        {
            FileStream fs = File.Open(filePath, FileMode.Open);
            if (null == fs) throw new ArgumentNullException();
            uint bytesToRead = (uint)(2 * count);
            IntPtr nativeHandle = fs.SafeFileHandle.DangerousGetHandle();
            try
            {
                long unused;
                if (!SetFilePointerEx(nativeHandle, offset, out unused, 0))
                    return false;
                fixed (ushort* pFirst = &buffer[0])
                    if (!ReadFile(nativeHandle, new IntPtr(pFirst), bytesToRead, out bytesToRead, IntPtr.Zero))
                        return false;
                if (bytesToRead < 2 * count)
                    return false;
                return true;
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
