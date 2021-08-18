using Archiver.Shared.Native;
using Archiver.Shared.TapeDrivers;

namespace Archiver.Shared.Classes.Tape
{
    public class TapeInfo
    {
        public Windows.TapeMediaInfo MediaInfo { get; set; }
        public Windows.TapeDriveInfo DriveInfo { get; set; }
        public long TapePosition { get; set; }
    }
}