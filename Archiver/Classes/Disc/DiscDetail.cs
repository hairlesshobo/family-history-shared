using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Utilities.Shared;
using Newtonsoft.Json;

namespace Archiver.Classes.Disc
{
    public class DiscVerificationResult
    {
        public DateTime VerificationDTM { get; set; }
        public bool DiscValid { get; set; }
    }
    public class DiscDetail : DiscSummary
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
                return DiscGlobals._discStagingDir + $"/iso/{this.DiscName}.iso";
            }
        }
        [JsonIgnore]
        public string RootStagingPath { 
            get
            {
                return DiscGlobals._discStagingDir + $"/stage/disc {this.DiscNumber.ToString("0000")}";
            }
        }

        public int FilesCopied 
        { 
            get
            {
                return this.Files.Where(x => x.Copied == true).Count();
            }
        }
        [JsonIgnore]
        public int DaysSinceLastVerify {
            get
            {
                if (this.Verifications.Count() <= 0)
                    return int.MaxValue;
                else
                    return (DateTime.UtcNow - this.Verifications.Max(x => x.VerificationDTM)).Days;
            }
        }

        [JsonIgnore]
        public bool LastVerifySuccess {
            get
            {
                if (this.Verifications.Count() == 0)
                    return false;
                else
                {
                    DiscVerificationResult lastResult = this.Verifications.OrderBy(x => x.VerificationDTM).LastOrDefault();

                    if (lastResult == null)
                        return false;
                    
                    return lastResult.DiscValid;
                }
            }
        }
        
        public bool Finalized { get; set; } = false;
        public List<DiscVerificationResult> Verifications { get; set; }
        

        public DiscDetail() : base()
        {
            this.Verifications = new List<DiscVerificationResult>();
            this.NewDisc = false;
        }

        public DiscDetail(int discNumber) : base(discNumber)
        {
            this.Verifications = new List<DiscVerificationResult>();
        }

        public DiscSummary GetDiscInfo()
        {
            return new DiscSummary()
            {
                ArchiveDTM = this.ArchiveDTM,
                DiscNumber = this.DiscNumber,
                Files = this.Files
            };
        }

        public void RecordVerification(DateTime verifyDtm, bool discValid)
        {
            if (this.Verifications == null)
                this.Verifications = new List<DiscVerificationResult>();

            this.Verifications.Add(new DiscVerificationResult()
            {
                VerificationDTM = verifyDtm,
                DiscValid = discValid
            });
        }

        public void SaveToJson()
            => Helpers.SaveDestinationDisc(this);
    }
}