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
using FoxHollow.FHM.Shared.Exceptions;

namespace FoxHollow.FHM.Shared.Native;

public static partial class Linux
{
    // TODO: Add file existence checking for open, etc.

    [DllImport("linux.so", EntryPoint = "eject_scsi", SetLastError = true)]
    public static extern int EjectScsi(int handle);

    [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
    public static extern int Open(string fileName, uint mode);

    [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
    private static extern int Open(string fileName, int mode);
    public static int Open(string fileName, OpenType openType)
        => Open(fileName, (int)openType);


    [DllImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
    public static extern int Close(int handle);



    [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
    public static extern int Read(int handle, byte[] data, int length);

    [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
    public static extern int Write(int handle, byte[] data, int length);

    [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
    public extern static int Ioctl(int fd, ulong request, uint param);

    [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
    public extern static int Ioctl(int fd, ulong request, int param);
    public static int Ioctl(int fd, ulong request)
         => Ioctl(fd, request, 0);

    [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
    public extern static int Ioctl(int fd, ulong request, IntPtr data);

    [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
    public extern static int Ioctl(int fd, ulong request, out UInt64 data);


    [DllImport("libc.so.6", EntryPoint = "__xstat", SetLastError = true)]
    private static extern int __XStat(int statVersion, string path, IntPtr statResult);

    private static int _Stat(string path, IntPtr statResult)
        => __XStat(1, path, statResult);

    public static int Stat(string path, ref Linux.StatResult statResult)
    {
        IntPtr ptr = IntPtr.Zero;
        int result = -1;
        statResult = new Linux.StatResult();

        try
        {
            // Allocate unmanaged memory
            int size = Marshal.SizeOf(statResult);
            ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(
                statResult,
                ptr,
                false
            );

            if (Linux._Stat(path, ptr) < 0)
                throw new NativeMethodException("Ioctl");

            statResult = (Linux.StatResult)Marshal.PtrToStructure(ptr, typeof(Linux.StatResult));

        }
        finally
        {
            // Free unmanaged memory
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }

        return result;
    }

    public static ulong GetFileInode(string path)
    {
        StatResult result = new StatResult();
        Stat(path, ref result);

        return result.Inode;
    }
}