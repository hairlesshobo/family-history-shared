using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using static Archiver.Shared.Utilities.Formatting;

namespace Archiver.Operations
{
    public static class MainMenu
    {
        private static bool _initialized = false;
        private static CliMenu<bool> _menu; 

        public static void StartOperation()
        {
            ShowMenuOld();
            // ShowMenuNew();
        }

        private static void ShowMenuOld()
        {
            Initialize();

            while (1 == 1)
            {
                Console.Clear();
                Formatting.WriteLineSplit("Main Menu", "Archiver");
                Formatting.DrawHorizontalLine(LineType.Thin); 
                Console.CursorTop += 0; Console.CursorLeft = 0;

                bool result = _menu.Show(false).First();

                if (result == false)
                {
                    Console.WriteLine();
                    Console.Write("Process complete, press ");
                    Formatting.WriteC(ConsoleColor.DarkYellow, "<enter>");
                    Console.Write(", ");
                    Formatting.WriteC(ConsoleColor.DarkYellow, "<esc>");
                    Console.Write(", or ");
                    Formatting.WriteC(ConsoleColor.DarkYellow, "q");
                    Console.Write(" to return to the main menu...");

                    while (true)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape)
                            break;
                    }
                }
            }
        }

        private static void Initialize()
        {
            if (_initialized == false)
            {
                _initialized = true;

                List<CliMenuEntry<bool>> entries = new List<CliMenuEntry<bool>>();

                entries.AddRange(BuildDiscMenu());
                entries.Add(new CliMenuEntry<bool>() { Header = true });
                entries.AddRange(BuildTapeMenu());
                // entries.Add(new CliMenuEntry<bool>() { Header = true });
                // entries.AddRange(BuildCsdMenu());
                // entries.Add(new CliMenuEntry<bool>() { Header = true });
                // entries.AddRange(BuildUniversalMenu());

                _menu = new CliMenu<bool>(entries);
                //_menu.MenuLabel = "Archiver, main menu...";
                _menu.OnCancel += () =>
                {
                    Environment.Exit(0);
                };
            }
        }

        private static List<CliMenuEntry<bool>> BuildDiscMenu()
        {
            string discMenuAppend = String.Empty;

            if (SysInfo.IsOpticalDrivePresent == false)
                discMenuAppend += " (drive not detected)";
                
            return new List<CliMenuEntry<bool>>()
            {
                new CliMenuEntry<bool>() {
                    Name = "Disc Operations" + discMenuAppend,
                    Header = true,
                    ShortcutKey = ConsoleKey.D
                },
                new CliMenuEntry<bool>() {
                    Name = "Search Disc Archive",
                    Action = Disc.DiscSearcher.StartOperation,
                    ForegroundColor = ConsoleColor.Green,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                },
                //! not implemented
                new CliMenuEntry<bool>() {
                    Name = "Restore entire disc(s)",
                    Action = NotImplemented,
                    Disabled = !SysInfo.IsOpticalDrivePresent || true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                new CliMenuEntry<bool>() {
                    Name = "View Archive Summary",
                    Action = Disc.DiscSummary.StartOperation,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },                   
                new CliMenuEntry<bool>() {
                    Name = "Verify Discs",
                    Action = Disc.DiscVerification.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsOpticalDrivePresent,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new CliMenuEntry<bool>() {
                    Name = "Scan For Changes",
                    Action = Disc.DiscArchiver.StartScanOnly,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new CliMenuEntry<bool>() {
                    Name = "Scan For Renamed/Moved Files",
                    Action = Disc.ScanForFileRenames.StartOperation,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new CliMenuEntry<bool>() {
                    Name = "Run Archive process",
                    Action = Disc.DiscArchiver.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsOpticalDrivePresent,
                    ForegroundColor = ConsoleColor.Red
                }
            };
        }

        private static List<CliMenuEntry<bool>> BuildTapeMenu()
        {
            string tapeMenuAppend = String.Empty;

            if (SysInfo.IsTapeDrivePresent == false)
                tapeMenuAppend += " (drive not detected)";
                
            return new List<CliMenuEntry<bool>>()
            {
                new CliMenuEntry<bool>() {
                    Name = "Tape Operations" + tapeMenuAppend,
                    Header = true,
                    ShortcutKey = ConsoleKey.T
                },
                new CliMenuEntry<bool>() {
                    Name = "Search Tape Archive",
                    Action = Tape.TapeSearcher.StartOperation,
                    ForegroundColor = ConsoleColor.Green,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                },
                //! not implemented
                new CliMenuEntry<bool>() {
                    Name = "Restore entire tape (to tar file)",
                    Action = Tape.RestoreTapeToTar.StartOperation,
                    Disabled = !SysInfo.IsTapeDrivePresent || true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                //! not implemented
                new CliMenuEntry<bool>() {
                    Name = "Restore entire tape (to original file structure)",
                    Action = NotImplemented,
                    Disabled = !SysInfo.IsTapeDrivePresent || true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                new CliMenuEntry<bool>() {
                    Name = "Read Tape Summary",
                    Action = Tape.ShowTapeSummary.StartOperation,
                    Disabled = !SysInfo.IsTapeDrivePresent,
                    // SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new CliMenuEntry<bool>() {
                    Name = "View Archive Summary",
                    Action = Tape.TapeArchiveSummary.StartOperation,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new CliMenuEntry<bool>() {
                    Name = "Verify Tape",
                    Action = Tape.TapeVerification.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsTapeDrivePresent,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new CliMenuEntry<bool>() {
                    Name = "Run tape archive",
                    Action = Tape.TapeArchiver.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsTapeDrivePresent,
                    ForegroundColor = ConsoleColor.Red
                }
            };
        }

        private static List<CliMenuEntry<bool>> BuildCsdMenu()
        {
            return new List<CliMenuEntry<bool>>()
            {
                new CliMenuEntry<bool>() {
                    Name = "Cold Storage Disk (HDD) Operations",
                    Header = true,
                    ShortcutKey = ConsoleKey.C
                },
                new CliMenuEntry<bool>() {
                    Name = "Register CSD Drive",
                    Action = CSD.RegisterDrive.StartOperation,
                    ForegroundColor = ConsoleColor.Green
                },
                //! not implemented
                new CliMenuEntry<bool>() {
                    Name = "Restore entire CSD Drive",
                    Action = NotImplemented,
                    Disabled = true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                //! not implemented
                new CliMenuEntry<bool>() {
                    Name = "Read CSD Drive Summary",
                    Action = NotImplemented,         
                    // Action = ShowTapeSummary.StartOperation,
                    Disabled = true, // remove once implemented
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new CliMenuEntry<bool>() {
                    Name = "View CSD Archive Summary",
                    Action = CSD.ArchiveSummary.StartOperation,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                //! not implemented
                new CliMenuEntry<bool>() {
                    Name = "Verify CSD Drive",
                    Action = NotImplemented,
                    // Action = TapeVerification.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem || true, // remove once implemented
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new CliMenuEntry<bool>() {
                    Name = "Clean CSD Drive - Remove files not in index",
                    Action = CSD.Cleaner.StartOperation,
                    // Action = TapeVerification.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem, // remove once implemented
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new CliMenuEntry<bool>() {
                    Name = "Run CSD Archive Process",
                    Action = CSD.Archiver.StartOperation,
                    Disabled = SysInfo.IsReadonlyFilesystem,
                    ForegroundColor = ConsoleColor.Red
                }
            };
        }

        private static List<CliMenuEntry<bool>> BuildUniversalMenu()
        {
            return new List<CliMenuEntry<bool>>()
            {
                new CliMenuEntry<bool>() {
                    Name = "Universal Operations",
                    Header = true
                },
                new CliMenuEntry<bool>() {
                    Name = "Copy Tools to Local Disk",
                    Action = NotImplemented,
                    Disabled = !SysInfo.IsReadonlyFilesystem
                },
                new CliMenuEntry<bool>() {
                    Name = "Create Index ISO",
                    Action = Helpers.CreateIndexIso,
                    Disabled = SysInfo.IsReadonlyFilesystem
                },
                new CliMenuEntry<bool>() {
                    Name = "Exit",
                    Action = () => Environment.Exit(0)
                }
            };
        }

        private static void NotImplemented()
        {
            Formatting.WriteLineC(ConsoleColor.Red, "This operation has not yet been implemented.");
        }
    }
}