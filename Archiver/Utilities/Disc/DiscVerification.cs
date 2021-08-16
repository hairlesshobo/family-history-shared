using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Disc
{
    public class DiscVerifier
    {
        private static int _nextLine = -1;
        private static int _discLine = -1;
        private static int _statusLine = -1;
        private static int _cancelLine = -1;
        private static int _discCount = 0;
        private List<DiscDetail> _discsToVerify;
        private List<int> _pendingDiscs = new List<int>();
        private List<int> _completedDiscs = new List<int>();
        private string _driveLetter;
        private volatile bool _cancel = false;
        Thread _generateThread;

        public DiscVerifier(string DriveLetter)
        {
            _discsToVerify = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber).ToList();

            Initialize(DriveLetter, false);
        }

        public DiscVerifier(string DriveLetter, List<DiscDetail> discsToVerify)
        {
            _discsToVerify = discsToVerify;

            Initialize(DriveLetter, true);
        }

        private void Initialize (string DriveLetter, bool force)
        {
            // we only want to verify discs that haven't been verified in the past week, 
            // or have never been verified at all
            if (force == true)
            {
                _pendingDiscs = _discsToVerify.Select(x => x.DiscNumber)
                                              .ToList();
            }
            else
            {
                _pendingDiscs = _discsToVerify.Where(x => x.DaysSinceLastVerify > 7)
                                            .Select(x => x.DiscNumber)
                                            .ToList();
                                        
                _completedDiscs = _discsToVerify.Where(x => x.DaysSinceLastVerify <= 7)
                                                .Select(x => x.DiscNumber)
                                                .ToList();
            }

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
            _cancelLine = _nextLine;
            _nextLine += 2;

            Console.SetCursorPosition(0, _discLine);
            Console.CursorLeft = 0;
            Formatting.WriteC(ConsoleColor.Magenta, "Verifying discs...");

            foreach (DiscDetail disc in _discsToVerify)
            {
                if (disc.DaysSinceLastVerify > 7)
                    WriteDiscPendingLine(disc, default(TimeSpan));
                else
                {
                    if (disc.LastVerifySuccess)
                        WriteDiscVerificationPassed(disc, default(TimeSpan));
                    else
                        WriteDiscVerificationFailed(disc, default(TimeSpan));
                }
            }

            SetStatus("Starting...");

            Console.SetCursorPosition(0, _cancelLine);
            Console.Write("Press ");
            Formatting.WriteC(ConsoleColor.DarkYellow, "<ctrl+c>");
            Console.Write(" to cancel");
        }

        private void SetStatus(string status)
        {
            Console.SetCursorPosition(0, _statusLine);
            StatusHelpers.WriteStatusLine("Status", status);
        }

        private DiscDetail WaitForDisc()
        {
            DiscDetail insertedDisc = default(DiscDetail);

            DriveInfo di = DriveInfo.GetDrives().Where(x => x.Name.TrimEnd('\\').ToUpper() == _driveLetter.ToUpper()).FirstOrDefault();

            if (di != null)
            {
                string append = "";

                if (_pendingDiscs.Count() == 1)
                    append = " " + _pendingDiscs[0].ToString();

                while (_cancel == false)
                {
                    if (di.IsReady == false)
                        SetStatus($"Please insert archive disc{append}...");

                    else
                    {
                        if (di.VolumeLabel.ToLower().StartsWith("archive ") == false)
                            SetStatus("Unknown disc inserted...");

                        else
                        {
                            string discIdStr = di.VolumeLabel.Substring(8);
                            int discId = Int32.Parse(discIdStr);

                            if (_pendingDiscs.Contains(discId))
                            {
                                SetStatus($"Verifying disc {discId}");
                                return _discsToVerify.FirstOrDefault(x => x.DiscNumber == discId);
                            }
                            else if (_completedDiscs.Contains(discId))
                            {
                                if (_pendingDiscs.Count() == 1)
                                    SetStatus($"Archive disc {discId} has already been verified, please insert disc {_pendingDiscs[0].ToString()}");
                                else
                                    SetStatus($"Archive disc {discId} has already been verified, please insert another disc");
                            }
                            else
                            {
                                if (_pendingDiscs.Count() == 1)
                                    SetStatus($"Archive disc {discId} does not need to be verified, please insert disc {_pendingDiscs[0].ToString()}");
                                else
                                    SetStatus($"Archive disc {discId} does not need to be verified, please insert another disc");
                            }
                        }
                    }

                    Thread.Sleep(500);
                }
            }

            return insertedDisc;
        }

        public void StartVerification()
        {
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += (sender, e) => {
                _cancel = true;

                if (_generateThread != null && _generateThread.IsAlive)
                    _generateThread.Abort();
            };

            while (_pendingDiscs.Count() > 0 && _cancel == false)
            {
                DiscDetail disc = WaitForDisc();

                if (disc == null)
                    break;

                Stopwatch sw = Stopwatch.StartNew();

                MD5_Disc md5disc = new MD5_Disc(_driveLetter);

                md5disc.OnProgressChanged += (progress) => {
                    WriteDiscVerifyingLine(disc, sw.Elapsed, progress.PercentCopied, progress.TotalCopiedBytes, progress.InstantTransferRate, progress.AverageTransferRate);
                };

                md5disc.OnComplete += (hash) => {
                    sw.Stop();

                    bool discValid = (disc.Hash.ToLower() == hash.ToLower());

                    disc.RecordVerification(DateTime.UtcNow, discValid);
                    Helpers.SaveDestinationDisc(disc);

                    _pendingDiscs.Remove(disc.DiscNumber);
                    _completedDiscs.Add(disc.DiscNumber);

                    if (discValid)
                        WriteDiscVerificationPassed(disc, sw.Elapsed);
                    else
                        WriteDiscVerificationFailed(disc, sw.Elapsed);
                };

                _generateThread = new Thread(md5disc.GenerateHash);
                _generateThread.Start();
                _generateThread.Join();
            }

            if (_cancel == true)
                SetStatus("Process canceled");
            else
                SetStatus("All Discs have been verified, review results above.");
                
            Console.SetCursorPosition(0, _nextLine);
            Console.CursorVisible = true;
            Console.TreatControlCAsInput = true;
        }

        private int GetDiscIndex(DiscDetail disc)
        {
            return _discsToVerify.IndexOf(disc)+1;
        }

        private void WriteDiscPendingLine(
            DiscDetail disc, 
            TimeSpan elapsed = default(TimeSpan))
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Pending".PadRight(12);
            line += " ";
            line += $"{Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)} data size";

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLine(DiscFormatting.GetDiscName(disc), line, ConsoleColor.Blue);
        }

        public void WriteDiscVerifyingLine(
            DiscDetail disc, 
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
            line += $"{Formatting.GetFriendlySize(bytesRead).PadLeft(10)}";
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(instantTransferRate).PadLeft(12) + "]";
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(averageTransferRate).PadLeft(12) + "]";

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLineWithPct(DiscFormatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        private void WriteDiscVerificationPassed(
            DiscDetail disc, 
            TimeSpan elapsed = default(TimeSpan))
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Verification Passed!";
            line += "   ";
            line += "(" + disc.Verifications.OrderBy(x => x.VerificationDTM).Last().VerificationDTM.ToString("MM/dd/yyyy") + ")";

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLine(DiscFormatting.GetDiscName(disc), line, ConsoleColor.Green);
        }

        private void WriteDiscVerificationFailed(
            DiscDetail disc, 
            TimeSpan elapsed = default(TimeSpan))
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Verification FAILED!".PadRight(12);

            Console.SetCursorPosition(0, _discLine + GetDiscIndex(disc));
            StatusHelpers.WriteStatusLine(DiscFormatting.GetDiscName(disc), line, ConsoleColor.Red);
        }
    }
}