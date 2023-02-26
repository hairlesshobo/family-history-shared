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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Structures;

namespace FoxHollow.FHM.Shared.Classes
{
    /// <summary>
    ///     Class used to generate a MD5 hash from a stream
    /// </summary>
    public class HashStreamGenerator
    {
        public delegate void ProgressChangedDelegate(HashGenerationProgress progress);
    
        private const int _defaultBlockSize = 1024 * 1024; // 1MB default block
        private int _blockSize = _defaultBlockSize;
        private Stream _stream;
        private bool _generateMd5 = true;
        private bool _generateSha1 = false;
        private Hashes _hashes = null;

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
        ///     the hash is generated, an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Will be thrown if an attempt is made to read the hash prior to generating it
        /// <exception>
        public string Md5Hash 
        { 
            get => (this.Complete ? _hashes.Md5Hash : throw new InvalidOperationException("The hash has not yet been generated"));
        }

        /// <summary>
        ///     This is the generated sha1 hash. If this field is attempted to be accessed before
        ///     the hash is generated, an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Will be thrown if an attempt is made to read the hash prior to generating it
        /// <exception>
        public string Sha1Hash
        {
            get => (this.Complete ? _hashes.Sha1Hash : throw new InvalidOperationException("The hash has not yet been generated"));
        }


        /// <summary>
        ///     Flag that specifies whether or not to generate MD5 hash 
        /// </summary>
        public bool GenerateMd5
        {
            get => _generateMd5;
            set => _generateMd5 = (!this.Started ? value : throw new InvalidOperationException("The hash generation has already begun"));
        }

        /// <summary>
        ///     Flag that specifies whether or not to generate sha1 hash 
        /// </summary>
        public bool GenerateSha1
        {
            get => _generateSha1;
            set => _generateSha1 = (!this.Started ? value : throw new InvalidOperationException("The hash generation has already begun"));
        }


        /// <summary>
        ///     Flag indivating whether the process has begun processing
        /// </summary>
        public bool Started { get; private set; } = false;

        /// <summary>
        ///     Flag indiciating whether the process has completed
        /// </summary>
        public bool Complete { get; private set; } = false;
        

        /// <summary>
        ///     Default constructor that builds a new generator from the provided stream
        /// </summary>
        /// <param name="stream">Stream to read from when generating hash</param>
        public HashStreamGenerator(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            this.OnProgressChanged += delegate { };
        }

        /// <summary>
        ///     Default constructor that builds a new generator from the provided stream
        /// </summary>
        /// <param name="stream">Stream to read from when generating hash</param>
        /// <param name="blockSize">Block size to use when reading from the stream</param>
        public HashStreamGenerator(Stream stream, int blockSize) : this(stream)
        {
            this.BlockSize = blockSize;
        }

        // TODO: Add fingerprint support.
        // Fingerprint support will require to know the size of the stream before it is read,
        // on-the-fly fingerprint generation will not work because of the nature of a fingerprint
        // selecting only samples of the data. This will require a new constructor that
        // accepts the StreamSize and automatically enables GenerateFingerprint.

        /// <summary>
        ///     Asynchronously generate the hashes
        /// </summary>
        /// <param name="cToken">Token that allows to cancel the process</param>
        /// <returns>Task that returns a generated hashes object</returns>
        public async Task<Hashes> GenerateAsync(CancellationToken cToken = default)
        {
            if (this.Complete)
                return _hashes;

            byte[] buffer = new byte[this.BlockSize]; 

            MD5 md5 = (_generateMd5 ? MD5.Create() : null);
            SHA1 sha1 = (_generateSha1 ? SHA1.Create() : null);

            HashGenerationProgress progress = new HashGenerationProgress()
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

            // TODO: convert to using ReadAsync
            while ((currentBlockSize = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (cToken.IsCancellationRequested)
                    return null;

                progress.TotalBytesProcessed += currentBlockSize;
                progress.PercentCompleted = ((double)progress.TotalBytesProcessed / (double)progress.TotalBytes);

                if (_generateMd5)
                    md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);

                if (_generateSha1)
                    sha1.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);

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

            string md5Hash = null;
            string sha1Hash = null;

            if (_generateMd5)
            {
                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                md5Hash = (_generateMd5 ? BitConverter.ToString(md5.Hash).Replace("-","").ToLower() : null);

                md5.Dispose();
            }

            if (_generateSha1)
            {
                sha1.TransformFinalBlock(new byte[] { }, 0, 0);

                sha1Hash = (_generateSha1 ? BitConverter.ToString(sha1.Hash).Replace("-","").ToLower() : null);

                sha1.Dispose();
            }

            this._hashes = new Hashes(md5Hash, sha1Hash);
            this.Complete = true;

            return _hashes;
        }
    }
}