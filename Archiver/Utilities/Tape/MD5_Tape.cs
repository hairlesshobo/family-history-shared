using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Archiver.Classes.Shared;
using Archiver.Shared.Classes.Tape;
using Archiver.Shared.TapeDrivers;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Tape
{
    public class MD5_Tape
    {
        public event MD5_CompleteDelegate OnComplete;
        public event MD5_ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 500;
        public string MD5_Hash { get; set; }
        private long _sourceSize = 0;
        private bool _requiresJsonRecord = false;

        public MD5_Tape()
        {
            _requiresJsonRecord = true;

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public MD5_Tape(long sourceSize)
        {
            _sourceSize = sourceSize;
            
            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void GenerateHash()
        {
            bool hasJson = TapeUtils.TapeHasJsonRecord();
            int blockSize = Config.TapeBlockingFactor * 512;

            if (_requiresJsonRecord && !hasJson)
                throw new InvalidOperationException("MD5 class was created without a source size, therefore the tape must have the json summary record");
            
            if (_requiresJsonRecord && hasJson)
            {
                TapeSummary summary = TapeUtils.ReadTapeSummaryFromTape();
                blockSize = 512 * summary.BlockingFactor;
                _sourceSize = summary.TotalArchiveBytes;
            }

            using (NativeWindowsTapeDriver tape = new NativeWindowsTapeDriver(Config.TapeDrive, blockSize))
            using (MD5 md5 = MD5.Create())
            {
                Md5Progress progress = new Md5Progress();
                progress.TotalBytes = _sourceSize;
                progress.TotalCopiedBytes = 0;
                
                // seek the tape to the beginning of the file marker
                tape.SetTapeFilePosition(hasJson ? 2 : 1);

                int size = (int)tape.BlockSize;
                byte[] buffer = new byte[size];

                Stopwatch sw = Stopwatch.StartNew();

                long lastSample = sw.ElapsedMilliseconds;
                long lastSampleCopyTotal = 0;
                long sampleCount = 0;
                bool endOfData = false;

                do
                {
                    endOfData = tape.Read(buffer);

                    if (!endOfData)
                    {
                        progress.TotalCopiedBytes += size;
                        progress.PercentCopied = ((double)progress.TotalCopiedBytes / (double)progress.TotalBytes) * 100.0;
                    
                        md5.TransformBlock(buffer, 0, size, buffer, 0);
                    }
                    
                    if (sw.ElapsedMilliseconds - lastSample > _sampleDurationMs || endOfData)
                    {
                        sampleCount++;

                        progress.BytesCopiedSinceLastupdate = progress.TotalCopiedBytes - lastSampleCopyTotal;
                        double timeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastSample) / 1000.0;
                        lastSampleCopyTotal = progress.TotalCopiedBytes;

                        progress.InstantTransferRate = (double)progress.BytesCopiedSinceLastupdate / timeSinceLastUpdate;

                        if (sampleCount == 1)
                            progress.AverageTransferRate = progress.InstantTransferRate;
                        else
                            progress.AverageTransferRate = progress.AverageTransferRate + (progress.InstantTransferRate - progress.AverageTransferRate) / sampleCount;

                        progress.ElapsedTime = sw.Elapsed;

                        OnProgressChanged(progress);
                        lastSample = sw.ElapsedMilliseconds;
                    }
                }
                while (!endOfData);

                progress.PercentCopied = 100.0;

                OnProgressChanged(progress);
                lastSample = sw.ElapsedMilliseconds;

                sw.Stop();
                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();

                OnComplete(this.MD5_Hash);
            }
        }

        public class Progress 
        {
            public long TotalCopiedBytes { get; set; } = 0;
            public long BytesCopiedSinceLastupdate { get; set; } = 0;
            public long TotalBytes { get; set; } = 0;
            public double PercentCopied { get; set; } = 0.0;
            public double InstantTransferRate { get; set; } = 0.0;
            public double AverageTransferRate { get; set; } = 0.0;
            public TimeSpan ElapsedTime { get; set; }
            public bool Complete { get; set; } = false;
            public string FileName { get; set; } = String.Empty;
        }
    }
}