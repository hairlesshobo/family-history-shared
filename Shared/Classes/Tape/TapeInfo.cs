namespace Archiver.Shared.Classes.Tape
{
    public class TapeInfo
    {
        public TapeMediaInfo MediaInfo { get; set; }
        public TapeDriveInfo DriveInfo { get; set; }
        public long TapePosition { get; set; }
    }
}