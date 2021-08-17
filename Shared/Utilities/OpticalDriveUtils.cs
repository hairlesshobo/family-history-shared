using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public static void GetDriveLabel(string drive)
        {
            if (SysInfo.OSType == OSType.Linux)
                LinuxGetDriveLabel(drive);
        }


        #region Linux Utilities
        [StructLayout(LayoutKind.Sequential)] 
        public struct StatResult {
               ulong     st_dev;         /* ID of device containing file */
               ulong     st_ino;         /* Inode number */
               ulong   st_nlink;       /* Number of hard links */
               uint    st_mode;        /* File type and mode */
               uint     st_uid;         /* User ID of owner */
               uint     st_gid;         /* Group ID of owner */
               int pad0;
               ulong     st_rdev;        /* Device ID (if special file) */
               long     st_size;        /* Total size, in bytes */
               long st_blksize;     /* Block size for filesystem I/O */
               long   st_blocks;      /* Number of 512B blocks allocated */

               /* Since Linux 2.6, the kernel supports nanosecond
                  precision for the following timestamp fields.
                  For the details before Linux 2.6, see NOTES. */

            //    struct timespec st_atim;  /* Time of last access */
            //    struct timespec st_mtim;  /* Time of last modification */
            //    struct timespec st_ctim;  /* Time of last status change */

        //    #define st_atime st_atim.tv_sec      /* Backward compatibility */
        //    #define st_mtime st_mtim.tv_sec
        //    #define st_ctime st_ctim.tv_sec
           };

        [DllImport("libc.so.6", EntryPoint = "__xstat", SetLastError = true)]
        private static extern int Stat(int statVersion, string path, IntPtr statResult);

        private static string LinuxGetDriveLabel(string drive)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                StatResult statResult = new StatResult();

                // Allocate unmanaged memory
                int size = Marshal.SizeOf(statResult);
                ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(
                    statResult,
                    ptr,
                    false
                );

                int result = Stat(1, LinuxGetOpticalDrivePath(drive), ptr);

                statResult = (StatResult)Marshal.PtrToStructure(ptr, typeof(StatResult));
                
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }

            return String.Empty;
        }

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
