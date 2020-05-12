using System;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Classes.Disc
{
    public class DiscSummary
    {
        public int DiscNumber { get; set; }
        public DateTime ArchiveDTM { get; set; }
        public string DiscName { 
            get
            {
                return $"archive {this.DiscNumber.ToString("0000")}";
            }
        }
        public int TotalFiles 
        {
            get
            {
                return this.Files.Count();
            }
        }
        public long DataSize 
        { 
            get
            {
                return this.Files.Sum(x => x.Size);
            } 
        }
        public List<DiscSourceFile> Files { get; set; }
        
        public DiscSummary()
        {
            this.Files = new List<DiscSourceFile>();
        }

        public DiscSummary(int discNumber)
        {
            this.Files = new List<DiscSourceFile>();
            this.DiscNumber = discNumber;
        }
    }
}