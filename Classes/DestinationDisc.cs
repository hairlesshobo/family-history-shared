using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DiscArchiver.Classes
{
    public class DestinationDisc : DiscInfo
    {
        public long BytesCopied { get; set; }
        public string Hash { get; set; }
        [JsonIgnore]
        public bool NewDisc { get; set; } = true;
        [JsonIgnore]
        public bool IsoCreated { get; set; } = false;
        
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
        
        public bool Finalized { get; set; } = false;
        

        public DestinationDisc() : base()
        {
            this.NewDisc = false;
        }

        public DestinationDisc(int discNumber) : base(discNumber)
        { }

        public DiscInfo GetDiscInfo()
        {
            return new DiscInfo()
            {
                ArchiveDTM = this.ArchiveDTM,
                DiscNumber = this.DiscNumber,
                Files = this.Files
            };
        }
    }
}