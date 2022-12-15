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
using FoxHollow.FHM.Shared.Structures;

namespace FoxHollow.FHM.Shared.Classes
{
    /// <summary>
    ///     Class used to generate a MD5 hash from a stream
    /// </summary>
    public class Md5StreamGenerator
    {
        public delegate void ProgressChangedDelegate(Md5Progress progress);
    
        private const int _defaultBlockSize = 1024 * 1024; // 1MB default block
        private int _blockSize = _defaultBlockSize;
        private Stream _stream;
        private string _hash = null;

        /// <summary>
        ///     Block size, in bytes, to use when reading from the source stream.
        /// 
        ///     Note: if the specified block size is 0 or less, the default block size will be used
        /// 
        ///     Default: 1 MB
        /// </summary>
        /// <value></value>
        public int BlockSize
        { 
            get => _blockSize;
            private set
            {
                if (value <= 0)
                    _blockSize = _defaultBlockSize;
                else
                    _blockSize = value;
            }
        }

        /// <summary>
        ///     Event that is fired periodically during the process with status updates.
        /// </summary>
        public event ProgressChangedDelegate OnProgressChanged;

        /// <summary>
        ///     Frequency, in milliseconds, with which to fire progress update events. 
        ///     0 = update after every block.
        /// 
        ///     WARNING: If this is set too low, it can have a negative impact on
        ///     generation performance. 
        /// 
        ///     Default: 500
        /// </summary>
        public int UpdateFrequencyMs { get; set; } = 500;

        /// <summary>
        ///     This is the generated md5 hash. If this field is attempted to be accessed before
        ///     the md5 is generated, an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Will be thrown if an attempt is made to read the hash prior to generating it
        /// <exception>
        public string Md5Hash 
        { 
            get => (this.Complete ? _hash : throw new InvalidOperationException("The hash has not yet been generated"));
            private set => _hash = value;
        }

        /// <summary>
        ///     Flag indiciating whether the process has completed
        /// </summary>
        /// <value></value>
        public bool Complete { get; private set; } = false;
        

        /// <summary>
        ///     Default constructor that builds a new generator from the provided stream
        /// </summary>
        /// <param name="stream">Stream to read from when generating hash</param>
        public Md5StreamGenerator(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            this.OnProgressChanged += delegate { };
        }

        /// <summary>
        ///     Default constructor that builds a new generator from the provided stream
        /// </summary>
        /// <param name="stream">Stream to read from when generating hash</param>
        /// <param name="blockSize">Block size to use when reading from the stream</param>
        public Md5StreamGenerator(Stream stream, int blockSize) : this(stream)
        {
            this.BlockSize = blockSize;
        }

        /// <summary>
        ///     Asynchronously generate the MD5 hash
        /// </summary>
        /// <param name="cToken">Token that allows to cancel the process</param>
        /// <returns>Task that returns a generated MD5 hash</returns>
        public Task<string> GenerateAsync(CancellationToken cToken)
            => Task.Run(() => Generate(cToken));

        /// <summary>
        ///     Generate the MD5 hash
        /// </summary>
        /// <param name="cToken">Token that allows to cancel the process</param>
        /// <returns>The generated MD5 hash</returns>
        private string Generate(CancellationToken cToken = default)
        {
            if (this.Complete)
                return this.Md5Hash;

            byte[] buffer = new byte[this.BlockSize]; 

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                Md5Progress progress = new Md5Progress()
                {
                    TotalBytesProcessed = 0,
                    BytesProcessedSinceLastUpdate = 0,
                    TotalBytes = _stream.Length,
                    PercentCompleted = 0.0,
                    InstantRate = 0.0,
                    AverageRate = 0.0,
                    Complete = false
                };

                int currentBlockSize = 0;

                Stopwatch sw = Stopwatch.StartNew();

                long lastSample = sw.ElapsedMilliseconds;
                long lastSampleCopyTotal = 0;
                long sampleCount = 0;

                while ((currentBlockSize = _stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (cToken.IsCancellationRequested)
                        return null;

                    progress.TotalBytesProcessed += currentBlockSize;
                    progress.PercentCompleted = ((double)progress.TotalBytesProcessed / (double)progress.TotalBytes);

                    md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);

                    // TODO: currentBlockSize can never be 0...
                    if (sw.ElapsedMilliseconds - lastSample > this.UpdateFrequencyMs || currentBlockSize == 0)
                    {
                        sampleCount++;

                        progress.BytesProcessedSinceLastUpdate = progress.TotalBytesProcessed - lastSampleCopyTotal;
                        double timeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastSample) / 1000.0;
                        lastSampleCopyTotal = progress.TotalBytesProcessed;

                        progress.InstantRate = (double)progress.BytesProcessedSinceLastUpdate / timeSinceLastUpdate;

                        if (sampleCount == 1)
                            progress.AverageRate = progress.InstantRate;
                        else
                            progress.AverageRate = progress.AverageRate + (progress.InstantRate - progress.AverageRate) / sampleCount;

                        progress.ElapsedTime = sw.Elapsed;

                        OnProgressChanged(progress);
                        lastSample = sw.ElapsedMilliseconds;
                    }
                }

                sw.Stop();

                progress.PercentCompleted = 1.0;
                progress.Complete = true;

                this.OnProgressChanged(progress);

                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                this.Md5Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                this.Complete = true;
            }

            return this.Md5Hash;
        }
    }
}