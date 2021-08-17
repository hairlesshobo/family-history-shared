using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Archiver.Shared.Classes.Tape
{
    public class TapeVerificationResult
    {
        public DateTime VerificationDTM { get; set; }
        public bool TapeValid { get; set; }
    }

    public class TapeDetail : TapeSummary
    {
        public List<TapeVerificationResult> Verifications { get; set; }
        public string Hash { get; set; } = null;
        public double CompressionRatio
        {
            get
            {
                if (this.ArchiveBytesOnTape == 0)
                    return 0;
                else
                    return ((double)this.TotalArchiveBytes / (double)this.ArchiveBytesOnTape);

            }
        }
        public TapeSourceInfo SourceInfo { get; set; }
        public long ArchiveBytesOnTape { get; set; }
        public int FilesCopied 
        { 
            get
            {
                return this.FlattenFiles().Where(x => x.Copied == true).Count();
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
                    TapeVerificationResult lastResult = this.Verifications.OrderBy(x => x.VerificationDTM).LastOrDefault();

                    if (lastResult == null)
                        return false;
                    
                    return lastResult.TapeValid;
                }
            }
        }

        public TapeDetail() : base()
        {
            this.Files = new List<TapeSourceFile>();
            this.Verifications = new List<TapeVerificationResult>();
        }

        public TapeSummary GetSummary()
        {
            return (TapeSummary)this;
        }
    }
}