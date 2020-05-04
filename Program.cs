using System;
using System.Collections.Generic;
using System.Text;
using DiscArchiver.Classes;
using DiscArchiver.Operations;
using DiscArchiver.Utilities;
using Newtonsoft.Json;

namespace DiscArchiver
{
    class Program
    {
        private static void NotImplemented()
        {
            Console.WriteLine("This operation has not yet been implemented.");
        }

        // private static void DoScanOnly()
        // {
        //     Status.Initialize();

        //     DiscProcessing.IndexAndCountFiles();

        //     Status.ProcessComplete();

        //     Summary.StartOperation();
        // }

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;

            Config.ReadConfig();
            Helpers.ReadIndex();

            CliMenu menu = new CliMenu(new List<CliMenuEntry>()
            {
                new CliMenuEntry() {
                    Name = "Run Archive process",
                    Action = Archiver.StartOperation
                },
                new CliMenuEntry() {
                    Name = "Verify Discs",
                    Action = DiscVerification.StartOperation
                },
                new CliMenuEntry() {
                    Name = "View Archive Summary",
                    Action = Summary.StartOperation
                },
                new CliMenuEntry() {
                    Name = "Create Index ISO",
                    Action = Helpers.CreateIndexIso
                },
                new CliMenuEntry() {
                    Name = "Scan For Changes",
                    Action = NotImplemented
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
