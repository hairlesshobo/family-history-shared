using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Archiver.Classes.Tape
{
    public class TapeVerificationResult
    {
        public DateTime VerificationDTM { get; set; }
        public bool TapeValid { get; set; }
    }

    public class TapeDetail : TapeSummary
    {
        public long BytesCopied { get; set; } = 0;
        public List<TapeVerificationResult> Verifications { get; set; }
        public string Hash { get; set; } = null;
        public double CompressionRatio { get; set; } = 0.0;
        public TapeSourceInfo SourceInfo { get; set; }
        public long ExcludedFiles { get; set; }
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