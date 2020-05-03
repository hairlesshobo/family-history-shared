using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DiscArchiver.Classes;
using DiscArchiver.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

namespace DiscArchiver
{
    class Program
    {
        private static void RunArchiver()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Preparing...");
            Console.ResetColor();
            Status.Initialize();

            DiscProcessing.IndexAndCountFiles();

            if (Globals._newlyFoundFiles > 0)
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

        private static void ViewArchiveSummary()
        {
            IEnumerable<DestinationDisc> existingDiscs = Globals._destinationDiscs.Where(x => x.NewDisc == false);


            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Current archive statistics...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();

            Console.WriteLine($"                Discs: {existingDiscs.Count()}");
            Console.WriteLine($"                Files: {existingDiscs.Sum(x => x.Files.Count())}");
            Console.WriteLine($"            Data Size: {Formatting.GetFriendlySize(existingDiscs.Sum(x => x.DataSize))}");
            Console.WriteLine($"    Last Archive Date: {existingDiscs.Max(x => x.ArchiveDTM).ToShortDateString()}");
        }

        private static void VerifyDiscs()
        {
            Console.WriteLine("Disc verification process beginning...");
            Console.WriteLine();

            string selectedDrive = Helpers.SelectDrive();
            DiscVerification verifier = new DiscVerification(selectedDrive);
        }

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;

            Config.ReadConfig();
            Helpers.ReadIndex();


            CliMenu menu = new CliMenu(new List<CliMenuEntry>()
            {
                new CliMenuEntry() {
                    Name = "View Archive Summary",
                    Action = ViewArchiveSummary
                },
                new CliMenuEntry() {
                    Name = "Verify Discs",
                    Action = VerifyDiscs
                },
                new CliMenuEntry() {
                    Name = "Scan For Changes"
                },
                new CliMenuEntry() {
                    Name = "Run Archive process",
                    Action = RunArchiver
                },
                new CliMenuEntry() {
                    Name = "Create Index ISO",
                    Action = Helpers.CreateIndexIso
                },
                new CliMenuEntry() {
                    Name = "Exit",
                    Action = () => Environment.Exit(0)
                }
            });

            menu.MenuLabel = "Disc Archiver, main menu...";

            while (1 == 1)
            {
                menu.Show(true);
                Console.WriteLine();
                Console.WriteLine("Process complete, press enter to return to the main menu...");
                Console.ReadLine();
            }
        }
    }
}
