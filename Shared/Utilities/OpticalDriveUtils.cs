using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class OpticalDriveUtils
    {
        public static string[] GetDriveNames()
        {
            if (SysInfo.OSType == OSType.Windows)
                return WindowsGetOpticalDriveNames();
            else if (SysInfo.OSType == OSType.Linux)
                return LinuxGetOpticalDriveNames();
            else
                throw new UnsupportedOperatingSystemException();
        }

        public static string GetDriveLabel(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return WindowsGetDriveLabel(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return LinuxGetDriveLabel(drive);

            return null;
        }

        public static string GenerateDiscMD5(string drive)
        {
            if (SysInfo.OSType == OSType.Windows)
                return WindowsGenerateDiscMD5(drive);
            else if (SysInfo.OSType == OSType.Linux)
                return LinuxGenerateDiscMD5(drive);
            else
                throw new UnsupportedOperatingSystemException();
        }
    }
}
