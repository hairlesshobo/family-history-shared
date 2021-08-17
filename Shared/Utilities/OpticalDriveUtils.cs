using System;
using System.IO;
using System.Linq;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static class OpticalDriveUtils
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


        #region Linux Utilities
        private static string LinuxGetOpticalDrivePath(string driveName)
            => (driveName.IndexOf('/') >= 0 ? driveName : $"/dev/{driveName}");

        private static string[] LinuxGetOpticalDriveNames()
        {
            if (!File.Exists("/proc/sys/dev/cdrom/info"))
                return new string[] { };

            string[] lines = File.ReadAllLines("/proc/sys/dev/cdrom/info");

            string driveNameLine = lines.FirstOrDefault(x => x.ToLower().StartsWith("drive name"));
            string stringValue = driveNameLine.Substring(driveNameLine.IndexOf(':')+1).Trim('\t').Trim();
            string[] driveNames = stringValue.Split('\t').Where(x => !String.IsNullOrWhiteSpace(x)).OrderBy(x => x).ToArray();

            return driveNames;
        }

        // private static TOut LinuxGetOpticalDriveCapability<TOut>(string driveName, string lineName)
        // {

        // }

        // private static TOut LinuxGetOpticalDriveCapability<TOut>(int driveIndex, string lineName)
        // {
        //     TOut returnValue = default(TOut);

        //     string[] lines = File.ReadAllLines("/proc/sys/dev/cdrom/info");

        //     string driveCapabilityLine = lines.FirstOrDefault(x => x.ToLower().StartsWith(lineName));

        //     if (String.IsNullOrWhiteSpace(driveCapabilityLine))
        //         return returnValue;

        //     string stringValue = driveCapabilityLine.Substring(driveCapabilityLine.IndexOf(':')+1).Trim('\t').Trim();
        //     string[] stringValueParts = stringValue.Split('\t');

        //     if (stringValueParts.Length < driveIndex)
        //         throw new DriveNotFoundException($"Unable to find optical drive number {driveIndex}");

        //     if (typeof(TOut) == typeof(string))
        //         returnValue = (TOut)((object)stringValueParts[driveIndex]);

        //     return returnValue;
        // }
        #endregion

        #region Windows Utilities
        private static string WindowsGetOpticalDrivePath(string driveName)
            => driveName;

        // TODO: Test that this actually works
        private static string[] WindowsGetOpticalDriveNames()
            => DriveInfo.GetDrives().Select(x => x.Name).ToArray();

        #endregion
    }
}
