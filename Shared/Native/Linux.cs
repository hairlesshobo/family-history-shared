using System;
using System.Runtime.InteropServices;

namespace Archiver.Shared.Native
{
    public static partial class Linux
    {
        // TODO: Add file existence checking for open, etc.

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

                result = Linux._Stat(path, ptr);

                // TODO: Error handling

                statResult = (Linux.StatResult)Marshal.PtrToStructure(ptr, typeof(Linux.StatResult));
                
            }
            finally
            {
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