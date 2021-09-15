/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

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
        public delegate void CompleteDelegate(string hash);
        public delegate void ProgressChangedDelegate(Md5Progress progress);
    
        private int _blockSize = 2048 * 512; // 1MB block size
        // private int _blockSize = 2048 * 64; // default 128KB block size;

        private Stream _stream;


        public event CompleteDelegate OnComplete;
        public event ProgressChangedDelegate OnProgressChanged;
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

        public Task<string> GenerateAsync(CancellationToken cToken)
            => Task.Run(() => Generate(cToken));

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
                    progress.PercentCopied = ((double)progress.TotalCopiedBytes / (double)progress.TotalBytes);

                    md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);

                    // TODO: currentBlockSize can never be 0...
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

                progress.PercentCopied = 1.0;

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