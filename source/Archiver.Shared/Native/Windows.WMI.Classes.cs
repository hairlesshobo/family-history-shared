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
using System.Management;

namespace FoxHollow.Archiver.Shared.Native
{
    public static partial class Windows
    {
        public static partial class WMI
        {
            public class Win32_CDROMDrive
            {
                public ushort? Availability;
                public ushort[] Capabilities;
                public string[] CapabilityDescriptions;
                public string Caption;
                public string CompressionMethod;
                public uint? ConfigManagerErrorCode;
                public bool? ConfigManagerUserConfig;
                public string CreationClassName;
                public ulong? DefaultBlockSize;
                public string Description;
                public string DeviceID;
                public string Drive;
                public bool? DriveIntegrity;
                public bool? ErrorCleared;
                public string ErrorDescription;
                public string ErrorMethodology;
                public ushort? FileSystemFlags;
                public uint? FileSystemFlagsEx;
                public string Id;
                public DateTime? InstallDate;
                public uint? LastErrorCode;
                public string Manufacturer;
                public ulong? MaxBlockSize;
                public uint? MaximumComponentLength;
                public ulong? MaxMediaSize;
                public bool? MediaLoaded;
                public string MediaType;
                public string MfrAssignedRevisionLevel;
                public ulong? MinBlockSize;
                public string Name;
                public bool? NeedsCleaning;
                public uint? NumberOfMediaSupported;
                public string PNPDeviceID;
                public ushort[] PowerManagementCapabilities;
                public bool? PowerManagementSupported;
                public string RevisionLevel;
                public uint? SCSIBus;
                public ushort? SCSILogicalUnit;
                public ushort? SCSIPort;
                public ushort? SCSITargetId;
                public string SerialNumber;
                public ulong? Size;
                public string Status;
                public ushort? StatusInfo;
                public string SystemCreationClassName;
                public string SystemName;
                // wtf is real64 ??
                // real64 TransferRate;
                public string VolumeName;
                public string VolumeSerialNumber;
            }
        }
    }
}