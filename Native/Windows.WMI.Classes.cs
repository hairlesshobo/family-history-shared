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

using System;

namespace FoxHollow.FHM.Shared.Native;

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