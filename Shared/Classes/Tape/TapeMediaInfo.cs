using System;
using System.Runtime.InteropServices;

namespace Archiver.Shared.Classes.Tape
{
    // TODO: 
    // convert these to shared classes that will work for any driver, then
    // move the structures back to the windows driver
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
}