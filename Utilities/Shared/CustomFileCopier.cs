using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Archiver.Classes.Shared;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Shared
{

    public delegate void ProgressChangedDelegate(CustomFileCopier.Progress progress);
    public delegate void CompleteDelegate(CustomFileCopier.Progress progress);


    public class CustomFileCopier
    {
        public ISourceFile SourceFile { get; set; }
        public string DestinationRoot { get; set; }
        public string DestinationDirectory { get; set; }
        public string DestinationFilePath { get; set; }
        public bool OverwriteDestination { get; set; } = false;
        public bool Preserve { get; set; } = true;
        public int SampleDurationMs { get; set; } = 1000;
        public string MD5_Hash { get; set; } = null;

        public event CompleteDelegate OnComplete;
        public event ProgressChangedDelegate OnProgressChanged;

        public CustomFileCopier(ISourceFile sourceFile, string destinationRoot)
        {
            Initialize(sourceFile, destinationRoot);
        }

        private void Initialize(ISourceFile sourceFile, string destinationRoot, string newFileName = null)
        {
            this.SourceFile = sourceFile;
            this.DestinationRoot = Helpers.CleanPath(destinationRoot);
            this.DestinationDirectory = Helpers.CleanPath($"{this.DestinationRoot}/{this.SourceFile.RelativeDirectory}");

            if (!File.Exists(this.SourceFile.FullPath))
                throw new FileNotFoundException($"Source file doesn't exist: {this.SourceFile.FullPath}");

            if (!Directory.Exists(this.DestinationDirectory))
                Directory.CreateDirectory(this.DestinationDirectory);

            if (newFileName != null)
                this.DestinationFilePath = Path.Join(this.DestinationDirectory, newFileName);
            else
            {
                FileInfo fi = new FileInfo(this.SourceFile.FullPath);
                this.DestinationFilePath = Path.Join(this.DestinationDirectory, fi.Name);
            }

            this.DestinationFilePath = Helpers.CleanPath(this.DestinationFilePath);

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void Copy()
        {
            Progress progress = new Progress();

            byte[] buffer = new byte[1024 * 1024 * 2]; // 2MB buffer
            // bool cancelFlag = false;

            using (FileStream source = new FileStream(this.SourceFile.FullPath, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;

                progress.TotalBytes = fileLength;

                if (this.OverwriteDestination && File.Exists(this.DestinationFilePath))
                    File.Delete(this.DestinationFilePath);

                File.Create(this.DestinationFilePath).Dispose();

                FileInfo sourceFileInfo = new FileInfo(this.SourceFile.FullPath);

                progress.FileName = sourceFileInfo.Name;

                using (var md5 = MD5.Create())
                using (FileStream dest = new FileStream(this.DestinationFilePath, FileMode.Truncate, FileAccess.Write))
                {
                    int currentBlockSize = 0;

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    long lastSample = sw.ElapsedMilliseconds;
                    long lastSampleCopyTotal = 0;
                    long sampleCount = 0;
                    int md5Offset = 0;

                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        progress.TotalCopiedBytes += currentBlockSize;
                        progress.PercentCopied = ((double)progress.TotalCopiedBytes / (double)progress.TotalBytes) * 100.0;

                        dest.Write(buffer, 0, currentBlockSize);
                        md5Offset += md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);
                        
                        if (sw.ElapsedMilliseconds - lastSample > this.SampleDurationMs || currentBlockSize < buffer.Length)
                        {
                            sampleCount++;

                            progress.BytesCopiedSinceLastupdate = progress.TotalCopiedBytes - lastSampleCopyTotal;
                            double timeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastSample) / 1000.0;
                            lastSampleCopyTotal = progress.TotalCopiedBytes;
                            double instantTransferRate = (double)progress.BytesCopiedSinceLastupdate / timeSinceLastUpdate;

                            progress.InstantTransferRate = instantTransferRate;

                            if (sampleCount == 1)
                                progress.AverageTransferRate = instantTransferRate;
                            else
                                progress.AverageTransferRate = progress.AverageTransferRate + (instantTransferRate - progress.AverageTransferRate) / sampleCount;

                            progress.ElapsedTime = sw.Elapsed;
                            
                            if (currentBlockSize < buffer.Length)
                            {
                                progress.Complete = true;
                                sw.Stop();
                                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                                this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                            }

                            OnProgressChanged(progress);
                            lastSample = sw.ElapsedMilliseconds;
                        }
                    }
                }

                if (this.Preserve)
                {
                    FileInfo destinationFileInfo = new FileInfo(this.DestinationFilePath);

                    destinationFileInfo.LastAccessTimeUtc = sourceFileInfo.LastAccessTimeUtc;
                    destinationFileInfo.LastWriteTimeUtc = sourceFileInfo.LastWriteTimeUtc;
                    destinationFileInfo.CreationTimeUtc = sourceFileInfo.CreationTimeUtc;

                    PreserveDirectoryTimes();
                }
            }

            OnComplete(progress);
        }

        private void PreserveDirectoryTimes()
        {
            string[] dirParts = this.SourceFile.RelativeDirectory.Trim('/').Split("/");

            StringBuilder dirRecursion = new StringBuilder();

            foreach (string part in dirParts)
            {
                dirRecursion.Append($"/{part}");

                string sourceDirPart = $"{this.SourceFile.SourceRootPath}{dirRecursion.ToString()}";
                string destDirPart = $"{this.DestinationRoot}{dirRecursion.ToString()}";

                if (Directory.Exists(destDirPart) && Directory.Exists(destDirPart))
                {
                    // copy the relevant bits
                    DirectoryInfo destinationDirInfo = new DirectoryInfo(destDirPart);
                    DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDirPart);

                    destinationDirInfo.LastAccessTimeUtc = sourceDirInfo.LastAccessTimeUtc;
                    destinationDirInfo.LastWriteTimeUtc = sourceDirInfo.LastWriteTimeUtc;
                    destinationDirInfo.CreationTimeUtc = sourceDirInfo.CreationTimeUtc;
                }
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