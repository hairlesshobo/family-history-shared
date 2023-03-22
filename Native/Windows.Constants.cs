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

namespace FoxHollow.FHM.Shared.Native;

public static partial class Windows
{
    public const short FILE_READ_ATTRIBUTES = 0x0080;
    public const short INVALID_HANDLE_VALUE = -1;
    public const uint GENERIC_READ = 0x80000000;
    public const uint GENERIC_WRITE = 0x40000000;
    public const uint CREATE_NEW = 1;
    public const uint CREATE_ALWAYS = 2;
    public const uint OPEN_EXISTING = 3;
    public const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
    public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;


    public const uint FILE_SHARE_NONE = 0;
    public const uint FILE_SHARE_READ = 1;
    public const uint FILE_SHARE_WRITE = 2;
    public const uint FILE_SHARE_DELETE = 4;

    public const uint NO_ERROR = 0;
    public const int TAPE_LOAD = 0;
    public const int TAPE_UNLOAD = 1;

    public const int TAPE_SETMARKS = 0;
    public const int TAPE_FILEMARKS = 1;
    public const int TAPE_SHORT_FILEMARKS = 2;
    public const int TAPE_LONG_FILEMARKS = 3;



    public const int TAPE_REWIND = 0;
    public const int TAPE_LOGICAL_POSITION = 1;
    public const int TAPE_LOGICAL_BLOCK = 2;
    public const int TAPE_SPACE_END_OF_DATA = 4;
    public const int TAPE_RELATIVE_BLOCKS = 5;
    public const int TAPE_SPACE_FILEMARKS = 6;
    public const int TAPE_SPACE_SEQUENTIAL_FMKS = 7;
    public const int TAPE_SPACE_SETMARKS = 8;
    public const int TAPE_SPACE_SEQUENTIAL_SMKS = 9;

    public const int FALSE = 0;
    public const int TRUE = 0;

    public const int SET_TAPE_MEDIA_INFORMATION = 0;
    public const int SET_TAPE_DRIVE_INFORMATION = 1;

    public const int MEDIA_PARAMS = 0;
    public const int DRIVE_PARAMS = 1;


    public const uint IOCTL_STORAGE_READ_CAPACITY = 0x2d5140;
    public const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
    public const uint IOCTL_CDROM_RAW_READ = 0x2403e;
    public const uint IOCTL_CDROM_CURRENT_POSITION = 0x1;
    public const uint IOCTL_CDROM_GET_DRIVE_GEOMETRY_EX = 0x24050;
    public const uint IOCTL_SCSI_PASS_THROUGH_DIRECT = 0x4d014;
    public const uint IOCTL_DISK_GET_LENGTH_INFO = 0x7405c;
    public const uint FSCTL_ALLOW_EXTENDED_DASD_IO = 0x90083;
    public static readonly byte[] SCSI_RAW_READ = new byte[] { 0xBE, 0, 0, 0, 0, 1, 0, 0, 1, 0x10, 0, 0 };

    public const byte SCSI_IOCTL_DATA_OUT = 0;
    public const byte SCSI_IOCTL_DATA_IN = 1;
    public const byte SCSI_IOCTL_DATA_UNSPECIFIED = 2;

    public const int SPTD_SENSE_SIZE = 128;

    public const int ERROR_SECTOR_NOT_FOUND = 0x1B; // 27
}