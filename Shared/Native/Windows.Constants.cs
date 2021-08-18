using System;

namespace Archiver.Shared.Native
{
    public static partial class Windows
    {
        public const short FILE_ATTRIBUTE_NORMAL = 0x80;
        public const short INVALID_HANDLE_VALUE = -1;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

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
    }
}