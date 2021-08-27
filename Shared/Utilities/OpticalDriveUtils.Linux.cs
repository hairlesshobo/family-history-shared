using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Archiver.Shared.Classes;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Native;
using Microsoft.Win32.SafeHandles;

namespace Archiver.Shared.Utilities
{
    public static partial class OpticalDriveUtils
    {
        private static class Linux
        {
            /// <Summary>
            ///     Get the drive label for the provided drive ID
            /// </Summary>
            /// <Retuns>
            ///     The drive label, if found. If the drive does not have a label or no media is loaded, null is returned
            /// </Returns>
            internal static string GetDriveLabel(string driveName)
            {
                string byLabelDir = "/dev/disk/by-label";

                if (!Directory.Exists(byLabelDir))
                    return null;

                string[] labels = Directory.GetFiles(byLabelDir);

                if (labels.Length == 0)
                    return null;

                ulong driveInode = Native.Linux.GetFileInode(PathUtils.LinuxGetDrivePath(driveName));

                foreach (string deviceLabel in labels)
                {
                    ulong labelInode = Native.Linux.GetFileInode(deviceLabel);

                    if (labelInode == driveInode)
                    {
                        string label = deviceLabel.Substring(deviceLabel.LastIndexOf('/')+1)
                                                .Replace(@"\x20", " ");

                        return label;
                    }
                }

                return null;
            }

            internal static Stream GetDriveRawStream(string driveName)
                => new LinuxNativeStreamReader(LinuxNativeStreamReader.StreamSourceType.Disk, driveName);

            internal static List<OpticalDrive> GetDrives()
            {
                List<OpticalDrive> allDrives = new List<OpticalDrive>();

                allDrives = GetOpticalDriveNames().Select(x => 
                {
                    bool discLoaded = IsDiscLoaded(x);

                    return new OpticalDrive()
                    {
                        Name = x,
                        IsDiscLoaded = discLoaded,
                        IsReady = discLoaded,
                        FullPath = PathUtils.LinuxGetDrivePath(x),
                        VolumeLabel = (discLoaded ? GetDriveLabel(x) : null),
                        MountPoint = (discLoaded ? GetMountPoint(x) : null),
                        VolumeFormat = (discLoaded ? GetMountFormat(x)?.ToUpper() : null)
                    };
                }).ToList();

                return allDrives;
            }

            internal static void EjectDrive(string driveName)
            {
                string drivePath = PathUtils.LinuxGetDrivePath(driveName);

                int handle = Native.Linux.Open(drivePath, Native.Linux.O_RDONLY | Native.Linux.O_NONBLOCK);

                if (handle < 0)
                    throw new NativeMethodException("Open");

                try
                {
                    int result = Native.Linux.Ioctl(handle, Native.Linux.CDROMEJECT);

                    if (result < 0)
                        throw new NativeMethodException("Ioctl");

                    return;
                }
                finally
                {
                    if (handle >= 0)
                        Native.Linux.Close(handle);
                }
            }

            private static void ConfigureDrive(string driveName)
            {
                string drivePath = PathUtils.LinuxGetDrivePath(driveName);

                int handle = Native.Linux.Open(drivePath, Native.Linux.OpenType.ReadOnly);

                if (handle < 0)
                {
                    int errno = Marshal.GetLastWin32Error();

                    if (errno == Native.Linux.ENOMEDIUM)
                        return;
                        
                    throw new NativeMethodException("Open");
                }

                try
                {
                    int result = Native.Linux.Ioctl(handle, Native.Linux.CDROM_SET_OPTIONS, Native.Linux.CDO_AUTO_CLOSE);

                    if (result < 0)
                        throw new NativeMethodException("Ioctl");

                    return;
                }
                finally
                {
                    if (handle >= 0)
                        Native.Linux.Close(handle);
                }
            }

            private static bool IsDiscLoaded(string driveName)
            {
                string drivePath = PathUtils.LinuxGetDrivePath(driveName);

                int handle = Native.Linux.Open(drivePath, Native.Linux.O_RDONLY | Native.Linux.O_NONBLOCK);

                if (handle < 0)
                {
                    int errno = Marshal.GetLastWin32Error();

                    if (errno == Native.Linux.ENOMEDIUM)
                        return false;
                        
                    throw new NativeMethodException("Open");
                }

                try
                {
                    int result = Native.Linux.Ioctl(handle, Native.Linux.CDROM_DRIVE_STATUS);

                    if (result < 0)
                        throw new NativeMethodException("Ioctl");

                    return (result == 4);
                }
                finally
                {
                    if (handle >= 0)
                        Native.Linux.Close(handle);
                }
            }
            

