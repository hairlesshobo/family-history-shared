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
using Archiver.Classes.CSD;
using Archiver.Shared.Utilities;
using Archiver.Utilities;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
{
    public static class Archiver
    {
        public static void RunArchive(bool askBeforeArchive = false)
        {
            CsdUtils.ReadIndexToGlobal();
            Console.Clear();

            Formatting.WriteLineC(ConsoleColor.Magenta, "Preparing...");

            // ask whether to search for and process deletions
            Status.Initialize();

            Processing.IndexAndCountFiles();

            if (CsdGlobals._newFileCount > 0)
            {
                Processing.SizeFiles();
                bool sufficientSpace = Processing.VerifyFreeSpace();
                 
                if (!sufficientSpace)
                {
                    Status.ProcessComplete();

                    long requiredBytes = CsdGlobals._newFileEntries.Sum(x => x.Size);
                    long freeSpace = CsdGlobals._destinationCsds.Sum(x => x.FreeSpace);

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
                    Processing.DistributeFiles();

                    bool doProcess = true;

                    if (askBeforeArchive)
                    {
                        List<CsdDetail> csdsToWrite = CsdGlobals._destinationCsds
                                                                .Where(x => x.HasPendingWrites == true)
                                                                .ToList();

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine($"       New files found: {CsdGlobals._newFileCount.ToString("N0")}");
                        Console.WriteLine($"   Deleted files found: {CsdGlobals._deletedFileCount.ToString("N0")}");
                        Console.WriteLine($"CSD Drives to Write To: {csdsToWrite.Count.ToString("N0")}");
                        Console.WriteLine();
                        
                        Console.Write("Do you want to run the archive process now? (yes/");
                        Formatting.WriteC(ConsoleColor.Blue, "NO");
                        Console.Write(") ");

                        Console.CursorVisible = true;
                        string response = Console.ReadLine();
                        Console.CursorVisible = false;

                        int endLine = Console.CursorTop;

                        doProcess = response.ToLower().StartsWith("yes");
                        Console.WriteLine();
                        Console.WriteLine();
                    }

                    if (doProcess)
                        Processing.ProcessCsdDrives();
                }
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found to archive. Nothing to do.");
            }

            CsdGlobals.Reset();
        }

        public static void StartScanOnly()
            => RunArchive(true);

        public static void StartOperation()
            => RunArchive(false);
    }
}