using System;
using Archiver.Utilities;

namespace Archiver.Operations
{
    public static class Archiver
    {
        public static void StartOperation()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Preparing...");
            Console.ResetColor();
            Status.Initialize();

            DiscProcessing.IndexAndCountFiles();

            if (DiscGlobals._newlyFoundFiles > 0)
            {
                DiscProcessing.SizeFiles();
                DiscProcessing.DistributeFiles();

                DiscProcessing.ProcessDiscs();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Process complete... don't forget to burn the ISOs to disc!");
                Console.ResetColor();
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found to archive. Nothing to do.");
            }
        }
    }
}