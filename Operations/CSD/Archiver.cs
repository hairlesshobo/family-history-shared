using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.CSD;
using Archiver.Utilities;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
{
    public static class Archiver
    {
        public static void RunArchive(bool askBeforeArchive = false)
        {
            CsdGlobals._destinationCsds = CsdUtils.ReadIndex();
            Console.Clear();

            Formatting.WriteLineC(ConsoleColor.Magenta, "Preparing...");

            // ask whether to search for and process deletions
            Status.Initialize();

            Processing.IndexAndCountFiles();

            if (CsdGlobals._newFileCount > 0)
            {
                Processing.SizeFiles();
                Processing.VerifyFreeSpace();
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