using System;
using System.Collections.Generic;
using Archiver.Classes.Tape;
using Archiver.Utilities;
using Archiver.Utilities.Shared;
using Archiver.Utilities.Tape;

namespace Archiver.Operations
{
    public static class MainMenu
    {
        private static CliMenu<string> _menu = new CliMenu<string>(new List<CliMenuEntry<string>>()
        {
            new CliMenuEntry() {
                Name = "Disc Operations",
                Header = true
            },
            new CliMenuEntry() {
                Name = "Run Archive process",
                Action = Archiver.StartOperation,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry() {
                Name = "View Archive Summary",
                Action = Summary.StartOperation
            },
            new CliMenuEntry() {
                Name = "Verify Discs",
                Action = DiscVerification.StartOperation,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry() {
                Name = "Scan For Changes",
                Action = NotImplemented
            },

            new CliMenuEntry() {
                Header = true
            },
            new CliMenuEntry() {
                Name = "Tape Operations",
                Header = true
            },
            new CliMenuEntry() {
                Name = "Run tape archive",
                Action = () => {
                    TapeSourceInfo tape = TapeUtils.SelectTape();

                    if (tape != null)
                    {
                        TapeProcessor processor = new TapeProcessor(tape);
                        processor.ProcessTape();
                    }
                },
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry() {
                Name = "Read Tape Summary",
                Action = () => {
                    string text = TapeUtils.ReadTxtSummaryFromTape();

                    Console.WriteLine(text);
                    Console.SetCursorPosition(0, 0);
                }
            },
            new CliMenuEntry() {
                Name = "Verify Tape",
                Action = NotImplemented
            },


            new CliMenuEntry() {
                Header = true
            },
            new CliMenuEntry() {
                Name = "Universal Operations",
                Header = true
            },
            new CliMenuEntry() {
                Name = "Copy Tools to Local Disk",
                Action = NotImplemented,
                Disabled = !Globals._readOnlyFs
            },
            new CliMenuEntry() {
                Name = "Create Index ISO",
                Action = Helpers.CreateIndexIso,
                Disabled = Globals._readOnlyFs
            },
            new CliMenuEntry() {
                Name = "Exit",
                Action = () => Environment.Exit(0)
            }
        });

        public static void StartOperation()
        {
            _menu.MenuLabel = "Disc Archiver, main menu...";
            _menu.OnCancel += () =>
            {
                Environment.Exit(0);
            };

            while (1 == 1)
            {
                _menu.Show(true);
                Console.WriteLine();
                Console.WriteLine("Process complete, press enter to return to the main menu...");
                Console.ReadLine();
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