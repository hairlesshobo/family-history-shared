using System;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Shared.Classes.Disc
{
    public class DiscScanStats
    {
        public IReadOnlyList<DiscSourceFile> NewFileEntries => (IReadOnlyList<DiscSourceFile>)DiscSourceFiles.Where(x => x.Copied == false);
        public List<DiscSourceFile> DiscSourceFiles = new List<DiscSourceFile>();
        public List<DiscDetail> DestinationDiscs = new List<DiscDetail>();
        public int NewDiscCount => DestinationDiscs.Where(x => x.NewDisc == true).Count();


        public long NewlyFoundFiles { get; set; } = 0;
        public long RenamedFiles { get; set; } = 0;
        public long ExistingFilesArchived { get; set; } = 0;
        public long TotalSize { get; set; } = 0;
        public long ExcludedFileCount { get; set; } = 0;

        public DiscScanStats(List<DiscDetail> existingDiscs = null)
        {
            if (existingDiscs != null)
            {
                this.DestinationDiscs = existingDiscs;

                foreach (DiscDetail disc in this.DestinationDiscs)
                    disc.Files.ForEach(x => this.DiscSourceFiles.Add(x));
            }
        }
    }
}