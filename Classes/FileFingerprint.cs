/*
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
using System.IO;
using System.Security.Cryptography;

namespace FoxHollow.FHM.Shared.Classes
{
    // 
    // File Fingerprint Generation
    //
    // Data to select when generating fingerprint:
    //
    // If file is less than 512KB, use entire file
    //
    // If file is more than 512KB, then only use 512KB worth of data, selected as follows:
    //       16 KiB from beginning of file
    //       16 KiB from end of file
    //      480 KiB worth (120) 4KiB chunks from evenly distributed sections of the file
    //
    // Structure:
    //   file_content (512KB selected above) + file_length
    //
    // SHA256 the above structure 
    //
    //

    /// <summary>
    ///     Class used to generate file fingerprints
    /// </summary>
    public class FileFingerprint
    {
        // TODO: Redesign in a way that bytes can be fed directly to this class as opposed to opening the file and reading it directly
        public FileInfo FileInfo { get; private set; }
        public string Fingerprint { get; private set; }
        public long FileLength => this.FileInfo?.Length ?? -1;

        public static FileFingerprint GenerateFingerprint(
            string filePath,
            int sampleLen = 512, 
            int beginningSampleLen = 16, 
            int endSampleLen = 16, 
            int sampleChunkSize = 4
        ) {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            return GenerateFingerprint(new FileInfo(filePath), sampleLen, beginningSampleLen, endSampleLen, sampleChunkSize);
        }

        public static FileFingerprint GenerateFingerprint(
            FileInfo fileInfo, 
            int sampleLen = 512, 
            int beginningSampleLen = 16, 
            int endSampleLen = 16, 
            int sampleChunkSize = 4
        ) {
            FileFingerprint fingerprint = new FileFingerprint();
            fingerprint.FileInfo = fileInfo;

            byte[] lengthBytes = BitConverter.GetBytes(fingerprint.FileLength);

            // we work in bytes, but KiB was provided. We need to multiple by 1024
            sampleLen *= 1024;
            beginningSampleLen *= 1024;
            endSampleLen *= 1024;
            sampleChunkSize *= 1024;

            // setup a buffer to store the sample
            byte[] dataSample = new byte[sampleLen + lengthBytes.Length];

            // copy the length to the end of the data sample
            Array.Copy(lengthBytes, 0, dataSample, sampleLen, lengthBytes.Length);

            // if the file is smaller than or equal to the sample length, we just use the entire file
            if (fingerprint.FileInfo.Length <= sampleLen)
            {
                byte[] tmpBuffer = File.ReadAllBytes(fingerprint.FileInfo.FullName);

                Array.Copy(tmpBuffer, dataSample, tmpBuffer.Length);
            }
            
            // the file is larger, so we take a sample of the data to fingerprint
            else
            {
                // our sample chunks 
                int dataSampleLen = sampleLen - beginningSampleLen - endSampleLen;
                int sampleChunkCount = dataSampleLen / sampleChunkSize;

                long[] sampleOffsets = new long[sampleChunkCount+2]; // the +2 is to leave room for the start and and samples

                // first sample is always at the beginning of the file
                sampleOffsets[0] = 0;

                // last sample is always the length of the file minus the end sample size
                sampleOffsets[sampleOffsets.Length-1] = fingerprint.FileLength - endSampleLen;

                long middleFileSize = fingerprint.FileLength - beginningSampleLen - endSampleLen;
                long middleOffsetPerChunk = middleFileSize / sampleChunkSize;

                // loop through and generate the sample offsets
                for (int i = 1; i <= sampleChunkCount; i++)
                    sampleOffsets[i] = (middleOffsetPerChunk * i) + beginningSampleLen;

                using (FileStream source = new FileStream(fingerprint.FileInfo.FullName, FileMode.Open, FileAccess.Read))
                {
                    int currentBlockSize = 0;

                    // read the beginning chunk
                    currentBlockSize = source.Read(dataSample, 0, beginningSampleLen);

                    // read the middle sample data chunks
                    for (int chunk = 1; chunk <= sampleChunkCount; chunk++)
                    {
                        source.Seek(sampleOffsets[chunk], SeekOrigin.Begin);
                        currentBlockSize = source.Read(dataSample, (sampleChunkSize * (chunk-1)) + beginningSampleLen, sampleChunkSize);
                    }

                    // read the end chunk
                    source.Seek(sampleOffsets[sampleOffsets.Length-1], SeekOrigin.Begin);
                    currentBlockSize = source.Read(dataSample, sampleLen - endSampleLen, endSampleLen);
                }
            }
            

            // generate the fingerprint hash
            string hash;

            using (SHA256 shaM = SHA256.Create())
            {
                byte[] hashArray = shaM.ComputeHash(dataSample);
                hash = BitConverter.ToString(hashArray).Replace("-","").ToLower();
            }

            fingerprint.Fingerprint = hash;

            return fingerprint;
        }
        
    }
}