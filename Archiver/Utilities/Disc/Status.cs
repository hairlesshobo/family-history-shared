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
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Disc
{
    public static class Status
    {
        private static bool _initialized = false;
        private static int _copyWidth = 0;
        private static int _nextLine = -1;
        private static int _fileCountLine = -1;
        private static int _sizeLine = -1;
        private static int _distributeLine = -1;
        private static int _renameScanLine = -1;
        private static int _existingDiscCount = 0;
        private static int _newDiscs = 0;

        private static int _discLine = -1;

        private const string _scanningLabel = "Scanning";
        private const string _sizingLabel = "Size";
        private const string _distrubuteLabel = "Distribute";
        private const string _renameScanLabel = "Rename Scan";

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
                _existingDiscCount = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == false).Count();
                _newDiscs = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == true).Count();

                _nextLine++;
                _discLine = _nextLine++;

                _nextLine = _discLine + _newDiscs;
                _nextLine++;
                //_copyTotalLine = _nextLine;

                _copyWidth = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == true).Max(x => x.TotalFiles).ToString().Length;

                Console.CursorTop = _discLine;
                Console.CursorLeft = 0;
                Formatting.WriteC(ConsoleColor.Magenta, header);

                foreach (DiscDetail disc in DiscGlobals._destinationDiscs.Where(x => x.Finalized == false).OrderBy(x => x.DiscNumber))
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
            StatusHelpers.WriteStatusLine(DiscFormatting.GetDiscName(disc), line, ConsoleColor.Blue);
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
            line += StatusHelpers.FileCountPosition(currentFile, disc.TotalFiles, _copyWidth);
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(instantTransferRate).PadLeft(12) + "]";
            line += " ";
            line += "[" + Formatting.GetFriendlyTransferRate(averageTransferRate).PadLeft(12) + "]";

            if (disc.BytesCopied == disc.DataSize)
                complete = true;

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(DiscFormatting.GetDiscName(disc), line, currentPercent, complete, ConsoleColor.DarkYellow);
        }

        public static void WriteDiscIndex(DiscDetail disc, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing disc index:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(DiscFormatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteDiscHashListFile(DiscDetail disc, TimeSpan elapsed, double currentPercent)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Writing disc hash file:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(DiscFormatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteDiscIso(DiscDetail disc, TimeSpan elapsed, int currentPercent)
        {
            bool complete = (currentPercent == 100);

            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Creating ISO:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(DiscFormatting.GetDiscName(disc), line, currentPercent, complete, ConsoleColor.DarkYellow);
        }
        
        public static void WriteDiscIsoHash(DiscDetail disc, TimeSpan elapsed, double currentPercent = 0.0)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Reading ISO MD5:";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLineWithPct(DiscFormatting.GetDiscName(disc), line, currentPercent, (currentPercent == 100.0), ConsoleColor.DarkYellow);
        }

        public static void WriteDiscJsonLine(DiscDetail disc, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Saving disc details to json file ...";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(DiscFormatting.GetDiscName(disc), line, ConsoleColor.DarkYellow);
        }

        public static void WriteDiscComplete(DiscDetail disc, TimeSpan elapsed)
        {
            string line = "";
            line += Formatting.FormatElapsedTime(elapsed);
            line += " ";
            line += "Complete!";

            Console.SetCursorPosition(0, _discLine+disc.DiscNumber-_existingDiscCount);
            StatusHelpers.WriteStatusLine(DiscFormatting.GetDiscName(disc), line, ConsoleColor.DarkGreen);
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

            string currentSizeFriendly = Formatting.GetFriendlySize(DiscGlobals._totalSize);
            
            string line = StatusHelpers.FileCountPosition(fileCount, DiscGlobals._newlyFoundFiles);
            line += $" [{currentSizeFriendly.PadLeft(12)}]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)DiscGlobals._newlyFoundFiles) * 100.0;

            Console.SetCursorPosition(0, _sizeLine);
            StatusHelpers.WriteStatusLineWithPct(_sizingLabel, line, currentPercent, complete);
        }

        public static void FileDistributed(long fileCount, int discCount, bool complete = false)
        {
            if (_distributeLine == -1)
                _distributeLine = _nextLine++;            

            string line = StatusHelpers.FileCountPosition(fileCount, DiscGlobals._newlyFoundFiles);
            line += $" [ {discCount.ToString().PadLeft(3)} Disc(s)]";
            line += " ";

            double currentPercent = ((double)fileCount / (double)DiscGlobals._newlyFoundFiles) * 100.0;

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
        {
            Console.SetCursorPosition(0, _nextLine+2);
        }



    }
}