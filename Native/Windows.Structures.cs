/*
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
using FoxHollow.FHM.Shared.TapeDrivers;
using Microsoft.Win32.SafeHandles;

namespace FoxHollow.FHM.Shared.Native
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