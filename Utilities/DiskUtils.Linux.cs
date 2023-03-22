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

using FoxHollow.FHM.Shared.Exceptions;

namespace FoxHollow.FHM.Shared.Utilities;

public static partial class DiskUtils
{
    public static class Linux
    {
        public static ulong GetDiskSize(string driveName)
        {
            string drivePath = PathUtils.GetDrivePath(driveName);

            int handle = Native.Linux.Open(drivePath, Native.Linux.OpenType.ReadOnly); ;

            try
            {
                return GetDiskSize(handle);
            }
            finally
            {
                if (handle > 0)
                    Native.Linux.Close(handle);
            }
        }

        public static ulong GetDiskSize(int handle)
        {
            ulong returnValue64 = 0;

            if (Native.Linux.Ioctl(handle, Native.Linux.BLKGETSIZE64, out returnValue64) < 0)
                throw new NativeMethodException("Ioctl");

            return returnValue64;
        }

        public static long GetFileSize(string devicePath)
        {
            Native.Linux.StatResult result = new Native.Linux.StatResult();
            Native.Linux.Stat(devicePath, ref result);

            return result.Size;
        }
    }
}