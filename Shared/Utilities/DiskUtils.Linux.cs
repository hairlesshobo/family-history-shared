using System;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Native;

namespace Archiver.Shared.Utilities
{
    public static partial class DiskUtils
    {
        public static class Linux
        {
            public static ulong GetDiskSize(string driveName)
            {
                string drivePath = PathUtils.GetDrivePath(driveName);

                int handle = Native.Linux.Open(drivePath, Native.Linux.OpenType.ReadOnly);;

                try
                {
                    return GetDiskSize(handle);
                }
                finally
                {
                    if (handle > 0)
                        Native.Linux.Close(handle);
                }
            }

            public static ulong GetDiskSize(int handle)
            {
                ulong returnValue64 = 0;

                if (Native.Linux.Ioctl(handle, Native.Linux.BLKGETSIZE64, out returnValue64) < 0)
                    throw new NativeMethodException("Ioctl");

                return returnValue64;
            }

            public static long GetFileSize(string devicePath)
            {
                Native.Linux.StatResult result = new Native.Linux.StatResult();
                Native.Linux.Stat(devicePath, ref result);

                return result.Size;
            }
        }
    }
}