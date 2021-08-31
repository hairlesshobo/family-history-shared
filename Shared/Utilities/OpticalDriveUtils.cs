using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class OpticalDriveUtils
    {
        public static List<OpticalDrive> GetDrives()
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDrives();
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDrives();
            
            throw new UnsupportedOperatingSystemException();
        }


        public static OpticalDrive GetDriveByName(string driveName)
            => GetDrives().FirstOrDefault(x => x.Name.ToLower() == driveName.ToLower());

        public static string[] GetDriveNames()
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetOpticalDriveNames();
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetOpticalDriveNames();
            
            throw new UnsupportedOperatingSystemException();
        }

        public static string GetDriveLabel(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDriveLabel(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDriveLabel(drive);

            throw new UnsupportedOperatingSystemException();
        }

        public static Stream GetDriveRawStream(OpticalDrive drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDriveRawStream(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDriveRawStream(drive);

            throw new UnsupportedOperatingSystemException();
        }

        public static void EjectDrive(OpticalDrive drive)
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
