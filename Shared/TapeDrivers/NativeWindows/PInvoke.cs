using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

#region Typedefenitions
using BOOL = System.Int32;
#endregion

namespace Archiver.Shared.TapeDrivers
{
    public partial class NativeWindowsTapeDriver
    {
        #region PInvoke
        // Use interop to call the CreateFile function.
        // For more information about CreateFile,
        // see the unmanaged MSDN reference library.
        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int PrepareTape(
            SafeFileHandle handle,
            int prepareType,
            BOOL isImmediate
            );


        [DllImport( "kernel32", SetLastError = true )]
        private static extern int SetTapePosition(
            SafeFileHandle handle,
            int positionType,
            int partition,
            int offsetLow,
            int offsetHigh,
            BOOL isImmediate
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int GetTapePosition(
            SafeFileHandle handle,
            int positionType,
            out int partition,
            out int offsetLow,
            out int offsetHigh
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int WriteTapemark(
            SafeFileHandle handle,
            int tapemarkType,
            int tapemarkCount,
            BOOL isImmediate
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int GetTapeParameters(
           SafeFileHandle handle,
           int operationType,
           ref int size,
           IntPtr mediaInfo
           );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int SetTapeParameters(
           SafeFileHandle handle,
           int operationType,
           TapeSetDriveParameters parameters
           );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int GetLastError();
        #endregion
    }
}