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