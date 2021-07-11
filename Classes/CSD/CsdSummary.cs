using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Utilities.Shared;

namespace Archiver.Classes.CSD
{
    public class CsdSummary
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
                // TODO: round the size up to blocksize.. pretty sure i have a function for tape that can be reused for this
                // TODO: Each file needs to be rounded up to block size, then summed
                return this.Files.Sum(x => Helpers.RoundToNextMultiple(x.Size, this.BlockSize));
            } 
        }
        public List<CsdSourceFile> Files { get; set; }
        
        public CsdSummary()
        {
            this.WriteDtmUtc = new List<DateTime>();
            this.Files = new List<CsdSourceFile>();
        }
    }
}