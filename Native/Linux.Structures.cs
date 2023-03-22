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

using System.Runtime.InteropServices;

namespace FoxHollow.FHM.Shared.Native;

public static partial class Linux
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StatResult
    {
        public ulong DeviceID;         /* ID of device containing file */
        public ulong Inode;         /* Inode number */
        public ulong HardLinkCount;       /* Number of hard links */
        public uint Mode;        /* File type and mode */
        public uint UID;         /* User ID of owner */
        public uint GID;         /* Group ID of owner */
        public int __pad0;
        public ulong SpecialDeviceID;        /* Device ID (if special file) */
        public long Size;        /* Total size, in bytes */
        public long BlockSize;     /* Block size for filesystem I/O */
        public long BlockCount;      /* Number of 512B blocks allocated */

        public LinuxTimeSpec TimeLastAccess;  /* Time of last access */
        public LinuxTimeSpec TimeLastModify;  /* Time of last modification */
        public LinuxTimeSpec TimeLastStatusChange;  /* Time of last status change */

        public long reserved1;
        public long reserved2;
        public long reserved3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LinuxTimeSpec
    {
        long tv_sec;        /* Seconds.  */
        long tv_nsec;   /* Nanoseconds.  */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MagneticTapeOperation
    {
        /// <Summary>
        /// Operation to be performed
        /// </Summary>
        public short Operation; /* operations defined below */

        /// <Summary>
        /// How many times to perform this operation
        /// </Summary>
        public int Count;           /* how many of them */
    }
}