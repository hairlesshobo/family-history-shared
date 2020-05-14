using System;
using System.Diagnostics;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Tape
{
    public class TapeStatus : IDisposable
    {
        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";
        private const string _elapsedLabel = "Elapsed";
        private const string _statusLabel = "Status";
        private const string _bufferLabel = "Buffer";
        private const string _tarInputLabel = "Tar Input";
        private const string _tarWriteLabel = "Creating Tar";
        private const string _writeTapeLabel = "Writing Tape";

        private int _topLine = -1;

        private int _elapsedLine = 1;
        private int _statusLine = 2;

        private int _scanLine = 4;
        private int _sizeLine = 5;

        private int _tarInputLine = 7;
        private int _tarWriteLine = 8;
        
        private int _bufferLine = 10;
        private int _writeTapeLine = 11;

        private int _bottomLine = 13;

        private double _bufferLastPercent = 0.0;

        private TapeDetail _tapeDetail;
        private Stopwatch _stopwatch;

        public TapeStatus(TapeDetail tapeDetail)
        {
            _tapeDetail = tapeDetail;
            _topLine = Console.CursorTop;
            _stopwatch = new Stopwatch();
            Console.CursorVisible = false;
        }

        public void StartTimer()
        {
            _stopwatch.Start();
        }

        public void WriteStatus(string statusMessage)
        {
            WriteElapsed();
            Console.SetCursorPosition(0, _topLine + _statusLine);
            StatusHelpers.WriteStatusLine(_statusLabel, statusMessage);
        }

        public void UpdateBuffer(TapeTarWriterProgress progress)
        {
            string line = String.Empty;

            if (progress != null)
                line += Shared.Formatting.GetFriendlySize(progress.BufferFilledBytes).PadLeft(10);

            bool increasing = progress.BufferPercent > _bufferLastPercent;

            _bufferLastPercent = progress.BufferPercent;

            Console.SetCursorPosition(0, _topLine + _bufferLine);
            StatusHelpers.WriteStatusLineWithPct(_bufferLabel, line, progress.BufferPercent, (progress.BufferPercent == 100), increasing);
        }

        public void UpdateTarInput(TapeTarWriterProgress progress)
        {
            string line = String.Empty;

            if (progress != null)
            {

                line += StatusHelpers.FileCountPosition(progress.TarCurrentFileCount, progress.TarTotalFiles);
                line += " ";

                if (progress.TarStatus == TapeTarWriterStatus.Complete)
                    line += "".PadLeft(10);
                else
                    line += Shared.Formatting.GetFriendlySize(progress.TarCurrentFileSizeBytes).PadLeft(10);

                line += "   ";
                line += progress.TarCurrentFileName;
            }

            Console.SetCursorPosition(0, _topLine + _tarInputLine);
            StatusHelpers.WriteStatusLine(_tarInputLabel, line);
        }

        public void WriteElapsed()
        {
            Console.SetCursorPosition(0, _topLine + _elapsedLine);
            StatusHelpers.WriteStatusLine(_elapsedLabel, _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));
        }

        public void UpdateTarWrite(TapeTarWriterProgress progress)
        {
            string line = String.Empty;

            if (progress != null)
            {
                line += Shared.Formatting.GetFriendlySize(progress.TarBytesWritten).PadLeft(10);
                line += " ";
                line += $"[{progress.TarStatus.ToString().PadRight(8)}]";
                line += " ";
                line += $"[{Shared.Formatting.GetFriendlyTransferRate(progress.TarInstantTransferRate).PadLeft(12)}]";
                line += " ";
                line += $"[{Shared.Formatting.GetFriendlyTransferRate(progress.TarAverageTransferRate).PadLeft(12)}]";
            }

            Console.SetCursorPosition(0, _topLine + _tarWriteLine);
            StatusHelpers.WriteStatusLineWithPct(_tarWriteLabel, line, progress.TarWritePercent, (progress.TarBytesWritten == progress.TotalBytes));
        }

        public void UpdateTapeWrite(TapeTarWriterProgress progress)
        {
            string line = String.Empty;

            if (progress != null)
            {
                line += Shared.Formatting.GetFriendlySize(progress.TapeBytesWritten).PadLeft(10);
                line += " ";
                line += $"[{progress.TapeStatus.ToString().PadRight(8)}]";
                line += " ";
                line += $"[{Shared.Formatting.GetFriendlyTransferRate(progress.TapeInstantTransferRate).PadLeft(12)}]";
                line += " ";
                line += $"[{Shared.Formatting.GetFriendlyTransferRate(progress.TapeAverageTransferRate).PadLeft(12)}]";
            }

            Console.SetCursorPosition(0, _topLine + _writeTapeLine);
            StatusHelpers.WriteStatusLineWithPct(_writeTapeLabel, line, progress.TapeWritePercent, (progress.TapeBytesWritten == progress.TotalBytes));
        }

        public void FileScanned(long newFiles, long excludedFiles, bool complete = false)
        {
            string line = "";
            line += $"New: {newFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Excluded: {excludedFiles.ToString().PadLeft(7)}";

            if (complete)
                line += "   **Complete**";

            Console.SetCursorPosition(0, _topLine + _scanLine);
            StatusHelpers.WriteStatusLine(_scanningLabel, line);
        }

        public void FileSized(long fileCount, bool complete = false)
        {
            string currentSizeFriendly = Formatting.GetFriendlySize(_tapeDetail.DataSizeBytes);
            
            string line = StatusHelpers.FileCountPosition(fileCount, _tapeDetail.FileCount);
            line += $" [{currentSizeFriendly.PadLeft(12)}]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)_tapeDetail.FileCount) * 100.0;

            Console.SetCursorPosition(0, _topLine + _sizeLine);
            StatusHelpers.WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
        }




        private void ProcessComplete()
        {
            Console.SetCursorPosition(0, _topLine + _bottomLine);
            Console.CursorVisible = true;
        }

        public void Dispose()
        {
            ProcessComplete();
        }
    }
}