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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Utilities;

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