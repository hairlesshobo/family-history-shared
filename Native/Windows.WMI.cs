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
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using FoxHollow.Archiver.Shared.Exceptions;

namespace FoxHollow.Archiver.Shared.Native
{
    public static partial class Windows
    {
        public static partial class WMI
        {
            [SupportedOSPlatform("windows")]
            public static Win32_CDROMDrive GetCdromDetail(string driveLetter)
            {
                if (SysInfo.OSType != Models.OSType.Windows)
                    throw new UnsupportedOperatingSystemException();

                driveLetter = driveLetter.Trim('/');
                driveLetter = driveLetter.Trim('\\');
                
                if (!driveLetter.EndsWith(':'))
                    driveLetter += ':';

                driveLetter = driveLetter.ToUpper();

                //var query = new WqlObjectQuery($"SELECT Id,SCSILogicalUnit,Size,Name,MediaLoaded,VolumeName FROM Win32_CDROMDrive WHERE Id='A:'");
                // var query = new WqlObjectQuery($"SELECT SCSILogicalUnit FROM Win32_CDROMDrive WHERE Id='{DriveLetter}'");
                WqlObjectQuery query = new WqlObjectQuery($"SELECT * FROM Win32_CDROMDrive");

                using (var searcher = new ManagementObjectSearcher(query))
                {
                    var objects = searcher.Get().OfType<ManagementObject>();

                    Win32_CDROMDrive[] drives = objects
                        // .Select(o => o.Properties["SCSILogicalUnit"].Value.ToString())
                        .Select(o => new Win32_CDROMDrive()
                        {
                            Availability = (ushort?)o.Properties["Availability"]?.Value,
                            Capabilities = (ushort[])o.Properties["Capabilities"]?.Value,
                            CapabilityDescriptions = (string[])o.Properties["CapabilityDescriptions"]?.Value,
                            Caption = o.Properties["Caption"]?.Value?.ToString(),
                            CompressionMethod = o.Properties["CompressionMethod"]?.Value?.ToString(),
                            ConfigManagerErrorCode = (uint?)o.Properties["ConfigManagerErrorCode"]?.Value,
                            ConfigManagerUserConfig = (bool?)o.Properties["ConfigManagerUserConfig"]?.Value,
                            CreationClassName = o.Properties["CreationClassName"]?.Value?.ToString(),
                            DefaultBlockSize = (ulong?)o.Properties["DefaultBlockSize"]?.Value,
                            Description = o.Properties["Description"]?.Value?.ToString(),
                            DeviceID = o.Properties["DeviceID"]?.Value?.ToString(),
                            Drive = o.Properties["Drive"]?.Value?.ToString(),
                            DriveIntegrity = (bool?)o.Properties["DriveIntegrity"]?.Value,
                            ErrorCleared = (bool?)o.Properties["ErrorCleared"]?.Value,
                            ErrorDescription = o.Properties["ErrorDescription"]?.Value?.ToString(),
                            ErrorMethodology = o.Properties["ErrorMethodology"]?.Value?.ToString(),
                            FileSystemFlags = (ushort?)o.Properties["FileSystemFlags"]?.Value,
                            FileSystemFlagsEx = (uint?)o.Properties["FileSystemFlagsEx"]?.Value,
                            Id = o.Properties["Id"]?.Value?.ToString(),
                            InstallDate = (DateTime?)o.Properties["InstallDate"]?.Value,
                            LastErrorCode = (uint?)o.Properties["LastErrorCode"]?.Value,
                            Manufacturer = o.Properties["Manufacturer"]?.Value?.ToString(),
                            MaxBlockSize = (ulong?)o.Properties["MaxBlockSize"]?.Value,
                            MaximumComponentLength = (uint?)o.Properties["MaximumComponentLength"]?.Value,

                            MaxMediaSize = (ulong?)o.Properties["MaxMediaSize"]?.Value,
                            MediaLoaded = (bool?)o.Properties["MediaLoaded"]?.Value,
                            MediaType = o.Properties["MediaType"]?.Value?.ToString(),
                            MfrAssignedRevisionLevel = o.Properties["MfrAssignedRevisionLevel"]?.Value?.ToString(),
                            MinBlockSize = (ulong?)o.Properties["MinBlockSize"]?.Value,
                            Name = o.Properties["Name"]?.Value?.ToString(),
                            NeedsCleaning = (bool?)o.Properties["NeedsCleaning"]?.Value,
                            NumberOfMediaSupported = (uint?)o.Properties["NumberOfMediaSupported"]?.Value,
                            PNPDeviceID = o.Properties["PNPDeviceID"]?.Value?.ToString(),
                            PowerManagementCapabilities = (ushort[])o.Properties["PowerManagementCapabilities"]?.Value,
                            PowerManagementSupported = (bool?)o.Properties["PowerManagementSupported"]?.Value,
                            RevisionLevel = o.Properties["RevisionLevel"]?.Value?.ToString(),
                            SCSIBus = (uint?)o.Properties["SCSIBus"]?.Value,
                            SCSILogicalUnit = (ushort?)o.Properties["SCSILogicalUnit"]?.Value,
                            SCSIPort = (ushort?)o.Properties["SCSIPort"]?.Value,
                            SCSITargetId = (ushort?)o.Properties["SCSITargetId"]?.Value,
                            SerialNumber = o.Properties["SerialNumber"]?.Value?.ToString(),
                            Size = (ulong?)o.Properties["Size"]?.Value,
                            Status = o.Properties["Status"]?.Value?.ToString(),
                            StatusInfo = (ushort?)o.Properties["StatusInfo"]?.Value,
                            SystemCreationClassName = o.Properties["SystemCreationClassName"]?.Value?.ToString(),
                            SystemName = o.Properties["SystemName"]?.Value?.ToString(),
                            VolumeName = o.Properties["VolumeName"]?.Value?.ToString(),
                            VolumeSerialNumber = o.Properties["VolumeSerialNumber"]?.Value?.ToString(),
                        })
                        .ToArray();

                    return drives[0];

                    // if (resultStr != null)
                    //     return Int32.Parse(resultStr);
                }

                throw new DriveNotFoundException($"Could not find drive {driveLetter}");
            }
        }
    }
}