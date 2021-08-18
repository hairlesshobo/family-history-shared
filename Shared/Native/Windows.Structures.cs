using System;
using System.Runtime.InteropServices;
using Archiver.Shared.TapeDrivers;
using Microsoft.Win32.SafeHandles;

namespace Archiver.Shared.Native
{
    public static partial class Windows
    {
        public enum TapeDriveStatus
        {
            Ready = 0,
            ErrorEndOfMedia = 1100,
            ErrorMediaChanged = 1110,
            ErrorNoMediaInDrive = 1112,
            ErrorWriteProtect = 19,
        }

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

        [StructLayout( LayoutKind.Sequential )]
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

        [StructLayout( LayoutKind.Sequential )]
        public class TapeSetDriveParameters
        {
            public byte ECC;
            public byte Compression;
            public byte DataPadding;
            public byte ReportSetMarks;

            public uint EOTWarningZoneSize;
        }
    }
}