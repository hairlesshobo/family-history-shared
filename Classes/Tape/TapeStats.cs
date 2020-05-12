using System;

namespace Archiver.Classes.Tape
{
    public class TapeStats
    {
        public long FileCount { get; set; } = 0;
        public long DirectoryCount { get; set; } = 0;
        public long TotalSize { get; set; } = 0;
        public long TotalArchiveSize { get; set; } = 0;
        public long ExcludedFileCount { get; set; } = 0;
    }
}