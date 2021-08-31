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
        private static class Windows
        {
            public static List<OpticalDrive> GetDrives()
            {
                return DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.CDRom).Select(x =>
                {
                    string driveName = PathUtils.Windows.CleanDriveName(x.Name);

                    return new OpticalDrive()
                    {
                        Name = driveName,
                        MountPoint = driveName,
                        FullPath = PathUtils.GetDrivePath(driveName),
                        IsReady = x.IsReady,
                        VolumeLabel = (x.IsReady ? x.VolumeLabel : null),
                        VolumeFormat = (x.IsReady ? x.DriveFormat : null),
                        IsDiscLoaded = x.IsReady
                    };
                }).ToList();
            }

            public static string GetDriveLabel(string driveName)
                => DriveInfo.GetDrives()
                            .FirstOrDefault(x => PathUtils.Windows.CleanDriveName(x.Name) == PathUtils.Windows.CleanDriveName(driveName))
                           ?.VolumeLabel;

            public static WindowsRawDiscStreamReader GetDriveRawStream(OpticalDrive drive)
                => new WindowsRawDiscStreamReader(drive);

            public static string[] GetOpticalDriveNames()
                => DriveInfo.GetDrives().Select(x => PathUtils.Windows.CleanDriveName(x.Name)).ToArray();

            public static void EjectDrive(OpticalDrive drive)
            {
                SafeFileHandle fileHandle = null;

                try
                {
                    // Create an handle to the drive
                    fileHandle = Native.Windows.CreateFile(
                        drive.FullPath,
                        Native.Windows.GENERIC_READ,
                        Native.Windows.FILE_SHARE_READ | Native.Windows.FILE_SHARE_WRITE, 
                        IntPtr.Zero,
                        Native.Windows.OPEN_EXISTING,
                        0,
                        IntPtr.Zero
                    );

                    if (fileHandle.IsInvalid)
                        throw new NativeMethodException("CreateFile");

                    uint returnedBytes = 0;

                    // Eject the disk
                    Native.Windows.DeviceIoControl(
                        fileHandle,
                        Native.Windows.IOCTL_STORAGE_EJECT_MEDIA,
                        IntPtr.Zero, 0,
                        IntPtr.Zero, 0,
                        ref returnedBytes,
                        IntPtr.Zero
                        );
                }
                finally
                {
                    // Close Drive Handle
                    if (fileHandle != null && !fileHandle.IsClosed)
                    {
                        Native.Windows.CloseHandle(fileHandle);
                        // fileHandle = null;
                    }
                }
            }
        }
    }
}
