using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Native;
using Microsoft.Win32.SafeHandles;

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

        public static string GetDriveLabel(string drive)
        {
            if (SysInfo.OSType == OSType.Linux)
                return LinuxGetDriveLabel(drive);

            // TODO: Add windows here

            return null;
        }


        #region Linux Utilities
        /// <Summary>
        ///     Get the drive label for the provided drive ID
        /// </Summary>
        /// <Retuns>
        ///     The drive label, if found. If the drive does not have a label or no media is loaded, null is returned
        /// </Returns>
        private static string LinuxGetDriveLabel(string drive)
        {
            string byLabelDir = "/dev/disk/by-label";

            if (!Directory.Exists(byLabelDir))
                return null;

            string[] labels = Directory.GetFiles(byLabelDir);

            if (labels.Length == 0)
                return null;

            ulong driveInode = Linux.GetFileInode(LinuxGetOpticalDrivePath(drive));

            foreach (string deviceLabel in labels)
            {
                ulong labelInode = Linux.GetFileInode(deviceLabel);

                if (labelInode == driveInode)
                {
                    string label = deviceLabel.Substring(deviceLabel.LastIndexOf('/')+1)
                                              .Replace(@"\x20", " ");

                    return label;
                }
            }

            return null;
        }

        public static string LinuxGenerateDiscMD5(string driveName)
        {
            string drivePath = LinuxGetOpticalDrivePath(driveName);
            string md5Hash = String.Empty;

            int fd = Linux.Open(drivePath, Linux.OpenType.ReadOnly);

            try 
            {
                byte[] buffer = new byte[256 * 1024]; // 256KB buffer

                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    // Stopwatch sw = Stopwatch.StartNew();
                    int currentBlockSize = 0;

                    // long lastSample = sw.ElapsedMilliseconds;
                    long totalBytesRead = 0;
                    int md5Offset = 0;
                    // double currentPercent = 0.0;

                    while ((currentBlockSize = Linux.Read(fd, buffer, buffer.Length)) > 0)
                    {
                        totalBytesRead += currentBlockSize;

                        // currentPercent = ((double)totalBytesRead / (double)fileLength) * 100.0;
                        md5Offset += md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);
                        
                        // if (sw.ElapsedMilliseconds - lastSample > _sampleDurationMs || currentBlockSize < buffer.Length)
                        // {
                        //     OnProgressChanged(currentPercent);
                        //     lastSample = sw.ElapsedMilliseconds;
                        // }
                    }

                    // OnProgressChanged(currentPercent);
                    // lastSample = sw.ElapsedMilliseconds;

                    // sw.Stop();
                    md5.TransformFinalBlock(new byte[] { }, 0, 0);

                    // this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                    md5Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();

                    // OnComplete(this.MD5_Hash);
                }
            }
            finally
            {
                Linux.Close(fd);
            }

            return md5Hash;
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
        public static string WindowsGenerateDiscMD5(string driveName)
        {
            string drivePath = WindowsGetOpticalDrivePath(driveName);
            string md5Hash = String.Empty;

            SafeFileHandle handle = Windows.CreateFile(
                drivePath,
                // GENERIC_READ | GENERIC_WRITE,
                Windows.GENERIC_READ,
                Windows.FILE_SHARE_READ | Windows.FILE_SHARE_WRITE,
                IntPtr.Zero,
                Windows.OPEN_EXISTING,
                0,
                //FILE_ATTRIBUTE_ARCHIVE | FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero
                );

            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
                // throw new NativeMethodException("CreateFile", Marshal.GetLastWin32Error());
                // throw new TapeOperatorWin32Exception("CreateFile", Marshal.GetLastWin32Error());

            byte[] buffer = new byte[256 * 1024]; // 256KB buffer


            using (FileStream stream = new FileStream(handle, FileAccess.Read, (int)buffer.Length, false))
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                // Stopwatch sw = Stopwatch.StartNew();
                int currentBlockSize = 0;

                // long lastSample = sw.ElapsedMilliseconds;
                long totalBytesRead = 0;
                int md5Offset = 0;
                // double currentPercent = 0.0;

                // m_stream.Read(buffer, 0, buffer.Length);
                // m_stream.Flush();

                while ((currentBlockSize = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytesRead += currentBlockSize;

                    // currentPercent = ((double)totalBytesRead / (double)fileLength) * 100.0;
                    md5Offset += md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);
                    
                    // if (sw.ElapsedMilliseconds - lastSample > _sampleDurationMs || currentBlockSize < buffer.Length)
                    // {
                    //     OnProgressChanged(currentPercent);
                    //     lastSample = sw.ElapsedMilliseconds;
                    // }
                }

                // OnProgressChanged(currentPercent);
                // lastSample = sw.ElapsedMilliseconds;

                // sw.Stop();
                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                // this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                md5Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();

                // OnComplete(this.MD5_Hash);
            }

            return md5Hash;
        }

        private static string WindowsGetOpticalDrivePath(string driveName)
        {
            if (driveName.StartsWith(@"\\.\"))
                return driveName;
                
            driveName = driveName.Trim('/');
            driveName = driveName.Trim('\\');
            driveName = driveName.ToUpper();

            if (!driveName.EndsWith(":"))
                driveName += ":";

            return @"\\.\" + driveName;
        }

        // TODO: Test that this actually works
        private static string[] WindowsGetOpticalDriveNames()
            => DriveInfo.GetDrives().Select(x => x.Name).ToArray();

        #endregion
    }
}
