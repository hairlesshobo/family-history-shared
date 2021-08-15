using System;
using System.Runtime.InteropServices;

namespace Archiver.TapeServer.TapeDrivers
{
    public partial class NativeLinuxTapeDriver : IDisposable
    {
        [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
        private static extern int Open(string fileName, int mode);
        
        [DllImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
        private static extern int Close(int handle);



        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        private static extern int Read(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        private static extern int Write(int handle, byte[] data, int length);

        

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        // private extern static int Ioctl(int fd, int request, int data);
        // private extern static int Ioctl(int fd, ulong request, ulong data);
        private extern static int Ioctl(int fd, ulong request, IntPtr data);

    }
}
