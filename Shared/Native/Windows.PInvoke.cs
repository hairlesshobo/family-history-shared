using System;
using System.Runtime.InteropServices;
using Archiver.Shared.TapeDrivers;
using Microsoft.Win32.SafeHandles;
using BOOL = System.Int32;

namespace Archiver.Shared.Native
{
    public static partial class Windows
    {
        // Use interop to call the CreateFile function.
        // For more information about CreateFile,
        // see the unmanaged MSDN reference library.
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int PrepareTape(
            SafeFileHandle handle,
            int prepareType,
            BOOL isImmediate
            );


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetTapePosition(
            SafeFileHandle handle,
            int positionType,
            int partition,
            int offsetLow,
            int offsetHigh,
            BOOL isImmediate
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetTapePosition(
            SafeFileHandle handle,
            int positionType,
            out int partition,
            out int offsetLow,
            out int offsetHigh
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteTapemark(
            SafeFileHandle handle,
            int tapemarkType,
            int tapemarkCount,
            BOOL isImmediate
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetTapeParameters(
           SafeFileHandle handle,
           int operationType,
           ref int size,
           IntPtr mediaInfo
           );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetTapeParameters(
           SafeFileHandle handle,
           int operationType,
           TapeSetDriveParameters parameters
           );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            // IntPtr driveHandle,
            SafeFileHandle handle,
            uint IoControlCode,
            IntPtr lpInBuffer,
            uint inBufferSize,
            IntPtr lpOutBuffer,
            uint outBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            ref uint lpNumberOfBytesRead,
            IntPtr lpOverlapped
            );
    }
}