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
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;
using TerminalUI.Types;
using static Archiver.Shared.Utilities.Formatting;

namespace Archiver.Operations
{
    public static class MainMenu
    {
        private static bool _initialized = false;
        private static Menu _menu; 

        public async static Task StartOperationAsync(CancellationTokenSource cts)
            => await ShowMenuAsync(cts);

        private async static Task ShowMenuAsync(CancellationTokenSource cts)
        {
            BuildMenu();
            _menu.QuitCallback = () => {
                cts.Cancel();
                return Task.CompletedTask;
            };

            while (!cts.IsCancellationRequested)
            {
                Terminal.Clear();
                Terminal.InitHeader("Main Menu", "Archiver");

                var results = await _menu.ShowAsync(cts.Token);

                if (results == null)
                    return;

                if (results.First() == null)
                    continue;

                bool result = (bool)results.First();

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

        private static void BuildMenu()
        {
            if (_initialized == false)
            {
                _initialized = true;

                List<MenuEntry> entries = new List<MenuEntry>();

                entries.AddRange(BuildDiscMenu());
                entries.Add(new MenuEntry() { Header = true });
                entries.AddRange(BuildTapeMenu());
                entries.Add(new MenuEntry() { Header = true });
                entries.AddRange(BuildCsdMenu());
                entries.Add(new MenuEntry() { Header = true });
                entries.AddRange(BuildUniversalMenu());

                _menu = new Menu(entries);
                //_menu.MenuLabel = "Archiver, main menu...";
                // _menu.OnCancel += () =>
                // {
                //     Environment.Exit(0);
                // };
            }
        }

        private static List<MenuEntry> BuildDiscMenu()
        {
            string discMenuAppend = String.Empty;

            if (SysInfo.IsOpticalDrivePresent == false)
                discMenuAppend += " (drive not detected)";
                
            return new List<MenuEntry>()
            {
                new MenuEntry() {
                    Name = "Disc Operations" + discMenuAppend,
                    Header = true
                },
                new MenuEntry() {
                    Name = "Search Disc Archive",
                    Task = Tasks.Disc.DiscSearcherTask.StartTaskAsync,
                    ForegroundColor = ConsoleColor.Green,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                },
                //! not implemented
                new MenuEntry() {
                    Name = "Restore entire disc(s)",
                    Task = NotImplementedAsync,
                    SelectedValue = true,
                    Disabled = !SysInfo.IsOpticalDrivePresent || true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                new MenuEntry() {
                    Name = "View Archive Summary",
                    Task = Tasks.Disc.DiscArchiveSummaryTask.StartTaskAsync,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new MenuEntry() {
                    Name = "Verify Discs",
                    Task = Tasks.Disc.DiscVerificationTask.StartTaskAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsOpticalDrivePresent,
                    ForegroundColor = ConsoleColor.DarkYellow
                }
                ,
                new MenuEntry() {
                    Name = "Scan For Changes",
                    Task = Tasks.Disc.DiscArchiverTask.StartScanOnlyAsync,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                // new MenuEntry() {
                //     Name = "Scan For Renamed/Moved Files",
                //     Task = Disc.ScanForFileRenames.StartOperationAsync,
                //     ForegroundColor = ConsoleColor.DarkYellow
                // },
                new MenuEntry() {
                    Name = "Run Archive process",
                    Task = Tasks.Disc.DiscArchiverTask.StartOperationAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsOpticalDrivePresent,
                    ForegroundColor = ConsoleColor.Red
                }
            };
        }

        private static List<MenuEntry> BuildTapeMenu()
        {
            string tapeMenuAppend = String.Empty;

            if (SysInfo.IsTapeDrivePresent == false)
                tapeMenuAppend += " (drive not detected)";
                
            return new List<MenuEntry>()
            {
                new MenuEntry() {
                    Name = "Tape Operations" + tapeMenuAppend,
                    Header = true
                },
                new MenuEntry() {
                    Name = "Search Tape Archive",
                    Task = Tasks.Tape.TapeSearcherTask.StartTaskAsync,
                    ForegroundColor = ConsoleColor.Green,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                },
                //! not implemented
                new MenuEntry() {
                    Name = "Restore entire tape (to tar file)",
                    Task = Tape.RestoreTapeToTar.StartOperationAsync,
                    Disabled = !SysInfo.IsTapeDrivePresent || true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                //! not implemented
                new MenuEntry() {
                    Name = "Restore entire tape (to original file structure)",
                    Task = NotImplementedAsync,
                    Disabled = !SysInfo.IsTapeDrivePresent || true, // remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                // new MenuEntry() {
                //     Name = "Read Tape Summary",
                //     Task = Tape.ShowTapeSummary.StartOperation,
                //     Disabled = !SysInfo.IsTapeDrivePresent,
                //     // SelectedValue = true, // do not show the "press enter to return to main menu" message
                //     ForegroundColor = ConsoleColor.Blue
                // },
                new MenuEntry() {
                    Name = "View Archive Summary",
                    Task = Tasks.Tape.TapeArchiveSummaryTask.StartTaskAsync,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                // new MenuEntry() {
                //     Name = "Verify Tape",
                //     Task = Tape.TapeVerification.StartOperation,
                //     Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsTapeDrivePresent,
                //     ForegroundColor = ConsoleColor.DarkYellow
                // },
                // new MenuEntry() {
                //     Name = "Run tape archive",
                //     Task = Tape.TapeArchiver.StartOperation,
                //     Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsTapeDrivePresent,
                //     ForegroundColor = ConsoleColor.Red
                // }
            };
        }

        private static List<MenuEntry> BuildCsdMenu()
        {
            return new List<MenuEntry>()
            {
                new MenuEntry() {
                    Name = "Cold Storage Disk (HDD) Operations",
                    Header = true
                },
                new MenuEntry() {
                    Name = "Register CSD Drive",
                    Task = CSD.RegisterDrive.StartOperationAsync,
                    ForegroundColor = ConsoleColor.Green
                },
        //         //! not implemented
        //         new MenuEntry() {
        //             Name = "Restore entire CSD Drive",
        //             Task = NotImplemented,
        //             Disabled = true, // remove once implemented
        //             ForegroundColor = ConsoleColor.Green
        //         },
        //         //! not implemented
        //         new MenuEntry() {
        //             Name = "Read CSD Drive Summary",
        //             Task = NotImplemented,         
        //             // Task = ShowTapeSummary.StartOperation,
        //             Disabled = true, // remove once implemented
        //             SelectedValue = true, // do not show the "press enter to return to main menu" message
        //             ForegroundColor = ConsoleColor.Blue
        //         },
                new MenuEntry() {
                    Name = "View CSD Archive Summary",
                    Task = Tasks.CSD.CsdArchiveSummaryTask.StartTaskAsync,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
        //         //! not implemented
        //         new MenuEntry() {
        //             Name = "Verify CSD Drive",
        //             Task = NotImplemented,
        //             // Task = TapeVerification.StartOperation,
        //             Disabled = SysInfo.IsReadonlyFilesystem || true, // remove once implemented
        //             ForegroundColor = ConsoleColor.DarkYellow
        //         },
        //         new MenuEntry() {
        //             Name = "Clean CSD Drive - Remove files not in index",
        //             Task = CSD.Cleaner.StartOperation,
        //             // Task = TapeVerification.StartOperation,
        //             Disabled = SysInfo.IsReadonlyFilesystem, // remove once implemented
        //             ForegroundColor = ConsoleColor.DarkYellow
        //         },
                new MenuEntry() {
                    Name = "Run CSD Archive Process",
                    Task = CSD.Archiver.StartOperationAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem,
                    ForegroundColor = ConsoleColor.Red
                }
            };
        }

        private static List<MenuEntry> BuildUniversalMenu()
        {
            return new List<MenuEntry>()
            {
                new MenuEntry() {
                    Name = "Universal Operations",
                    Header = true
                },
                // new MenuEntry() {
                //     Name = "Copy Tools to Local Disk",
                //     Task = NotImplemented,
                //     Disabled = !SysInfo.IsReadonlyFilesystem
                // },
                // new MenuEntry() {
                //     Name = "Create Index ISO",
                //     Task = Helpers.CreateIndexIso,
                //     Disabled = SysInfo.IsReadonlyFilesystem
                // },
                new MenuEntry() {
                    Name = "Explode",
                    Task = (cToken) => throw new InvalidOperationException("meow!!!!")
                }//,
                // new MenuEntry() {
                //     Name = "Exit",
                //     Task = () => Environment.Exit(0)
                // }
            };
        }

        private static async Task NotImplementedAsync(CancellationToken cToken = default)
        {
            // TODO: What to do with cToken here?
            TaskCompletionSource tcs = new TaskCompletionSource();

            Terminal.InitHeader("Not Implemented", "Archiver");
            Terminal.InitStatusBar(
                new StatusBarItem(
                    "Main Menu",
                    (key) => { 
                        tcs.TrySetResult();
                        return Task.CompletedTask;
                    },
                    Key.MakeKey(ConsoleKey.Q)
                )
            );

            Terminal.Clear();
            Formatting.WriteLineC(ConsoleColor.Red, "This operation has not yet been implemented.");

            await tcs.Task;
        }
    }
}