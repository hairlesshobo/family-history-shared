using System;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Native;

namespace Archiver.Shared.Utilities
{
    public static partial class DiskUtils
    {
        public static ulong LinuxGetDiskSize(string drivePath)
        {
            int handle = Linux.Open(drivePath, Linux.OpenType.ReadOnly);;

            try
            {
                return LinuxGetDiskSize(handle);
            }
            finally
            {
                if (handle > 0)
                    Linux.Close(handle);
            }
        }

        public static ulong LinuxGetDiskSize(int handle)
        {
            ulong returnValue64 = 0;

            if (Linux.Ioctl(handle, Linux.BLKGETSIZE64, out returnValue64) < 0)
                throw new NativeMethodException("Ioctl");

            return returnValue64;
        }

        public static long LinuxGetFileSize(string devicePath)
        {
            Linux.StatResult result = new Linux.StatResult();
            Linux.Stat(devicePath, ref result);

            return result.Size;
        }
    }
}