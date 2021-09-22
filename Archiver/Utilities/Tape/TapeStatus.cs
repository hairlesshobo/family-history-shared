/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Diagnostics;
using System.Threading;
using FoxHollow.Archiver.Shared.Classes.Tape;
using FoxHollow.Archiver.Shared.Utilities;
using FoxHollow.Archiver.Utilities;
using FoxHollow.Archiver.Utilities.Shared;

namespace FoxHollow.Archiver.Utilities.Tape
{
    public class TapeStatus : IDisposable
    {
        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";
        private const string _elapsedLabel = "Elapsed";
        private const string _statusLabel = "Status";
        private const string _tarWriteLabel = "Creating Tar";
        private const string _writeTapeLabel = "Writing Tape";

        private int _topLine = -1;

        private int _elapsedLine = 1;
        private int _statusLine = 2;

        private int _scanLine = 4;
        private int _sizeLine = 5;

        private int _summaryLine = 4;

        private int _tarWriteLine = 9;
        
        private int _writeTapeLine = 10;

        private int _exitSummaryLine = 12;

        private int _bottomLine = 17;

        private double _bufferLastPercent = 0.0;

        private volatile bool _timerEnabled = false;
        private TapeDetail _tapeDetail;
        private Stopwatch _stopwatch;
        private ConsoleColor _defaultForeground;

        private readonly object _sync = new object();
        private Thread _timerThread;

        public TapeStatus(TapeDetail tapeDetail)
        {
            _tapeDetail = tapeDetail;
            _topLine = Console.CursorTop;
            _stopwatch = new Stopwatch();
            Console.CursorVisible = false;
            _defaultForeground = Console.ForegroundColor;
        }

        public void StartTimer()
        {
            _timerThread = new Thread(UpdateStatusLoop);
            _timerThread.Start();
        }

        private void UpdateStatusLoop()
        {
            _timerEnabled = true;

            lock (_stopwatch)
            {
                _stopwatch.Start();
            }

            while (_stopwatch.IsRunning || _timerEnabled)
            {
                lock (this)
                {
                    if (_stopwatch.IsRunning)
                        this.WriteElapsed();
                }

                Thread.Sleep(1000);
            }
        }

        public void WriteStatus(string statusMessage)
        {
            lock (this)
            {
                if (statusMessage.ToLower().StartsWith("complete"))
                    WriteLine(_statusLine, _statusLabel, statusMessage, ConsoleColor.Green);
                else
                    WriteLine(_statusLine, _statusLabel, statusMessage, ConsoleColor.Blue);
            }
        }

        private void WriteLine(int lineNum, string left, string right)
            => WriteLine(lineNum, left, right, Console.ForegroundColor);

        private void WriteLine(int lineNum, string left, string right, ConsoleColor rightColor)
        {
            StatusHelpers.ClearLine(_topLine + lineNum);

            if (!String.IsNullOrEmpty(left))
            {
                Console.Write(left);
                Console.Write(": ");
            }

            Formatting.WriteC(rightColor, right);
        }

        private void WriteElapsed()
        {
            lock (this)
            {
                WriteLine(_elapsedLine, _elapsedLabel, _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));
            }
        }

        public void ShowBeforeSummary(TapeInfo info)
        {
            lock (this)
            {
                ConsoleColor defaultColor = Console.ForegroundColor;

                WriteLine(_summaryLine, "Files Found", _tapeDetail.FileCount.ToString("N0"), ConsoleColor.Blue);

                WriteLine(_summaryLine+1, "Source Size", Formatting.GetFriendlySize(_tapeDetail.DataSizeBytes), ConsoleColor.Blue);

                ConsoleColor capacityColor = (_tapeDetail.TotalArchiveBytes > info.MediaInfo.Capacity ? ConsoleColor.Yellow : ConsoleColor.Green);
                WriteLine(_summaryLine+2, "Tape Capacity", Formatting.GetFriendlySize(info.MediaInfo.Capacity), capacityColor);

                double requiredCompressionRatio = ((double)_tapeDetail.TotalArchiveBytes / (double)info.MediaInfo.Capacity);
                
                if (requiredCompressionRatio <= 1.0)
                    requiredCompressionRatio = 1.0;

                WriteLine(_summaryLine+3, "Required compression ratio", $"{Math.Round(requiredCompressionRatio, 2).ToString("0.00")}:1", capacityColor);
            }
        }

        public void ShowAfterSummary(TapeInfo info)
        {
            lock (this)
            {
                ConsoleColor defaultColor = Console.ForegroundColor;

                WriteLine(_exitSummaryLine, "Archive Size", Formatting.GetFriendlySize(_tapeDetail.TotalArchiveBytes), ConsoleColor.Blue);
                WriteLine(_exitSummaryLine+1, "Size on Tape", Formatting.GetFriendlySize(_tapeDetail.ArchiveBytesOnTape), ConsoleColor.Blue);
                WriteLine(_exitSummaryLine+2, "Tape Capacity Remaining", Formatting.GetFriendlySize(info.MediaInfo.Remaining), ConsoleColor.Blue);
                WriteLine(_exitSummaryLine+3, "Compression Ratio on Tape", $"{Math.Round(_tapeDetail.CompressionRatio, 2).ToString("0.00")}:1", ConsoleColor.Blue);
            }
        }

