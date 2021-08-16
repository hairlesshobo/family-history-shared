using System;
using System.Runtime.InteropServices;

namespace Archiver.TapeServer.TapeDrivers
{
    public partial class NativeLinuxTapeDriver : IDisposable
    {
        [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
        private static extern int POpen(string fileName, int mode);
        
        [DllImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
        private static extern int PClose(int handle);



        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        private static extern int PRead(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        private static extern int PWrite(int handle, byte[] data, int length);


        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        private extern static int PIoctl(int fd, ulong request, IntPtr data);

    }
}
