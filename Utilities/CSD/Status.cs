using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.CSD
{
    public static class Status
    {
        private static int _copyWidth = 0;
        private static int _nextLine = -1;
        private static int _fileCountLine = -1;
        private static int _sizeLine = -1;
        private static int _distributeLine = -1;
        private static int _verifySpaceLine = -1;
        private static int _renameScanLine = -1;
        private static int _pendingCsdCount = 0;

        private static int _csdLine = -1;

        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";
        private const string _distrubuteLabel = "Distribute";
        private const string _renameScanLabel = "Rename Scan";
        private const string _verifySpaceLabel = "Verify Space";

        private static Dictionary<int, int> _driveLineIndex;

        public static void Initialize()
        {
            _copyWidth = 0;
            _nextLine = -1;
            _fileCountLine = -1;
            _sizeLine = -1;
            _distributeLine = -1;
            _verifySpaceLine = -1;
            _renameScanLine = -1;
            _pendingCsdCount = 0;
            _csdLine = -1;

            _nextLine = Console.CursorTop;
            _driveLineIndex = new Dictionary<int, int>();
        }

        public static void InitCsdDriveLines()
        {
            if (_csdLine == -1)
            {
                _pendingCsdCount = CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).Count();

                _nextLine++;
                _csdLine = _nextLine++;

                _copyWidth = CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).Max(x => x.PendingFileCount).ToString().Length;
   
                Console.CursorTop = _csdLine;
                Console.CursorLeft = 0;
                Formatting.WriteC(ConsoleColor.Magenta, "Preparing CSD drives ...");

                int currLine = _csdLine+1;
                foreach (CsdDetail csd in CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).OrderBy(x => x.CsdNumber))
                {
                    _driveLineIndex.Add(csd.CsdNumber, currLine);
                    WriteCsdPendingLine(csd, default(TimeSpan));
                    currLine++;
                }

                _nextLine = currLine;
                _nextLine++;
            }
        }

        
        private static void WriteCsdPendingLine(
            CsdDetail csd, 
            TimeSpan elapsed = default(TimeSpan))
        {

            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Pending".PadRight(15);
            line += " ";
            line += $"{csd.Files.Where(x => !x.Copied).Count().ToString().PadLeft(7)} files assigned";
            line += "   ";
            line += $"{Formatting.GetFriendlySize(csd.PendingFiles.Sum(x => x.Size)).PadLeft(10)} data size";

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLine(csd.CsdName, line, ConsoleColor.Blue);
        }

        public static void WriteAttachCsdLine(
            CsdDetail csd, 
            TimeSpan elapsed,
            long totalFileCount, 
            long totalBytes)
        {

            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += $"Attach {csd.CsdName}".PadRight(15);
            line += " ";
            line += $"{totalFileCount.ToString().PadLeft(7)} files assigned";
            line += "   ";
            line += $"{Formatting.GetFriendlySize(totalBytes).PadLeft(10)} data size"; 

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLine(csd.CsdName, line, ConsoleColor.Yellow);
        }

        public static void WriteCsdCopyLine(
            CsdDetail csd, 
            string driveLetter,
            TimeSpan elapsed = default(TimeSpan),
            int currentFile = 0, 
            int totalFileCount = 0,
            long bytesCopied = 0, 
            long totalBytes = 0,
            double instantTransferRate = 0.0, 
            double averageTransferRate = 0.0)
        {
            bool complete = false;

            string line = "";

            double currentPercent = ((double)bytesCopied / (double)totalBytes) * 100.0;

            line += Formatting.FormatElapsedTime(elapsed);
            line += " Copy: ";
            line += $"{Formatting.GetFriendlySize(bytesCopied).PadLeft(10)}";
            line += " / ";
            line += $"{Formatting.GetFriendlySize(totalBytes).PadLeft(10)}";
            line += " ";
            line += StatusHelpers.FileCountPosition(currentFile, totalFileCount, _copyWidth);
            line += " ";
            line += $"[{driveLetter}]";
            line += " ";
            line += "[I:" + Formatting.GetFriendlyTransferRate(instantTransferRate).PadLeft(11) + "]";
            line += " ";
            line += "[A:" + Formatting.GetFriendlyTransferRate(averageTransferRate).PadLeft(11) + "]";

            if (csd.BytesCopied == csd.DataSize)
                complete = true;

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLineWithPct(csd.CsdName, line, currentPercent, complete, ConsoleColor.Cyan);
        }

        public static void WriteCsdIndex(CsdDetail csd, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing CSD Drive index:";

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLineWithPct(csd.CsdName, line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteHashListFile(CsdDetail csd, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing CSD drive hash file:";

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLineWithPct(csd.CsdName, line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteJsonLine(CsdDetail csd, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Saving CSD drive details to json file ...";

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLine(csd.CsdName, line, ConsoleColor.DarkYellow);
        }

        public static void WriteCsdComplete(CsdDetail csd, TimeSpan elapsed, long totalFileCount, long totalBytes)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Complete!".PadRight(15);
            line += " ";
            line += $"{totalFileCount.ToString().PadLeft(7)} files copied";
            line += "   ";
            line += $"{Formatting.GetFriendlySize(totalBytes).PadLeft(10)} data copied";

            Console.SetCursorPosition(0, _driveLineIndex[csd.CsdNumber]);
            StatusHelpers.WriteStatusLine(csd.CsdName, line, ConsoleColor.DarkGreen);
        }








        public static void FileScanned(
            long newFiles, 
            long existingFiles, 
            long excludedFiles, 
            long deletedFiles, 
            long modifiedFiles, 
            double filesPerSecond,
            bool complete = false)
        {
            if (_fileCountLine == -1)
                _fileCountLine = _nextLine++;

            filesPerSecond = Math.Round(filesPerSecond, 1);

            string line = "";
            line += $"New: {newFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Existing: {existingFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Excluded: {excludedFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Deleted: {deletedFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Modified: {modifiedFiles.ToString().PadLeft(7)}";

            if (!complete)
            {
                line += "   ";
                line += $"[Scan Rate:{filesPerSecond.ToString("N0").PadLeft(5)} files/s]";
            }
            else
                line += "   **Complete**";

            Console.SetCursorPosition(0, _fileCountLine);
            StatusHelpers.WriteStatusLine(_scanningLabel, line);
        }
        

        public static void FileSized(long fileCount, long totalSizes, bool complete = false)
        {
            if (_sizeLine == -1)
                _sizeLine = _nextLine++;

            string currentSizeFriendly = Formatting.GetFriendlySize(CsdGlobals._totalSizePending);
            
            string line = StatusHelpers.FileCountPosition(fileCount, CsdGlobals._newFileCount);
            line += $" [{currentSizeFriendly.PadLeft(12)}]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)CsdGlobals._newFileCount) * 100.0;

            Console.SetCursorPosition(0, _sizeLine);
            StatusHelpers.WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
        }

        public static void WriteVerifyFreeSpace(long requiredBytes, long availableBytes, bool sufficientSpace)
        {
            if (_verifySpaceLine == -1)
                _verifySpaceLine = _nextLine++;

            string line = $"Space required: {Formatting.GetFriendlySize(requiredBytes)}, Space available: {Formatting.GetFriendlySize(availableBytes)}";

            ConsoleColor color = sufficientSpace ? ConsoleColor.Green : ConsoleColor.Red;

            Console.SetCursorPosition(0, _verifySpaceLine);
            StatusHelpers.WriteStatusLine(_verifySpaceLabel, line, color);
        }
        

        public static void FileDistributed(long fileCount, int csdCount, bool complete = false)
        {
            if (_distributeLine == -1)
                _distributeLine = _nextLine++;            

            string line = StatusHelpers.FileCountPosition(fileCount, CsdGlobals._newFileCount);
            line += $" [ {csdCount.ToString().PadLeft(4)} CSD(s)]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)CsdGlobals._newFileCount) * 100.0;

            Console.SetCursorPosition(0, _distributeLine);
            StatusHelpers.WriteStatusLineWithPct(_distrubuteLabel, line, currentPercent, complete);
        }

        public static void RenameScanned(long currentFileCount, long totalFileCount, long renamesFound, bool complete = false)
        {
            if (_renameScanLine == -1)
                _renameScanLine = _nextLine++;            

            string line = StatusHelpers.FileCountPosition(currentFileCount, totalFileCount);
            line += $" [ {renamesFound.ToString().PadLeft(5)} Found]";
            line += " ";

            double currentPercent = ((double)currentFileCount / (double)totalFileCount) * 100.0;

            Console.SetCursorPosition(0, _renameScanLine);
            StatusHelpers.WriteStatusLineWithPct(_renameScanLabel, line, currentPercent, complete);
        }


        public static void ProcessComplete()
            => Console.SetCursorPosition(0, _nextLine+2);
    }
}