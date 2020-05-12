using System;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Tape
{
    public class TapeStatus : IDisposable
    {
        private int _copyWidth = 0;
        private int _nextLine = -1;
        private int _fileCountLine = -1;
        private int _sizeLine = -1;
        private int _distributeLine = -1;
        private int _existingDiscCount = 0;
        private int _newDiscs = 0;

        private int _discLine = -1;

        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";

        private TapeDetail _tapeDetail;

        public TapeStatus(TapeDetail tapeDetail)
        {
            _tapeDetail = tapeDetail;
            _nextLine = Console.CursorTop;
        }

        public void InitDiscLines(string header = null)
        {
            if (header == null)
                header = "Preparing archive discs...";

            if (_discLine == -1)
            {
                _existingDiscCount = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == false).Count();
                _newDiscs = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == true).Count();

                _nextLine++;
                _discLine = _nextLine++;

                _nextLine = _discLine + _newDiscs;
                _nextLine++;
                //_copyTotalLine = _nextLine;

                _copyWidth = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == true).Max(x => x.TotalFiles).ToString().Length;

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.CursorTop = _discLine;
                Console.CursorLeft = 0;
                Console.Write(header);
                Console.ResetColor();

                foreach (DiscDetail disc in DiscGlobals._destinationDiscs.Where(x => x.Finalized == false).OrderBy(x => x.DiscNumber))
                    WriteDiscPendingLine(disc, default(TimeSpan));
            }
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
            line += $"{disc.TotalFiles.ToString().PadLeft(7)} files assigned";
            line += "   ";
            line += $"{Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)} data size";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.Blue);
        }

        public void WriteDiscCopyLine(
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

        public void WriteDiscJsonLine(DiscDetail disc, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Saving tape details to json file ...";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.DarkYellow);
        }

        public void WriteDiscComplete(DiscDetail disc, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Complete!";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(Formatting.GetDiscName(disc), line, ConsoleColor.DarkGreen);
        }








        public void FileScanned(long newFiles, long excludedFiles, bool complete = false)
        {
            if (_fileCountLine == -1)
                _fileCountLine = _nextLine++;

            string line = "";
            line += $"New: {newFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Excluded: {excludedFiles.ToString().PadLeft(7)}";

            if (complete)
                line += "   **Complete**";

            Console.SetCursorPosition(0, _fileCountLine);
            StatusHelpers.WriteStatusLine(_scanningLabel, line);
        }
        

        public void FileSized(long fileCount, bool complete = false)
        {
            if (_sizeLine == -1)
                _sizeLine = _nextLine++;

            string currentSizeFriendly = Formatting.GetFriendlySize(_tapeDetail.Stats.TotalSize);
            
            string line = FileCountPosition(fileCount);
            line += $" [{currentSizeFriendly.PadLeft(12)}]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)_tapeDetail.Stats.FileCount) * 100.0;

            Console.SetCursorPosition(0, _sizeLine);
            StatusHelpers.WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
        }


        public void ProcessComplete()
        {
            Console.SetCursorPosition(0, _nextLine+2);
        }


        private string FileCountPosition (long currentFile, long totalFiles = -1, int width = 0)
        {
            if (totalFiles == -1)
                totalFiles = _tapeDetail.Stats.FileCount;

            string totalFilesStr = totalFiles.ToString();

            if (width == 0)
                width = totalFilesStr.Length;

            return $"[{currentFile.ToString().PadLeft(width)} / {totalFiles.ToString().PadLeft(width)}]";
        }

        public void Dispose()
        {
            ProcessComplete();
        }
    }
}