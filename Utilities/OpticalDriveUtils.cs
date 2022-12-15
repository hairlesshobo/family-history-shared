/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Utilities
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
