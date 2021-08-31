using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Models;

namespace Archiver.Shared.Classes
{
    public class Md5StreamGenerator
    {
        //! Warning: For some crazy ass, unknown reason.. on windows, if this is set higher than
        //! 131,072 the generated MD5 has will NOT be correct. I have no idea why. I've spent days 
        //! trying to hunt down the reason without any success. You've been warned
        private int _blockSize = 2048 * 512; // 1MB block size
        // private int _blockSize = 2048 * 64; // default 128KB block size;

        private Stream _stream;


        public event MD5_CompleteDelegate OnComplete;
        public event MD5_ProgressChangedDelegate OnProgressChanged;
        public string Md5Hash { get; private set; }

        public int SampleDurationMs { get; set; } = 500;
        

        public Md5StreamGenerator(Stream stream)
        {
            _stream = stream;

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public Md5StreamGenerator(Stream stream, int blockSize) : this(stream)
        {
            _blockSize = blockSize;
        }

        public async Task<string> GenerateAsync(CancellationToken cToken)
        {
            return await Task.Run(() => Generate(cToken));
        }

        private string Generate(CancellationToken cToken)
        {
            byte[] buffer = new byte[_blockSize]; 

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                Md5Progress progress = new Md5Progress();
                progress.TotalBytes = _stream.Length;
                progress.TotalCopiedBytes = 0;

                int currentBlockSize = 0;

                Stopwatch sw = Stopwatch.StartNew();

                long lastSample = sw.ElapsedMilliseconds;
                long lastSampleCopyTotal = 0;
                long sampleCount = 0;

                while ((currentBlockSize = _stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (cToken.IsCancellationRequested)
                        return null;

                    progress.TotalCopiedBytes += currentBlockSize;
                    progress.PercentCopied = ((double)progress.TotalCopiedBytes / (double)progress.TotalBytes) * 100.0;

                    md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);

                    if (sw.ElapsedMilliseconds - lastSample > this.SampleDurationMs || currentBlockSize == 0)
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

                progress.PercentCopied = 100.0;

                OnProgressChanged(progress);
                lastSample = sw.ElapsedMilliseconds;

                sw.Stop();

                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                this.Md5Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();

                OnComplete(this.Md5Hash);
            }

            return this.Md5Hash;
        }
    }
}