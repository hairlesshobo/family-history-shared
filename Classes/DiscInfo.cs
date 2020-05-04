using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscArchiver.Classes
{
    public class DiscInfo
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
        public List<SourceFile> Files { get; set; }
        
        public DiscInfo()
        {
            this.Files = new List<SourceFile>();
        }

        public DiscInfo(int discNumber)
        {
            this.Files = new List<SourceFile>();
            this.DiscNumber = discNumber;
        }
    }
}