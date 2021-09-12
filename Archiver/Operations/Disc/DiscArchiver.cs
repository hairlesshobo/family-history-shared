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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;
using TerminalUI;

namespace Archiver.Operations.Disc
{
    public static class DiscArchiver
    {
        public static async Task RunArchiveAsync(bool askBeforeArchive = false)
        {
            DiscScanStats stats = new DiscScanStats(await Helpers.ReadDiscIndexAsync());
            Status.SetStats(stats);

            Terminal.Clear();
            Terminal.Header.UpdateLeft("Disc Archiver");

            Formatting.WriteLineC(ConsoleColor.Magenta, "Preparing...");
            Status.Initialize();

            DiscProcessing.IndexAndCountFiles(stats);

            if (stats.NewlyFoundFiles > 0)
            {
                DiscProcessing.SizeFiles(stats);
                DiscProcessing.DistributeFiles(stats);

                bool doProcess = true;

                List<DiscDetail> newDiscs = stats.DestinationDiscs
                                                 .Where(x => x.NewDisc == true)
                                                 .ToList();

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"    New files found: {stats.NewlyFoundFiles.ToString("N0")}");
                Console.WriteLine($"New Discs to Create: {newDiscs.Count}");
                Console.WriteLine();

                // if (askBeforeArchive)
                // {
                //     Console.Write("Do you want to run the archive process now? (yes/");
                //     Formatting.WriteC(ConsoleColor.Blue, "NO");
                //     Console.Write(") ");

                //     Console.CursorVisible = true;
                //     string response = Console.ReadLine();
                //     Console.CursorVisible = false;

                //     int endLine = Console.CursorTop;

                //     doProcess = response.ToLower().StartsWith("yes");
                //     Console.WriteLine();
                //     Console.WriteLine();
                // }

                // if (doProcess)
                // {
                //     DiscProcessing.ProcessDiscs(stats);

                //     Formatting.WriteLineC(ConsoleColor.Green, "Process complete... don't forget to burn the ISOs to disc!");
                // }
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found to archive. Nothing to do.");
            }
        }

        public static async Task StartScanOnly()
            => await RunArchiveAsync(true);

        public static async Task StartOperation()
            => await RunArchiveAsync(false);
    }
}