using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Archiver.Shared.Classes.Disc
{
    public class DiscScanStats
    {
        public IReadOnlyList<DiscSourceFile> NewFileEntries => (IReadOnlyList<DiscSourceFile>)SourceFileDict.Select(x => x.Value).Where(x => x.Copied == false);
        public int NewDiscCount => DestinationDiscs.Where(x => x.NewDisc == true).Count();

        public List<DiscDetail> DestinationDiscs { get; set; }
        public Dictionary<string, DiscSourceFile> SourceFileDict { get; set; }
        public Stopwatch ProcessSw { get; set; }
        public long NewlyFoundFiles { get; set; }
        public long RenamedFiles { get; set; }
        public long ExistingFilesArchived { get; set; }
        public long TotalSize { get; set; }
        public long ExcludedFileCount { get; set; }

        public DiscScanStats(List<DiscDetail> existingDiscs = null)
        {
            this.DestinationDiscs = new List<DiscDetail>();
            this.ProcessSw = new Stopwatch();
            this.NewlyFoundFiles = 0;
            this.RenamedFiles = 0;
            this.ExistingFilesArchived = 0;
            this.TotalSize = 0;
            this.ExcludedFileCount = 0;
            this.SourceFileDict = new Dictionary<string, DiscSourceFile>(StringComparer.OrdinalIgnoreCase);

            if (existingDiscs != null)
            {
                this.DestinationDiscs = existingDiscs;

                foreach (DiscDetail disc in this.DestinationDiscs)
                    disc.Files.ForEach(x => this.SourceFileDict.Add(x.RelativePath, x));
            }
        }
    }
}