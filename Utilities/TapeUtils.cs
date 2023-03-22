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
using System.Text;
using System.Text.RegularExpressions;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Utilities
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
            // if (SysInfo.Config.Tape.Driver.ToLower() != "auto-remote")
            // {
                if (SysInfo.OSType == OSType.Windows)
                    return WindowsIsTapeDrivePresent();
                else if (SysInfo.OSType == OSType.Linux)
                    return LinuxIsTapeDrivePresent();
            // }

            return false;
        }
    }
}