            private static string GetMountFormat(string driveName)
            {
                /*
                    See GetMountPoint for info about the /proc/self/mountinfo file
                */

                string drivePath = PathUtils.LinuxGetDrivePath(driveName);

                string mountLine = File.ReadAllLines("/proc/self/mountinfo").FirstOrDefault(x => x.Split(" - ")[1].Trim().Split(' ')[1].ToLower() == drivePath);

                if (String.IsNullOrWhiteSpace(mountLine))
                    return null;

                string format = mountLine.Split(" - ")[1].Split(' ')[0].Trim();

                if (String.IsNullOrWhiteSpace(format))
                    return null;

                return format;
            }

            private static string GetMountPoint(string driveName)
            {
                /*
                    36 35 98:0 /mnt1 /mnt2 rw,noatime master:1 - ext3 /dev/root rw,errors=continue
                    (1)(2)(3)   (4)   (5)      (6)      (7)   (8) (9)   (10)         (11)

                    561 32 11:0 / /media/flip/archive\0400001 ro,nosuid,nodev,relatime shared:89 - udf /dev/sr0 ro,uid=1000,gid=1000,iocharset=utf8
                    (1) (2) (3) (4)          (5)                        (6)              (7)    (8)(9)   (10)                  (11)

                    (1) mount ID:  unique identifier of the mount (may be reused after umount)
                    (2) parent ID:  ID of parent (or of self for the top of the mount tree)
                    (3) major:minor:  value of st_dev for files on filesystem
                    (4) root:  root of the mount within the filesystem
                    (5) mount point:  mount point relative to the process's root
                    (6) mount options:  per mount options
                    (7) optional fields:  zero or more fields of the form "tag[:value]"
                    (8) separator:  marks the end of the optional fields
                    (9) filesystem type:  name of filesystem of the form "type[.subtype]"
                    (10) mount source:  filesystem specific information or "none"
                    (11) super options:  per super block options
                */

                string drivePath = PathUtils.LinuxGetDrivePath(driveName);

                string mountLine = File.ReadAllLines("/proc/self/mountinfo").FirstOrDefault(x => x.Split(" - ")[1].Trim().Split(' ')[1].ToLower() == drivePath);

                if (String.IsNullOrWhiteSpace(mountLine))
                    return null;

                string mountPoint = mountLine.Split(' ')[4].Trim().Replace(@"\040", " ");

                if (String.IsNullOrWhiteSpace(mountPoint))
                    return null;
                    
                return mountPoint;
            }

            internal static string GenerateDiscMD5(string driveName)
            {
                string drivePath = PathUtils.LinuxGetDrivePath(driveName);
                string md5Hash = String.Empty;

                int handle = Native.Linux.Open(drivePath, Native.Linux.OpenType.ReadOnly);

                if (handle < 0)
                    throw new NativeMethodException("Open");

                try 
                {
                    ulong discTotalBytes = DiskUtils.LinuxGetDiskSize(handle);
                    
                    byte[] buffer = new byte[256 * 1024]; // 256KB buffer

                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        //% slax bootloader, known good MD5: 3c78799690d95bd975e352020fc2acb8

                        // Stopwatch sw = Stopwatch.StartNew();
                        int currentBlockSize = 0;

                        // long lastSample = sw.ElapsedMilliseconds;
                        ulong totalBytesRead = 0;
                        int md5Offset = 0;
                        double currentPercent = 0.0;

                        while ((currentBlockSize = Native.Linux.Read(handle, buffer, buffer.Length)) > 0)
                        {
                            totalBytesRead += (ulong)currentBlockSize;

                            currentPercent = ((double)totalBytesRead / (double)discTotalBytes) * 100.0;
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
                    Native.Linux.Close(handle);
                }

                return md5Hash;
            }

            internal static string[] GetOpticalDriveNames()
            {
                if (!File.Exists("/proc/sys/dev/cdrom/info"))
                    return new string[] { };

                string[] lines = File.ReadAllLines("/proc/sys/dev/cdrom/info");

                string driveNameLine = lines.FirstOrDefault(x => x.ToLower().StartsWith("drive name"));
                string stringValue = driveNameLine.Substring(driveNameLine.IndexOf(':')+1).Trim('\t').Trim();
                string[] driveNames = stringValue.Split('\t').Where(x => !String.IsNullOrWhiteSpace(x)).OrderBy(x => x).ToArray();

                return driveNames;
            }

            // private static TOut GetOpticalDriveCapability<TOut>(string driveName, string lineName)
            // {

            // }

            // private static TOut GetOpticalDriveCapability<TOut>(int driveIndex, string lineName)
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
        }
    }
}
