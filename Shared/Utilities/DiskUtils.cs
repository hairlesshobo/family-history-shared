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
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
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

        public static void SaveDestinationDisc(DiscDetail disc, string destinationDir = null, string fileName = null)
        {
            if (destinationDir == null)
                destinationDir = SysInfo.Directories.JSON;

            if (fileName == null)
                fileName = $"disc_{disc.DiscNumber.ToString("0000")}.json";

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            string jsonFilePath = destinationDir + "/" + fileName;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(disc, new Newtonsoft.Json.JsonSerializerSettings() {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });

            // Write the json data needed for future runs of this app
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }
    }
}