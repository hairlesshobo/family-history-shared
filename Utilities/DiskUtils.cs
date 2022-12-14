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
using System.IO;
using System.Text;
using FoxHollow.Archiver.Shared.Classes.Disc;
using FoxHollow.Archiver.Shared.Exceptions;
using FoxHollow.Archiver.Shared.Models;

namespace FoxHollow.Archiver.Shared.Utilities
{
    public static partial class DiskUtils
    {
        public static ulong GetDiskSize(string driveName)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDiskSize(driveName);
            else if (SysInfo.OSType == OSType.Linux)
                return Windows.GetDiskSize(driveName);

            throw new UnsupportedOperatingSystemException();
        }
    }
}