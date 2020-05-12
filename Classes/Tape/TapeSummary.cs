using System;

namespace Archiver.Classes.Tape
{
    public class TapeSummary
    {
        public string Name { get; set; }
        public uint BlockingFactor { get; set; }
        public DateTime LastWriteDTM { get; set; }
        public ulong DataSizeBytes { get; set; }
        public uint FileCount { get; set; }
        public uint DirectoryCount { get; set; }
        public ulong TotalArchiveBytes { get; set; }
    }
}