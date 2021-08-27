using System;
using System.Collections.Generic;
using System.IO;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class OpticalDriveUtils
    {
        public static string[] GetDriveNames()
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetOpticalDriveNames();
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetOpticalDriveNames();
            else
                throw new UnsupportedOperatingSystemException();
        }

        public static string GetDriveLabel(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDriveLabel(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDriveLabel(drive);

            return null;
        }

        public static Stream GetDriveRawStream(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDriveRawStream(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDriveRawStream(drive);

            return null;
        }

        [Obsolete]
        public static string GenerateDiscMD5(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GenerateDiscMD5(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GenerateDiscMD5(drive);
            else
                throw new UnsupportedOperatingSystemException();
        }

        public static List<OpticalDrive> GetDrives()
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDrives();
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDrives();
            else
                throw new UnsupportedOperatingSystemException();
        }


        public static void EjectDrive(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                Windows.EjectDrive(drive);
            else if (SysInfo.OSType == OSType.Linux)
                Linux.EjectDrive(drive);
            else
                throw new UnsupportedOperatingSystemException();
        }
    }
}
