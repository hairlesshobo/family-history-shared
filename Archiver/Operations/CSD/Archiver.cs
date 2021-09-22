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
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.Archiver.Shared.Classes.CSD;
using FoxHollow.Archiver.Shared.Utilities;
using FoxHollow.Archiver.Utilities.CSD;
using FoxHollow.Archiver.Utilities.Shared;
using FoxHollow.TerminalUI;

namespace FoxHollow.Archiver.Operations.CSD
{
    public static class Archiver
    {
        private static async Task RunArchive(bool askBeforeArchive, CancellationToken cToken)
        {
            // TODO: use cToken here
            CsdScanStats stats = new CsdScanStats(await Helpers.ReadCsdIndexAsync(cToken));
            Status.SetStats(stats);

            Terminal.Clear();
            Terminal.Header.UpdateLeft("CSD Archiver");

            // Formatting.WriteLineC(ConsoleColor.Magenta, "Preparing...");

            // ask whether to search for and process deletions
            Status.Initialize();

            Processing.IndexAndCountFiles(stats);

            if (stats.NewFileCount > 0)
            {
                Processing.SizeFiles(stats);
                bool sufficientSpace = Processing.VerifyFreeSpace(stats);
                 
                if (!sufficientSpace)
                {
                    Status.ProcessComplete();

                    long requiredBytes = stats.NewFileEntries.Sum(x => x.Size);
                    long freeSpace = stats.DestinationCsds.Sum(x => x.FreeSpace);

                    long additionalCapacityNeeded = requiredBytes - freeSpace;

                    Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                    Console.WriteLine("There is not enough free space on the CSD drives.");
                    Console.WriteLine();
                    Console.Write("Please register at least ");
                    Formatting.WriteC(ConsoleColor.Yellow, Formatting.GetFriendlySize(additionalCapacityNeeded));
                    Console.Write(" of additional CSD drives and then try again.");
                    Console.WriteLine();
                    Console.WriteLine();
                }
                else
                {
                    Processing.DistributeFiles(stats);

                    // bool doProcess = true;

                    // if (askBeforeArchive)
                    // {
                    //     List<CsdDetail> csdsToWrite = CsdGlobals._destinationCsds
                    //                                             .Where(x => x.HasPendingWrites == true)
                    //                                             .ToList();

                    //     Console.WriteLine();
                    //     Console.WriteLine();
                    //     Console.WriteLine($"       New files found: {CsdGlobals._newFileCount.ToString("N0")}");
                    //     Console.WriteLine($"   Deleted files found: {CsdGlobals._deletedFileCount.ToString("N0")}");
                    //     Console.WriteLine($"CSD Drives to Write To: {csdsToWrite.Count.ToString("N0")}");
                    //     Console.WriteLine();
                        
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
                    //     Processing.ProcessCsdDrives();
                }
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found to archive. Nothing to do.");
            }
        }

        public static Task StartScanOnlyAsync(CancellationToken cToken)
            => RunArchive(true, cToken);

        public static Task StartOperationAsync(CancellationToken cToken)
            => RunArchive(false, cToken);
    }
}