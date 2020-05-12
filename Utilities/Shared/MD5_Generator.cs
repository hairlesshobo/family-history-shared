using System;
using System.Diagnostics;
using System.IO;

namespace Archiver.Utilities.Shared
{
    public delegate void MD5_ProgressChangedDelegate(double currentPercent);
    public delegate void MD5_CompleteDelegate(string hash);

    public class MD5_Generator
    {
        public event MD5_CompleteDelegate OnComplete;
        public event MD5_ProgressChangedDelegate OnProgressChanged;

        private string _filePath;
        private const int _sampleDurationMs = 100;
        public string MD5_Hash { get; set; }

        public MD5_Generator(string filePath)
        {
            _filePath = filePath;

            this.OnComplete += delegate { };
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

                    OnComplete(this.MD5_Hash);
                }
            }
        }
    }
}