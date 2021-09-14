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
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Tape;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class TapeUtilsNew
    {
        public static string CleanTapeDrivePath(string path)
        {
            // Clean up windows path name
            if (SysInfo.OSType == OSType.Windows)
                path = path.Replace('/', '\\');

            // provide mapping from tapeX to OS specific tape path or from "auto" to first tape of the current platform
            if (path.ToLower() == "auto" || Regex.Match(path, @"tape\d+", RegexOptions.IgnoreCase).Success)
            {
                string tapeNum = "0";
                
                if (path.ToLower() != "auto")
                    tapeNum = path.Substring(4);

                if (SysInfo.OSType == OSType.Windows)
                    path = @$"\\.\Tape{tapeNum}";
                else if (SysInfo.OSType == OSType.Linux)
                    path = $"/dev/nst{tapeNum}";
                else
                    throw new UnsupportedOperatingSystemException();
            }

            return path;
        }

        public static byte[] GetStringPaddedBytes(string Input, uint BlockSize)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(Input);
            int lengthNeeded = (int)HelpersNew.RoundToNextMultiple(rawData.Length, (int)BlockSize);
            Array.Resize(ref rawData, lengthNeeded);

            return rawData;
        }

        // TODO: Find a way to split this that works for archiver and tape server
        public static bool IsTapeDrivePresent()
        {
            if (SysInfo.Config.Tape.Driver.ToLower() != "auto-remote")
            {
                if (SysInfo.OSType == OSType.Windows)
                    return WindowsIsTapeDrivePresent();
                else if (SysInfo.OSType == OSType.Linux)
                    return LinuxIsTapeDrivePresent();
            }

            return false;
        }
    }
}