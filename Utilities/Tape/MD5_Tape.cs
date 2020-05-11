using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using static Archiver.Utilities.CustomFileCopier;

namespace Archiver.Utilities.Tape
{
    public class MD5_Tape
    {
        public event MD5_CompleteDelegate OnComplete;
        public event ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 500;
        public string MD5_Hash { get; set; }
        private long _sourceSize = 0;

        public MD5_Tape(long sourceSize)
        {
            _sourceSize = sourceSize;
            
            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void GenerateHash()
        {
            bool hasJson = TapeUtils.TapeHasJsonRecord();

            using (TapeOperator tape = new TapeOperator(Globals._tapeDrive, Globals._tapeBlockingFactor * 512))
            using (MD5 md5 = MD5.Create())
            {
                Progress progress = new Progress();
                progress.TotalBytes = _sourceSize;
                progress.TotalCopiedBytes = 0;
                
                // seek the tape to the beginning of the file marker
                tape.SetTapeFilePosition(hasJson ? 2 : 1);

                int size = (int)tape.BlockSize;
                byte[] buffer = new byte[size];

                Stopwatch sw = Stopwatch.StartNew();

                long lastSample = sw.ElapsedMilliseconds;
                int md5Offset = 0;
                long lastSampleCopyTotal = 0;
                long sampleCount = 0;
                bool endOfData = false;

                do
                {
                    endOfData = tape.Read(buffer);
                    progress.TotalCopiedBytes += size;
                    progress.PercentCopied = ((double)progress.TotalCopiedBytes / (double)progress.TotalBytes) * 100.0;

                    md5Offset += md5.TransformBlock(buffer, 0, size, buffer, 0);
                    
                    if (sw.ElapsedMilliseconds - lastSample > _sampleDurationMs)
                    {
                        sampleCount++;

                        progress.BytesCopiedSinceLastupdate = progress.TotalCopiedBytes - lastSampleCopyTotal;
                        double timeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastSample) / 1000.0;
                        lastSampleCopyTotal = progress.TotalCopiedBytes;

                        progress.InstantTransferRate = (double)progress.BytesCopiedSinceLastupdate / timeSinceLastUpdate;;

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

                progress.TotalBytes = progress.TotalCopiedBytes;
                progress.PercentCopied = 100.0;

                OnProgressChanged(progress);
                lastSample = sw.ElapsedMilliseconds;

                sw.Stop();
                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();

                OnComplete(this.MD5_Hash);
            }
        }
    }
}