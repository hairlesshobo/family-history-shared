namespace Archiver.Shared.TapeDrivers
{
    public partial class NativeWindowsTapeDriver
    {
        #region Private constants
        private const short FILE_ATTRIBUTE_NORMAL = 0x80;
        private const short INVALID_HANDLE_VALUE = -1;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        private const uint NO_ERROR = 0;
        private const int TAPE_LOAD = 0;
        private const int TAPE_UNLOAD = 1;

        private const int TAPE_SETMARKS = 0;
        private const int TAPE_FILEMARKS = 1;
        private const int TAPE_SHORT_FILEMARKS = 2;
        private const int TAPE_LONG_FILEMARKS = 3;



        private const int TAPE_REWIND = 0;
        private const int TAPE_LOGICAL_POSITION = 1;
        private const int TAPE_LOGICAL_BLOCK = 2;
        private const int TAPE_SPACE_END_OF_DATA = 4;
        private const int TAPE_RELATIVE_BLOCKS = 5;
        private const int TAPE_SPACE_FILEMARKS = 6;
        private const int TAPE_SPACE_SEQUENTIAL_FMKS = 7;
        private const int TAPE_SPACE_SETMARKS = 8;
        private const int TAPE_SPACE_SEQUENTIAL_SMKS = 9;

        private const int FALSE = 0;
        private const int TRUE = 0;

        private const int SET_TAPE_MEDIA_INFORMATION = 0;
        private const int SET_TAPE_DRIVE_INFORMATION = 1;

        private const int MEDIA_PARAMS = 0;
        private const int DRIVE_PARAMS = 1;
        #endregion
    }
}