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
using Archiver.Shared.Models;

namespace Archiver.Utilities.Shared
{
    public class MD5_Generator
    {
        public delegate void ProgressChangedDelegateSimple(double currentPercent);

        public event ProgressChangedDelegateSimple OnProgressChanged;

        private string _filePath;
        private const int _sampleDurationMs = 100;
        public string MD5_Hash { get; set; }

        public MD5_Generator(string filePath)
        {
            _filePath = filePath;

            this.OnProgressChanged += delegate { };
        }

        public void GenerateHash()
        {
            byte[] buffer = new byte[256 * 1024]; // 256KB buffer

            using (FileStream source = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;

                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    int currentBlockSize = 0;

                    long lastSample = sw.ElapsedMilliseconds;
                    long totalBytesRead = 0;
                    int md5Offset = 0;
                    double currentPercent = 0.0;

                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytesRead += currentBlockSize;

                        currentPercent = ((double)totalBytesRead / (double)fileLength) * 100.0;
                        md5Offset += md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);
                        
                        if (sw.ElapsedMilliseconds - lastSample > _sampleDurationMs || currentBlockSize < buffer.Length)
                        {
                            OnProgressChanged(currentPercent);
                            lastSample = sw.ElapsedMilliseconds;
                        }
                    }

                    OnProgressChanged(currentPercent);
                    lastSample = sw.ElapsedMilliseconds;

                    sw.Stop();
                    md5.TransformFinalBlock(new byte[] { }, 0, 0);

                    this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                }
            }
        }
    }
}