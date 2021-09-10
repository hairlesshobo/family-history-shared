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
using System.Runtime.InteropServices;

namespace Archiver.Shared.Native
{
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
            long tv_sec;		/* Seconds.  */
            long tv_nsec;	/* Nanoseconds.  */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MagneticTapeOperation
        {
            /// <Summary>
            /// Operation to be performed
            /// </Summary>
            public short Operation;	/* operations defined below */

            /// <Summary>
            /// How many times to perform this operation
            /// </Summary>
	        public int Count;	        /* how many of them */
        }
    }
}