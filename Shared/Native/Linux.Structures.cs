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

               /* Since Linux 2.6, the kernel supports nanosecond
                  precision for the following timestamp fields.
                  For the details before Linux 2.6, see NOTES. */

            //    struct timespec st_atim;  /* Time of last access */
            //    struct timespec st_mtim;  /* Time of last modification */
            //    struct timespec st_ctim;  /* Time of last status change */

        //    #define st_atime st_atim.tv_sec      /* Backward compatibility */
        //    #define st_mtime st_mtim.tv_sec
        //    #define st_ctime st_ctim.tv_sec
           };
    }
}