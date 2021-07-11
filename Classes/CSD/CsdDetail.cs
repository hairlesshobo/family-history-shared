using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;
using Newtonsoft.Json;

namespace Archiver.Classes.CSD
{
    public class CsdVerificationResult
    {
        public DateTime VerificationDTM { get; set; }
        public bool CsdValid { get; set; }
    }
    public class CsdDetail : CsdSummary
    {
        public long BytesCopied { get; set; }
        [JsonIgnore]
        public List<CsdSourceFile> PendingWrites { get; set; }
        [JsonIgnore]
        public bool HasPendingWrites { get; set; } = false;
        public string Hash { get; set; }
        [JsonIgnore]
        public bool NewCsd { get; set; } = true;
        [JsonIgnore]
        public bool IsoCreated { get; set; } = false;
        
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
                    CsdVerificationResult lastResult = this.Verifications.OrderBy(x => x.VerificationDTM).LastOrDefault();

                    if (lastResult == null)
                        return false;
                    
                    return lastResult.CsdValid;
                }
            }
        }
        
        public List<CsdVerificationResult> Verifications { get; set; }
        

        public CsdDetail() : base()
        {
            this.PendingWrites = new List<CsdSourceFile>();
            this.Verifications = new List<CsdVerificationResult>();
            this.NewCsd = false;
        }

        public CsdDetail(int csdNumber, int blockSize, long totalSpace) : base()
        {
            this.PendingWrites = new List<CsdSourceFile>();
            this.Verifications = new List<CsdVerificationResult>();
            this.BlockSize = blockSize;
            this.TotalSpace = totalSpace;
            this.RegisterDtmUtc = DateTime.UtcNow;
            this.CsdNumber = csdNumber;

            CsdGlobals._destinationCsds.Add(this);
        }

        public CsdSummary GetCsdSummary()
        {
            return new CsdSummary()
            {
                RegisterDtmUtc = this.RegisterDtmUtc,
                CsdNumber = this.CsdNumber,
                Files = this.Files,
                TotalSpace = this.TotalSpace,
                BlockSize = this.BlockSize,
                WriteDtmUtc = this.WriteDtmUtc
            };
        }

        public void RecordVerification(DateTime verifyDtm, bool csdValid)
        {
            if (this.Verifications == null)
                this.Verifications = new List<CsdVerificationResult>();

            this.Verifications.Add(new CsdVerificationResult()
            {
                VerificationDTM = verifyDtm,
                CsdValid = csdValid
            });
        }

        public void SaveToJson()
            => CsdUtils.SaveDetailToIndex(this);
    }
}