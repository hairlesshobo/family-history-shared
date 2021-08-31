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
        public struct RAW_READ_INFO
        {
            public long DiskOffset;
            public uint SectorCount;
            public TRACK_MODE_TYPE TrackMode;
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
        public struct SCSI_PASS_THROUGH_DIRECT 
        {
            public ushort Length;
            public byte ScsiStatus;
            public byte PathId;
            public byte TargetId;
            public byte Lun;
            public byte CdbLength;
            public byte SenseInfoLength;
            public byte DataIn;
            public uint DataTransferLength;
            public uint TimeOutValue;
            public IntPtr DataBuffer;
            public uint SenseInfoOffset;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Cdb16;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct sptd_with_sense
        {
            public SCSI_PASS_THROUGH_DIRECT s;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Native.Windows.SPTD_SENSE_SIZE)]
            public byte[] sense;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GET_LENGTH_INFORMATION 
        {
           public long Length;
        }
    }
}