using System;
using System.Linq;
using DiscArchiver.Shared;

namespace Archiver
{
    public static class Status
    {
        private static bool _initialized = false;
        private static int _copyWidth = 0;
        private static int _nextLine = -1;
        private static int _fileCountLine = -1;
        private static int _sizeLine = -1;
        private static int _distributeLine = -1;
        private static int _existingDiscCount = 0;
        private static int _newDiscs = 0;

        private static int _discLine = -1;

        private static volatile string _lock = "lock";

        private const int _leftHeaderWidth = 11;

        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";
        private const string _distrubuteLabel = "Distribute";

        public static void Initialize()
        {
            if (_initialized == false)
                _nextLine = Console.CursorTop;
        }

        public static void InitDiscLines()
        {
            if (_discLine == -1)
            {
                _existingDiscCount = Globals._destinationDiscs.Where(x => x.NewDisc == false).Count();
                _newDiscs = Globals._destinationDiscs.Where(x => x.NewDisc == true).Count();

                _nextLine++;
                _discLine = _nextLine++;

                _nextLine = _discLine + _newDiscs;
                _nextLine++;
                //_copyTotalLine = _nextLine;

                _copyWidth = Globals._destinationDiscs.Where(x => x.NewDisc == true).Max(x => x.TotalFiles).ToString().Length;

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.CursorTop = _discLine;
                Console.CursorLeft = 0;
                Console.Write("Preparing archive discs...");
                Console.ResetColor();

                foreach (DestinationDisc disc in Globals._destinationDiscs.Where(x => x.Finalized == false).OrderBy(x => x.DiscNumber))
                    WriteDiscPendingLine(disc, default(TimeSpan));
            }
        }

        private static string FormatElapsedTime(TimeSpan elapsed)
        {
            if (elapsed == default(TimeSpan))
                return "--:--:--";
            else
                return elapsed.ToString(@"hh\:mm\:ss");
        }

        private static string GetDiscName(DestinationDisc disc)
        {
            return $"Disc {disc.DiscNumber.ToString("0000")}";
        }

        private static void WriteDiscPendingLine(
            DestinationDisc disc, 
            TimeSpan elapsed = default(TimeSpan))
        {

            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Pending".PadRight(12);
            line += " ";
            line += $"{disc.TotalFiles.ToString().PadLeft(7)} files assigned";
            line += "   ";
            line += $"{Helpers.GetFriendlySize(disc.DataSize).PadLeft(10)} data size";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLine(GetDiscName(disc), line, ConsoleColor.Blue);
            }
        }

