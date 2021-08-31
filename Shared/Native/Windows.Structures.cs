using System;
using System.Runtime.InteropServices;
using Archiver.Shared.TapeDrivers;
using Microsoft.Win32.SafeHandles;

namespace Archiver.Shared.Native
{
    public static partial class Windows
    {

        // TODO: convert these to shared classes that will work for any driver
        [StructLayout(LayoutKind.Sequential)]
        public struct TapeMediaInfo
        {
            public long Capacity;
            public long Remaining;

            public uint BlockSize;
            public uint PartitionCount;

            public byte IsWriteProtected;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TapeDriveInfo
        {
            public byte ECC;
            public byte Compression;
            public byte DataPadding;
            public byte ReportSetMarks;

            public uint DefaultBlockSize;
            public uint MaximumBlockSize;
            public uint MinimumBlockSize;
            public uint PartitionCount;

            public uint FeaturesLow;
            public uint FeaturesHigh;
            public uint EOTWarningZone;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TapeSetDriveParameters
        {
            public byte ECC;
            public byte Compression;
            public byte DataPadding;
            public byte ReportSetMarks;

            public uint EOTWarningZoneSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STORAGE_READ_CAPACITY
        {
            public uint Version;
            public uint Size;
            public uint BlockLength;
            public ulong NumberOfBlocks;
            public ulong DiskLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY 
        {
            public long Cylinders;
            public MEDIA_TYPE MediaType;
            public uint TracksPerCylinder;
            public uint SectorsPerTrack;
            public uint BytesPerSector;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY_EX {
            public DISK_GEOMETRY Geometry;
            public uint DiskSize;
            public byte Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GET_LENGTH_INFORMATION 
        {
           public long Length;
        }
    }
}