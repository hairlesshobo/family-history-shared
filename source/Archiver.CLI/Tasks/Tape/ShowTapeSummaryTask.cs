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
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.Archiver.CLI.Utilities.Shared;
using FoxHollow.Archiver.CLI.Utilities.Tape;
using FoxHollow.Archiver.Shared.Utilities;
using FoxHollow.TerminalUI;
using FoxHollow.TerminalUI.Elements;

namespace FoxHollow.Archiver.CLI.Tasks.Tape
{
    public static class ShowTapeSummaryTask
    {
        public static async Task StartTaskAsync(CancellationToken cToken)
        {
            Terminal.Clear();
            Terminal.Header.UpdateLeft("Tape Summary");

            if (TapeUtils.IsTapeLoaded() == false)
            {
                Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                Console.WriteLine("No tape is present in the drive, please insert tape and run this operation again.");
            }
            else
            {
                // TODO: make this a class that async reads line by line like TapeArchiveSummaryTask?
                string text = TapeUtils.ReadTxtSummaryFromTape();

                using (Pager pager = Pager.StartNew())
                {
                    pager.AutoScroll = false;
                    pager.ShowLineNumbers = true;

                    foreach (string line in text.Split("\n"))
                        pager.AppendLine(line);

                    await pager.WaitForQuitAsync();
                }
            }
        }
    }
}