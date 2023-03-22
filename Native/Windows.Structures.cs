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
    public struct DISK_GEOMETRY_EX
    {
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