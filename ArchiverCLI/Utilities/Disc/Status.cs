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
using FoxHollow.Archiver.CLI.Utilities.Shared;

namespace FoxHollow.Archiver.CLI.Utilities.Disc
{
    public static class Status
    {
        private static int _nextLine = -1;
        private static int _renameScanLine = -1;

        private const string _renameScanLabel = "Rename Scan";
        





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

    }
}