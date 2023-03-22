//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using BOOL = System.Int32;

namespace FoxHollow.FHM.Shared.Native;

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

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
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