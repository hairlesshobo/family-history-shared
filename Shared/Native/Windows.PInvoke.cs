/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Runtime.InteropServices;
using FoxHollow.Archiver.Shared.TapeDrivers;
using Microsoft.Win32.SafeHandles;
using BOOL = System.Int32;

namespace FoxHollow.Archiver.Shared.Native
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