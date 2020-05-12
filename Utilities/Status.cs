using System;
using System.Linq;
using Archiver.Classes;

namespace Archiver.Utilities
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

        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";
        private const string _distrubuteLabel = "Distribute";

        public static void Initialize()
        {
            if (_initialized == false)
                _nextLine = Console.CursorTop;
        }

        public static void InitDiscLines(string header = null)
        {
            if (header == null)
                header = "Preparing archive discs...";

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
                Console.Write(header);
                Console.ResetColor();

                foreach (DiscDetail disc in Globals._destinationDiscs.Where(x => x.Finalized == false).OrderBy(x => x.DiscNumber))
                    WriteDiscPendingLine(disc, default(TimeSpan));
            }
        }

        
        private static void WriteDiscPendingLine(
            DiscDetail disc, 
            TimeSpan elapsed = default(TimeSpan))
        {

            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Pending".PadRight(12);
            line += " ";
            line += $"{disc.TotalFiles.ToString().PadLeft(7)} files assigned";
            line += "   ";
            line += $"{Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)} data size";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.Blue);
        }

        public static void WriteDiscCopyLine(
            DiscDetail disc, 
            TimeSpan elapsed = default(TimeSpan),
            int currentFile = 0, 
            double instantTransferRate = 0.0, 
            double averageTransferRate = 0.0)
        {
            bool complete = false;

            string line = "";

            double currentPercent = ((double)disc.BytesCopied / (double)disc.DataSize) * 100.0;

            line += Formatting.FormatElapsedTime(elapsed);
            line += " Copy: ";
            line += $"{Formatting.GetFriendlySize(disc.BytesCopied).PadLeft(10)}";
            line += " ";
            line += FileCountPosition(currentFile, disc.TotalFiles, _copyWidth);
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(instantTransferRate).PadLeft(12) + "]";
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(averageTransferRate).PadLeft(12) + "]";

            if (disc.BytesCopied == disc.DataSize)
                complete = true;

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, complete, ConsoleColor.DarkYellow);
        }

        public static void WriteDiscIndex(DiscDetail disc, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing disc index:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteDiscHashListFile(DiscDetail disc, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing disc hash file:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteDiscIso(DiscDetail disc, TimeSpan elapsed, int currentPercent)
        {
            bool complete = (currentPercent == 100);

            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Creating ISO:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, complete, ConsoleColor.DarkYellow);
        }
        
        public static void WriteDiscIsoHash(DiscDetail disc, TimeSpan elapsed, double currentPercent = 0.0)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Reading ISO MD5:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(Formatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteDiscJsonLine(DiscDetail disc, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Saving disc details to json file ...";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.DarkYellow);
        }

        public static void WriteDiscComplete(DiscDetail disc, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Complete!";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.DarkGreen);
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

            Console.SetCursorPosition(0, _fileCountLine);
            StatusHelpers.WriteStatusLine(_scanningLabel, line);
        }
        

        public static void FileSized(long fileCount, long totalSizes, bool complete = false)
        {
            if (_sizeLine == -1)
                _sizeLine = _nextLine++;

            string currentSizeFriendly = Formatting.GetFriendlySize(Globals._scannedTotalSize);
            
            string line = FileCountPosition(fileCount);
            line += $" [{currentSizeFriendly.PadLeft(12)}]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)Globals._scannedNewlyFoundFiles) * 100.0;

            Console.SetCursorPosition(0, _sizeLine);
            StatusHelpers.WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
        }

        public static void FileDistributed(long fileCount, int discCount, bool complete = false)
        {
            if (_distributeLine == -1)
                _distributeLine = _nextLine++;            

            string line = FileCountPosition(fileCount);
            line += $" [ {discCount.ToString().PadLeft(3)} Disc(s)]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)Globals._scannedNewlyFoundFiles) * 100.0;

            Console.SetCursorPosition(0, _distributeLine);
            StatusHelpers.WriteStatusLineWithPct(_distrubuteLabel, line, currentPercent, complete);
        }

        


        public static void ProcessComplete()
        {
            Console.SetCursorPosition(0, _nextLine+2);
        }


        private static string FileCountPosition (long currentFile, long totalFiles = -1, int width = 0)
        {
            if (totalFiles == -1)
                totalFiles = Globals._scannedNewlyFoundFiles;

            string totalFilesStr = totalFiles.ToString();

            if (width == 0)
                width = totalFilesStr.Length;

            return $"[{currentFile.ToString().PadLeft(width)} / {totalFiles.ToString().PadLeft(width)}]";
        }
    }
}