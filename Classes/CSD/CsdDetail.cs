using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;
using Newtonsoft.Json;

namespace Archiver.Classes.CSD
{


    public class CsdDetail
    {
        public int CsdNumber { get; set; }
        public DateTime RegisterDtmUtc { get; set; }
        public List<DateTime> WriteDtmUtc { get; set; }
        public string CsdName { 
            get
            {
                return $"CSD{this.CsdNumber.ToString("000")}";
            }
        }
        public int TotalFiles 
        {
            get
            {
                return this.Files.Count();
            }
        }
        public int BlockSize { get; set; }
        public long TotalSpace { get; set; }
        public long FreeSpace 
        { 
            get
            {
                return this.TotalSpace - this.DataSizeOnDisc;
            }
        }
        
        public long DataSize 
        { 
            get
            {
                return this.Files.Sum(x => x.Size);
            } 
        }

        public long DataSizeOnDisc
        {
            get
            {
                return this.Files.Sum(x => Helpers.RoundToNextMultiple(x.Size, this.BlockSize));
            } 
        }
        public List<CsdSourceFile> Files { get; set; }
        
        public CsdDriveInfo DriveInfo { get; set; }
        
        public long BytesCopied { get; set; }
        
        [JsonIgnore]
        public bool DiskFull => (this.FreeSpace - Config.CsdReservedCapacity) <= this.BlockSize;

        [JsonIgnore]
        public bool HasPendingWrites => this.PendingFileCount > 0;

        [JsonIgnore]
        public IEnumerable<CsdSourceFile> PendingFiles => this.Files.Where(x => x.Copied == false);

        [JsonIgnore]
        public long PendingBytes => this.PendingFiles.Sum(x => x.Size);

        [JsonIgnore]
        public long PendingFileCount => this.PendingFiles.Count();
        
        public int FilesCopied => this.Files.Where(x => x.Copied == true).Count();

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
            this.DriveInfo = new CsdDriveInfo();
            this.WriteDtmUtc = new List<DateTime>();
            this.Files = new List<CsdSourceFile>();

            this.Verifications = new List<CsdVerificationResult>();
        }

        public CsdDetail(int csdNumber, int blockSize, long totalSpace) : base()
        {
            this.DriveInfo = new CsdDriveInfo();
            this.WriteDtmUtc = new List<DateTime>();
            this.Files = new List<CsdSourceFile>();

            this.Verifications = new List<CsdVerificationResult>();
            this.BlockSize = blockSize;
            this.TotalSpace = totalSpace;
            this.RegisterDtmUtc = DateTime.UtcNow;
            this.CsdNumber = csdNumber;

            CsdGlobals._destinationCsds.Add(this);
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

        /// <summary>
        ///     Create a clone of this object but only include files that have been successfully copied
        /// </summary>
        public CsdDetail TakeSnapshot()
        {
            CsdDetail newCopy = new CsdDetail()
            {
                RegisterDtmUtc = this.RegisterDtmUtc,
                CsdNumber = this.CsdNumber,
                TotalSpace = this.TotalSpace,
                BlockSize = this.BlockSize,
                WriteDtmUtc = this.WriteDtmUtc,
                BytesCopied = this.BytesCopied,
                DriveInfo = this.DriveInfo,
                Verifications = this.Verifications
            };

            newCopy.Files = this.Files.Where(x => x.Copied == true).ToList();
             
            return newCopy;
        }
    }
}