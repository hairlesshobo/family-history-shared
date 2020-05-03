using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DiscArchiver.Classes;

namespace DiscArchiver.Utilities
{
    public class DiscVerification
    {
        private static int _nextLine = -1;
        private static int _discLine = -1;
        private static int _statusLine = -1;
        private static int _discCount = 0;
        private List<DestinationDisc> _discsToVerify;
        private List<int> _pendingDiscs;
        private string _driveLetter;

        public DiscVerification(string DriveLetter)
        {
            _discsToVerify = Globals._destinationDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber).ToList();

            Initialize(DriveLetter);
        }

        public DiscVerification(string DriveLetter, DestinationDisc disc)
        {
            _discsToVerify = new List<DestinationDisc>();
            _discsToVerify.Add(disc);

            Initialize(DriveLetter);
        }

        private void Initialize (string DriveLetter)
        {
            _pendingDiscs = _discsToVerify.Select(x => x.DiscNumber).ToList();

            _driveLetter = DriveLetter;

            // prepare the status output lines and whatnot
            _discCount = _discsToVerify.Count();

            _nextLine = Console.CursorTop;
            _nextLine += 2;
            _discLine = _nextLine;

            _nextLine = _discLine + _discCount;
            _nextLine += 2;
            _statusLine = _nextLine;
            _nextLine += 2;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.SetCursorPosition(0, _discLine);
            Console.CursorLeft = 0;
            Console.Write("Verifying discs...");
            Console.ResetColor();

            foreach (DestinationDisc disc in _discsToVerify)
                WriteDiscPendingLine(disc, default(TimeSpan));

            SetStatus("Starting...");
        }

        private void SetStatus(string status)
        {
            Console.SetCursorPosition(0, _statusLine);
            StatusHelpers.WriteStatusLine("Status", status);
        }

        private DestinationDisc WaitForDisc()
        {
            DestinationDisc insertedDisc = default(DestinationDisc);

            DriveInfo di = DriveInfo.GetDrives().Where(x => x.Name.TrimEnd('\\').ToUpper() == _driveLetter.ToUpper()).FirstOrDefault();

            if (di != null)
            {
                while (1 == 1)
                {
                    if (di.IsReady == false)
                        SetStatus("Please insert disc...");

                    else
                    {
                        if (di.VolumeLabel.ToLower().StartsWith("archive ") == false)
                            SetStatus("Unknown disc inserted...");

                        else
                        {
                            string discIdStr = di.VolumeLabel.Substring(8);
                            int discId = Int32.Parse(discIdStr);

                            if (_pendingDiscs.Contains(discId))
                                return _discsToVerify.FirstOrDefault(x => x.DiscNumber == discId);
                            else
                                SetStatus($"Archive disc {discId} does not need to be verified, please insert different disc");
                        }
                    }

                    Thread.Sleep(500);
                }
            }

            return insertedDisc;
        }

        public void StartVerification()
        {            
            DestinationDisc disc = WaitForDisc();

            Stopwatch sw = Stopwatch.StartNew();

            MD5_Disc md5disc = new MD5_Disc(_driveLetter);

            md5disc.OnProgressChanged += (progress) => {
                WriteDiscVerifyingLine(disc, sw.Elapsed, progress.PercentCopied, progress.TotalCopiedBytes, progress.InstantTransferRate, progress.AverageTransferRate);
            };

            md5disc.OnComplete += (hash) => {
                sw.Stop();

                if (disc.Hash.ToLower() == hash.ToLower())
                    WriteDiscVerificationPassed(disc, sw.Elapsed);
                else
                    WriteDiscVerificationFailed(disc, sw.Elapsed);
            };

            Thread generateThread = new Thread(md5disc.GenerateHash);
            generateThread.Start();
            generateThread.Join();
        }

        private int GetDiscIndex(DestinationDisc disc)
        {
            return _discsToVerify.IndexOf(disc)+1;
        }

        private void WriteDiscPendingLine(
            DestinationDisc disc, 
            TimeSpan elapsed = default(TimeSpan))
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Pending".PadRight(12);
            line += " ";
            line += $"{Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)} data size";

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.Blue);
        }

        public void WriteDiscVerifyingLine(
            DestinationDisc disc, 
            TimeSpan elapsed, 
            double currentPercent,
            long bytesRead,
            double instantTransferRate,
            double averageTransferRate
        )
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Reading:";
            line += " ";
            line += $"{Formatting.GetFriendlySize(disc.BytesCopied).PadLeft(10)}";
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(instantTransferRate).PadLeft(12) + "]";
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(averageTransferRate).PadLeft(12) + "]";

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        private void WriteDiscVerificationPassed(
            DestinationDisc disc, 
            TimeSpan elapsed = default(TimeSpan))
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Verification Passed!".PadRight(12);

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.Green);
        }

        private void WriteDiscVerificationFailed(
            DestinationDisc disc, 
            TimeSpan elapsed = default(TimeSpan))
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Verification FAILED!".PadRight(12);

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.Red);
        }
    }
}