using System;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class DiskUtils
    {
        public static ulong GetDiskSize(string driveName)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDiskSize(driveName);
            else if (SysInfo.OSType == OSType.Linux)
                return Windows.GetDiskSize(driveName);

            throw new UnsupportedOperatingSystemException();
        }
    }
}