        public void UpdateTarWrite(TapeTarWriterProgress progress)
        {
            string line = String.Empty;

            if (progress != null)
            {
                line += Formatting.GetFriendlySize(progress.TarBytesWritten).PadLeft(10);
                line += " ";
                line += $"[{progress.TarStatus.ToString().PadRight(8)}]";
                line += " ";
                line += $"[{Formatting.GetFriendlyTransferRate(progress.TarInstantTransferRate).PadLeft(12)}]";
                line += " ";
                line += $"[{Formatting.GetFriendlyTransferRate(progress.TarAverageTransferRate).PadLeft(12)}]";
                line += " ";
                line += StatusHelpers.FileCountPosition(progress.TarCurrentFileCount, progress.TarTotalFiles, 7);
            }

            lock (this)
            {
                Console.SetCursorPosition(0, _topLine + _tarWriteLine);
                StatusHelpers.WriteStatusLineWithPct(_tarWriteLabel, line, progress.TarWritePercent, (progress.TarBytesWritten == progress.TotalBytes));
            }
        }

        public void UpdateTapeWrite(TapeTarWriterProgress progress)
        {
            string line = String.Empty;

            if (progress != null)
            {
                bool increasing = progress.BufferPercent > _bufferLastPercent;

                _bufferLastPercent = progress.BufferPercent;

                line += Formatting.GetFriendlySize(progress.TapeBytesWritten).PadLeft(10);
                line += " ";
                line += $"[{progress.TapeStatus.ToString().PadRight(8)}]";
                line += " ";
                line += $"[{Formatting.GetFriendlyTransferRate(progress.TapeInstantTransferRate).PadLeft(12)}]";
                line += " ";
                line += $"[{Formatting.GetFriendlyTransferRate(progress.TapeAverageTransferRate).PadLeft(12)}]";
                line += " ";
                line += StatusHelpers.GeneratePercentBar(20, progress.BufferPercent, progress.BufferPercent == 100.0, increasing, true);

            }
            
            lock (this)
            {
                Console.SetCursorPosition(0, _topLine + _writeTapeLine);
                StatusHelpers.WriteStatusLineWithPct(_writeTapeLabel, line, progress.TapeWritePercent, (progress.TapeBytesWritten == progress.TotalBytes));
            }
        }

        public void FileScanned(long newFiles, long excludedFiles, bool complete = false)
        {
            string line = "";
            line += $"New: {newFiles.ToString().PadLeft(7)}";
            line += "   ";
            line += $"Excluded: {excludedFiles.ToString().PadLeft(7)}";

            if (complete)
                line += "   **Complete**";

            lock (this)
            {
                Console.SetCursorPosition(0, _topLine + _scanLine);
                StatusHelpers.WriteStatusLine(_scanningLabel, line);
            }
        }

        public void FileSized(long fileCount, bool complete = false)
        {
            lock (this)
            {
                string currentSizeFriendly = Formatting.GetFriendlySize(_tapeDetail.DataSizeBytes);
                
                string line = StatusHelpers.FileCountPosition(fileCount, _tapeDetail.FileCount);
                line += $" [{currentSizeFriendly.PadLeft(12)}]";
                line += " ";

                double currentPercent = ((double)fileCount / (double)_tapeDetail.FileCount) * 100.0;

                Console.SetCursorPosition(0, _topLine + _sizeLine);
                StatusHelpers.WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
            }
        }

        public bool ShowTapeWarning(double requiredRatio)
        {
            int tmpStartLine = _tarWriteLine;

            lock (this)
            {
                Console.SetCursorPosition(0, tmpStartLine);

                Formatting.WriteC(ConsoleColor.DarkYellow, "WARNING:");
                Console.Write(" The source data to be archived exceeds the tape capacity! This data source requires a ");
                Formatting.WriteC(ConsoleColor.Cyan, Math.Round(requiredRatio, 2).ToString("0.00") + ":1");
                Console.WriteLine(" compression ratio in order to fit.");
                Console.WriteLine();
                Console.WriteLine("For heavy data such as audio, photo, and video files, do not expect to achieve a ratio higher than 1.05:1 - 1.2:1!");
                Console.WriteLine();
                Console.Write("Do you want to continue anyways? (yes/");
                Formatting.WriteC(ConsoleColor.Blue, "NO");
                Console.Write(") ");
            }

            Console.CursorVisible = true;
            this.PauseTimer();
            string response = Console.ReadLine();
            Console.CursorVisible = false;

            int endLine = Console.CursorTop;

            if (response.ToLower().StartsWith("yes"))
            {
                for (int i = tmpStartLine; i <= endLine; i++)
                    StatusHelpers.ClearLine(i);

                this.ResumeTimer();
                return true;
            }
            
            return false;
        }


        private void PauseTimer()
        {
            lock (_stopwatch)
            {
                _stopwatch.Stop();
            }
        }

        private void ResumeTimer()
        {
            lock (_stopwatch)
            {
                _stopwatch.Start();
            }
        }

        public void StopTimer()
        {
            _timerEnabled = false;

            lock (_stopwatch)
            {
                _stopwatch.Stop();
            }

            if (_timerThread != null)
                _timerThread.Join();
        }

        private void ProcessComplete()
        {
            StopTimer();

            Console.SetCursorPosition(0, _topLine + _bottomLine);
            Console.CursorVisible = true;
        }

        public void Dispose()
            => ProcessComplete();
    }
}