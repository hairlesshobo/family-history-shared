using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.Disc;
using Archiver.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
{
    public static class DiscArchiver
    {
        public static void RunArchive(bool askBeforeArchive = false)
        {
            DiscGlobals._destinationDiscs = Helpers.ReadDiscIndex();
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Preparing...");
            Console.ResetColor();
            Status.Initialize();

            DiscProcessing.IndexAndCountFiles();

            if (DiscGlobals._newlyFoundFiles > 0)
            {
                DiscProcessing.SizeFiles();
                DiscProcessing.DistributeFiles();

                bool doProcess = true;

                List<DiscDetail> newDiscs = DiscGlobals._destinationDiscs
                                                        .Where(x => x.NewDisc == true)
                                                        .ToList();

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"    New files found: {DiscGlobals._newlyFoundFiles.ToString("N0")}");
                Console.WriteLine($"New Discs to Create: {newDiscs.Count}");
                Console.WriteLine();

                if (askBeforeArchive)
                {
                    ConsoleColor originalColor = Console.ForegroundColor;

                    Console.Write("Do you want to run the archive process now? (yes/");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("NO");
                    Console.ForegroundColor = originalColor;
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
                {
                    DiscProcessing.ProcessDiscs();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Process complete... don't forget to burn the ISOs to disc!");
                    Console.ResetColor();
                }
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found to archive. Nothing to do.");
            }

            DiscGlobals._destinationDiscs.Clear();
        }

        public static void StartScanOnly()
            => RunArchive(true);

        public static void StartOperation()
         => RunArchive(false);
    }
}