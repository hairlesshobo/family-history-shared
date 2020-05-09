using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using static Archiver.Utilities.CustomFileCopier;

namespace Archiver.Utilities
{
    public class MD5_Disc
    {
        public string Hash {
            get
            {
                return _hash;
            }
            private set
            {
                _hash = value;
            }
        }

        private string _hash;
        private string _rawDrive;
        private string _driveLetter;
        private DriveInfo _driveInfo;
        private int _driveId = 0;

        public event MD5_CompleteDelegate OnComplete;
        public event ProgressChangedDelegate OnProgressChanged;
        public int SampleDurationMs { get; set; } = 500;

        public MD5_Disc (string DriveLetter)
        {
            DriveLetter = DriveLetter.Trim('/');
            DriveLetter = DriveLetter.Trim('\\');
            DriveLetter = DriveLetter.ToUpper();

            if (!DriveLetter.EndsWith(":"))
                DriveLetter += ":";

            _driveLetter = DriveLetter;
            _rawDrive = @"\\.\" + _driveLetter;
            _driveInfo = DriveInfo.GetDrives().FirstOrDefault(x => x.Name.TrimEnd('\\') == _driveLetter);

            if (_driveInfo == null)
                throw new DriveNotFoundException($"Drive {_driveLetter} was not found!");

            _driveId = Helpers.GetCdromId(_driveLetter);

            if (_driveInfo.IsReady == false)
            {
                Console.Write("Please insert disc");

                while (_driveInfo.IsReady == false)
                {
                    Thread.Sleep(1000);
                    Console.Write(".");
                }
                Console.WriteLine();
            }

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public bool CanGetHash()
        {
            return File.Exists(Globals._ddPath);
        }

        public void GenerateHash()
        {
            // ok so this is only here to help the esimate used for progress bar. why windows
            // refuses to see the full disc size, i have no damn idea. also, for the record..
            // i have no idea why only dd in cygwin reads the full disc, but every call using
            // win32 CreateFile(), and windows explorer, shows the size as too small and therefore
            // doesn't generate the correct md5sum. In my limited test case, it truncates off
            // 555008 byts of data. so, for the esimate, we add that back.
            Progress progress = new Progress();
            progress.TotalBytes = _driveInfo.TotalSize + 555008;
            int fieldWidth = progress.TotalBytes.ToString().Length;
            progress.TotalCopiedBytes = 0;

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                // cygwin dd seems to limit us to 64KB blocks, even when i specify a higher block
                // size, so we will just work with what we can. read above for why i even use
                // cygwin dd in the first place.
                int size = 64 * 1024; // 1 MiB buffer

                Process process = new Process();
                process.StartInfo.FileName = Globals._ddPath;
                process.StartInfo.Arguments = $"if=/dev/sr{_driveId} bs={size} status=none";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;

                process.Start();

                // int offset = 0;
                byte[] buffer = new byte[size];

                Stopwatch sw = new Stopwatch();
                sw.Start();

                long lastSample = sw.ElapsedMilliseconds;
                long lastSampleCopyTotal = 0;
                long sampleCount = 0;

                while (process.HasExited == false)
                {
                    int currentBlockSize = process.StandardOutput.BaseStream.Read(buffer, 0, size);

                    progress.TotalCopiedBytes += currentBlockSize;
                    progress.PercentCopied = ((double)progress.TotalCopiedBytes / (double)progress.TotalBytes) * 100.0;

                    if (sw.ElapsedMilliseconds - lastSample > this.SampleDurationMs || currentBlockSize < buffer.Length)
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

                    md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);
                }

                progress.TotalBytes = progress.TotalCopiedBytes;
                progress.PercentCopied = 100.0;
                OnProgressChanged(progress);

                md5.TransformFinalBlock(new byte [] {}, 0, 0);
                this.Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                sw.Stop();

                OnComplete(this.Hash);
            }
        }
    }
}