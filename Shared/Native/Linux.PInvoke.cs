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
using FoxHollow.Archiver.Shared.Exceptions;

namespace FoxHollow.Archiver.Shared.Native
{
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
}