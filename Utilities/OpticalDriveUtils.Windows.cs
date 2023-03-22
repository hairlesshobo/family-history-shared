//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;
using Microsoft.Win32.SafeHandles;

namespace FoxHollow.FHM.Shared.Utilities;

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
            using (SafeFileHandle fileHandle = Native.Windows.CreateFile(
                    drive.FullPath,
                    Native.Windows.GENERIC_READ,
                    Native.Windows.FILE_SHARE_READ | Native.Windows.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    Native.Windows.OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                ))
            {

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
        }
    }
}