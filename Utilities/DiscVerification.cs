using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DiscArchiver.Classes;

namespace DiscArchiver.Utilities
{
    public class DiscVerification
    {
        private static int _nextLine = -1;
        private static int _discLine = -1;
        private static int _discCount = 0;
        private List<DestinationDisc> _discsToVerify;
        private string _driveLetter;

        public DiscVerification (string DriveLetter)
        {
            _driveLetter = DriveLetter;

            // prepare the status output lines and whatnot
            _discsToVerify = Globals._destinationDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber).ToList();
            _discCount = _discsToVerify.Count();

            _nextLine = Console.CursorTop;
            _nextLine++;
            _nextLine++;
            _discLine = _nextLine;

            _nextLine = _discLine + _discCount;
            _nextLine++;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.SetCursorPosition(0, _discLine);
            Console.CursorLeft = 0;
            Console.Write("Verifying discs...");
            Console.ResetColor();

            foreach (DestinationDisc disc in _discsToVerify)
                WriteDiscPendingLine(disc, default(TimeSpan));
        }

        // public void StartVerification()
        // {
        //     MD5_Disc disc = new MD5_Disc(_driveLetter);

        //     disc.OnProgressChanged += (progress) => {
        //         Console.CursorLeft = 0;
        //         int fieldWidth = progress.TotalBytes.ToString().Length;
        //         Console.Write($"{Math.Round(progress.PercentCopied, 0).ToString().PadLeft(3)}% {progress.TotalCopiedBytes.ToString().PadLeft(fieldWidth)} / {progress.TotalBytes.ToString().PadLeft(fieldWidth)}");
        //     };

        //     disc.OnComplete += (hash) => {
        //         Console.SetCursorPosition(0, Console.CursorTop+2);
        //         Console.WriteLine(hash);
        //     };

        //     Thread generateThread = new Thread(disc.GenerateHash);
        //     generateThread.Start();
        //     generateThread.Join();
        // }

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
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.Blue);
        }
    }
}