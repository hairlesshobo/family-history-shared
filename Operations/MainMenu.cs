using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.Tape;
using Archiver.Utilities;
using Archiver.Utilities.Shared;
using Archiver.Utilities.Tape;

namespace Archiver.Operations
{
    public static class MainMenu
    {
        private static CliMenu<bool> _menu = new CliMenu<bool>(new List<CliMenuEntry<bool>>()
        {
            new CliMenuEntry<bool>() {
                Name = "Disc Operations",
                Header = true
            },
            new CliMenuEntry<bool>() {
                Name = "Run Archive process",
                Action = DiscArchiver.StartOperation,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry<bool>() {
                Name = "View Archive Summary",
                Action = DiscSummary.StartOperation,
                SelectedValue = true // do not show the "press enter to return to main menu" message
            },
            new CliMenuEntry<bool>() {
                Name = "Verify Discs",
                Action = DiscVerification.StartOperation,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry<bool>() {
                Name = "Scan For Changes",
                Action = NotImplemented
            },

            new CliMenuEntry<bool>() {
                Header = true
            },
            new CliMenuEntry<bool>() {
                Name = "Tape Operations",
                Header = true
            },
            new CliMenuEntry<bool>() {
                Name = "Run tape archive",
                Action = TapeArchiver.StartOperation,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry<bool>() {
                Name = "Read Tape Summary",
                Action = ShowTapeSummary.StartOperation,
                SelectedValue = true // do not show the "press enter to return to main menu" message
            },
            new CliMenuEntry<bool>() {
                Name = "View Archive Summary",
                Action = TapeArchiveSummary.StartOperation,
                SelectedValue = true // do not show the "press enter to return to main menu" message
            },
            new CliMenuEntry<bool>() {
                Name = "Verify Tape",
                Action = TapeVerification.StartOperation
            },


            new CliMenuEntry<bool>() {
                Header = true
            },
            new CliMenuEntry<bool>() {
                Name = "Universal Operations",
                Header = true
            },
            new CliMenuEntry<bool>() {
                Name = "Copy Tools to Local Disk",
                Action = NotImplemented,
                Disabled = !Globals._readOnlyFs
            },
            new CliMenuEntry<bool>() {
                Name = "Create Index ISO",
                Action = Helpers.CreateIndexIso,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry<bool>() {
                Name = "Exit",
                Action = () => Environment.Exit(0)
            }
        });

        public static void StartOperation()
        {
            _menu.MenuLabel = "Archiver, main menu...";
            _menu.OnCancel += () =>
            {
                Environment.Exit(0);
            };

            while (1 == 1)
            {
                bool result = _menu.Show(true).First();

                if (result == false)
                {
                    Console.WriteLine();
                    Console.Write("Process complete, press <enter>, <q>, or <esc> to return to the main menu...");

                    while (true)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape)
                            break;
                    }
                }
            }
        }

        private static void NotImplemented()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("This operation has not yet been implemented.");
            Console.ResetColor();
        }
    }
}