        public static void WriteDiscCopyLine(
            DestinationDisc disc, 
            TimeSpan elapsed = default(TimeSpan),
            int currentFile = 0, 
            double instantTransferRate = 0.0, 
            double averageTransferRate = 0.0)
        {
            bool complete = false;

            string line = "";

            line += FormatElapsedTime(elapsed);
            line += " Copy: ";
            line += $"{Helpers.GetFriendlySize(disc.BytesCopied).PadLeft(10)}";
            line += " ";
            line += FileCountPosition(currentFile, disc.TotalFiles, _copyWidth);
            line += " ";

            double currentPercent = ((double)disc.BytesCopied / (double)disc.DataSize) * 100.0;
            
            line += "[" + Helpers.GetFriendlyTransferRate(instantTransferRate).PadLeft(12) + "]";
            line += " ";
            line += "[" + Helpers.GetFriendlyTransferRate(averageTransferRate).PadLeft(12) + "]";

            if (disc.BytesCopied == disc.DataSize)
                complete = true;

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLineWithPct(GetDiscName(disc), line, currentPercent, complete, ConsoleColor.DarkYellow);
            }
        }

        public static void WriteDiscIndex(DestinationDisc disc, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing disc index:";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLineWithPct(GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
            }  
        }

        public static void WriteDiscHashListFile(DestinationDisc disc, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing disc hash file:";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLineWithPct(GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
            }  
        }

        public static void WriteDiscIso(DestinationDisc disc, TimeSpan elapsed, int currentPercent)
        {
            bool complete = (currentPercent == 100);

            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Creating ISO:";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLineWithPct(GetDiscName(disc), line, currentPercent, complete, ConsoleColor.DarkYellow);
            }
        }
        
        public static void WriteDiscIsoHash(DestinationDisc disc, TimeSpan elapsed, double currentPercent = 0.0)
        {
            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Reading ISO MD5:";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLineWithPct(GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
            }  
        }

        public static void WriteDiscJsonLine(DestinationDisc disc, TimeSpan elapsed)
        {
            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Saving disc details to json file ...";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLine(GetDiscName(disc), line, ConsoleColor.DarkYellow);
            }  
        }

        public static void WriteDiscComplete(DestinationDisc disc, TimeSpan elapsed)
        {
            string line = "";
            line += FormatElapsedTime(elapsed);
            line += " ";
            line += "Complete!";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);

                WriteStatusLine(GetDiscName(disc), line, ConsoleColor.DarkGreen);
            }  
        }











        private static void WriteStatusLineWithPct(string left, string right, double percent, bool complete)
        {
            WriteStatusLineWithPct(left, right, percent, complete, Console.ForegroundColor);
        }

        private static void WriteStatusLineWithPct(string left, string right, double percent, bool complete, ConsoleColor color)
        {
            string line = right;

            if (!line.EndsWith(" "))
                line += " ";

            int leftWidth = line.Length + 1;

            if (left != null)
                leftWidth += _leftHeaderWidth + 2;

            line += GeneratePercentBar(Console.WindowWidth, leftWidth, 0, percent, complete);

            WriteStatusLine(left, line, color);
        }

        private static void WriteStatusLine(string left, string right)
        {
            WriteStatusLine(left, right, Console.ForegroundColor);
        }

        private static void WriteStatusLine(string left, string right, ConsoleColor rightColor)
        {
            int padRight = Console.WindowWidth - _leftHeaderWidth - 2 - 1;

            if (left != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"{left.PadLeft(_leftHeaderWidth)}: ");
                Console.ResetColor();
            }

            Console.ForegroundColor = rightColor;
            Console.Write($"{right.PadRight(padRight)}");
            Console.ResetColor();
        }

        public static void FileScanned(long newFiles, long existingFiles, long excludedFiles, bool complete = false)
        {
            if (_fileCountLine == -1)
                _fileCountLine = _nextLine++;

            string line = "";
            line += $"New: {newFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Existing: {existingFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Excluded: {excludedFiles.ToString().PadLeft(7)}";

            if (complete)
                line += "   **Complete**";

            lock (_lock)
            {
                Console.SetCursorPosition(0, _fileCountLine);
                WriteStatusLine(_scanningLabel, line);
            }
        }
        

        public static void FileSized(long fileCount, long totalSizes, bool complete = false)
        {
            if (_sizeLine == -1)
                _sizeLine = _nextLine++;

            string currentSizeFriendly = Helpers.GetFriendlySize(Globals._totalSize);
            
            string line = FileCountPosition(fileCount);
            line += $" [{currentSizeFriendly.PadLeft(12)}]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)Globals._newlyFoundFiles) * 100.0;

            lock (_lock)
            {
                Console.SetCursorPosition(0, _sizeLine);
                WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
            }
        }

        public static void FileDistributed(long fileCount, int discCount, bool complete = false)
        {
            if (_distributeLine == -1)
                _distributeLine = _nextLine++;            

            string line = FileCountPosition(fileCount);
            line += $" [ {discCount.ToString().PadLeft(3)} Disc(s)]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)Globals._newlyFoundFiles) * 100.0;

            lock (_lock)
            {
                Console.SetCursorPosition(0, _distributeLine);
                WriteStatusLineWithPct(_distrubuteLabel, line, currentPercent, complete);
            }
        }

        


        public static void ProcessComplete()
        {
            Console.SetCursorPosition(0, _nextLine+2);
        }

        public static string GeneratePercentBar (int AvailableSpace, int LeftLength, int RightLength, double CurrentPercent, bool Complete)
        {
            string percentLeft = "[";
            string percentRight = "] " + Math.Round(CurrentPercent, 0).ToString().PadLeft(3) + "%";

            int totalSegments = AvailableSpace - LeftLength - RightLength - percentRight.Length - percentLeft.Length - 1;
            int completeSegments = (int)Math.Floor(totalSegments * (CurrentPercent/100.0));

            string progressBar = "";

            for (int i = 0; i < completeSegments; i++)
                progressBar += "=";

            if (Complete == false)
                progressBar += ">";

            string line = percentLeft + progressBar.PadRight(totalSegments) + percentRight;

            return line;
        }

        private static string FileCountPosition (long currentFile, long totalFiles = -1, int width = 0)
        {
            if (totalFiles == -1)
                totalFiles = Globals._newlyFoundFiles;

            string totalFilesStr = totalFiles.ToString();

            if (width == 0)
                width = totalFilesStr.Length;

            return $"[{currentFile.ToString().PadLeft(width)} / {totalFiles.ToString().PadLeft(width)}]";
        }
    }
}