using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DiscArchiver.Classes
{
    public class DestinationDisc 
    {
        public int DiscNumber { get; set; }
        public long BytesCopied { get; set; }
        public DateTime ArchiveDTM { get; set; }
        public string Hash { get; set; }
        [JsonIgnore]
        public bool NewDisc { get; set; } = true;
        [JsonIgnore]
        public bool IsoCreated { get; set; } = false;
        public string DiscName { 
            get
            {
                return $"archive {this.DiscNumber.ToString("0000")}";
            }
        }
        [JsonIgnore]
        public string IsoPath { 
            get
            {
                return Globals._stagingDir + $"/iso/{this.DiscName}.iso";
            }
        }
        [JsonIgnore]
        public string RootStagingPath { 
            get
            {
                return Globals._stagingDir + $"/stage/disc {this.DiscNumber.ToString("0000")}";
            }
        }
        public int FilesCopied 
        { 
            get
            {
                return this.Files.Where(x => x.Copied == true).Count();
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
        public bool Finalized { get; set; } = false;
        public List<SourceFile> Files { get; set; }

        public DestinationDisc()
        {
            this.NewDisc = false;
            this.Files = new List<SourceFile>();
        }

        public DestinationDisc(int discNumber)
        {
            this.Files = new List<SourceFile>();
            this.DiscNumber = discNumber;
        }
    }
}