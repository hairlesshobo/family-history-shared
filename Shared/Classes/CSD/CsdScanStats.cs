using System;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Shared.Classes.CSD
{
    public class CsdScanStats
    {

        public List<CsdSourceFile> JsonReadSourceFiles = new List<CsdSourceFile>();
        public IReadOnlyList<CsdSourceFile> NewFileEntries => (IReadOnlyList<CsdSourceFile>)SourceFileDict.Select(x => x.Value).Where(x => x.Copied == false);
        
        public List<CsdSourceFile> DeletedFiles = new List<CsdSourceFile>();
        public List<CsdDetail> DestinationCsds = new List<CsdDetail>();
        public Dictionary<string, CsdSourceFile> SourceFileDict = new Dictionary<string, CsdSourceFile>(StringComparer.OrdinalIgnoreCase);


        public long NewFileCount { get; set; } = 0;
        public long DeletedFileCount { get; set; } = 0;
        public long ModifiedFileCount { get; set; } = 0;
        public long RenamedFileCount { get; set; } = 0;
        public long ExistingFileCount { get; set; } = 0;
        public long ExcludedFileCount { get; set; } = 0;
        public long TotalSizePending { get; set; } = 0;


        public CsdScanStats(List<CsdDetail> existingCsds = null)
        {
            if (existingCsds != null)
            {
                this.DestinationCsds = existingCsds;

                foreach (CsdDetail csd in this.DestinationCsds)
                {
                    foreach (CsdSourceFile file in csd.Files)
                    {
                        this.SourceFileDict.Add(file.RelativePath, file);
                    }
                }
            }
        }
    }
}