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

namespace FoxHollow.FHM.Shared.Utilities;

public static partial class PathUtils
{
    public static class Windows
    {
        /// <summary>
        ///     Converts the provided drive name (ex: "A:") to the raw path (ex: "\\.\A:")
        /// </summary>
        /// <param name="driveName">Name of the drive</param>
        /// <returns>Raw formatted drive path</returns>
        public static string GetDrivePath(string driveName)
        {
            if (driveName.StartsWith(@"\\.\"))
                return driveName;

            driveName = CleanDriveName(driveName);

            return @"\\.\" + driveName;
        }

        public static string CleanDriveName(string driveName)
        {
            driveName = driveName.Trim('/');
            driveName = driveName.Trim('\\');
            driveName = driveName.ToUpper();

            if (!driveName.EndsWith(":"))
                driveName += ":";

            return driveName;
        }
    }
}