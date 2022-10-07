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
using FoxHollow.Archiver.CLI.Utilities.Shared;
using FoxHollow.Archiver.Shared;
using FoxHollow.Archiver.Shared.Utilities;
using FoxHollow.TerminalUI;
using FoxHollow.TerminalUI.Elements;
using FoxHollow.TerminalUI.Types;
using static FoxHollow.Archiver.Shared.Utilities.Formatting;

namespace FoxHollow.Archiver.CLI.Tasks.Universal
{
    public static class MainMenu
    {
        public async static Task StartTaskAsync(CancellationTokenSource cts)
            => await ShowMenuAsync(cts);

        private async static Task ShowMenuAsync(CancellationTokenSource cts)
        {
            while (!cts.IsCancellationRequested)
            {
                Terminal.Header.UpdateHeader("Main Menu", "Archiver");
                Terminal.Clear();

                Menu menu = BuildMenu();
                menu.QuitCallback = () => {
                    cts.Cancel();
                    return Task.CompletedTask;
                };

                var results = await menu.ShowAsync(cts.Token);

                if (results == null)
                    return;

                if (results.First() == null)
                    continue;

                if ((bool)results.First() == false)
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

        private static Menu BuildMenu()
        {
            List<MenuEntry> entries = new List<MenuEntry>();

            entries.AddRange(BuildDiscMenu());
            entries.Add(new MenuEntry() { Header = true });
            entries.AddRange(BuildTapeMenu());
            entries.Add(new MenuEntry() { Header = true });
            entries.AddRange(BuildCsdMenu());
            entries.Add(new MenuEntry() { Header = true });
            entries.AddRange(BuildUniversalMenu());

            return new Menu(entries);
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
                    Disabled = !SysInfo.IsOpticalDrivePresent || true, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                new MenuEntry() {
                    Name = "View Archive Summary",
                    Task = Tasks.Disc.DiscArchiveSummaryTask.StartTaskAsync,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new MenuEntry() {
                    Name = "View disc summary",
                    Task = NotImplementedAsync, // Tasks.Disc.ShowDiscSummaryTask.StartTaskAsync,
                    Disabled = !SysInfo.IsOpticalDrivePresent || true, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.Blue
                },
                new MenuEntry() {
                    Name = "Verify Discs",
                    Task = Tasks.Disc.DiscVerificationTask.StartTaskAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsOpticalDrivePresent,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new MenuEntry() {
                    Name = "Scan For Changes",
                    Task = Tasks.Disc.DiscArchiverTask.StartScanOnlyTaskAsync,
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new MenuEntry() {
                    Name = "Scan For Renamed/Moved Files",
                    Task = NotImplementedAsync, // Tasks.Disc.ScanForFileRenamesTask.StartTaskAsync,
                    Disabled = true, // TODO: Remove once implemented
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new MenuEntry() {
                    Name = "Run Archive process",
                    Task = Tasks.Disc.DiscArchiverTask.StartTaskAsync,
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
                    Task = NotImplementedAsync, // Tasks.Tape.RestoreToTarTask.StartTaskAsync,
                    Disabled = !SysInfo.IsTapeDrivePresent || true, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                //! not implemented
                new MenuEntry() {
                    Name = "Restore entire tape (to original file structure)",
                    Task = NotImplementedAsync, // Tasks.Taepe.RestoreToFilesTask.StartTaskAsync,
                    Disabled = !SysInfo.IsTapeDrivePresent || true, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                new MenuEntry() {
                    Name = "Read Tape Summary",
                    Task = Tasks.Tape.ShowTapeSummaryTask.StartTaskAsync,
                    Disabled = !SysInfo.IsTapeDrivePresent,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new MenuEntry() {
                    Name = "View Archive Summary",
                    Task = Tasks.Tape.ShowArchiveSummaryTask.StartTaskAsync,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                new MenuEntry() {
                    Name = "Verify Tape",
                    Task = Tasks.Tape.TapeVerificationTask.StartTaskAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsTapeDrivePresent,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new MenuEntry() {
                    Name = "Run tape archive",
                    Task = Tasks.Tape.ArchiverTask.StartTaskAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem || !SysInfo.IsTapeDrivePresent || true, // TODO: Remove once converted
                    ForegroundColor = ConsoleColor.Red
                }
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
                    Task = Tasks.CSD.RegisterDriveTask.StartTaskAsync,
                    ForegroundColor = ConsoleColor.Green
                },
                //! not implemented
                new MenuEntry() {
                    Name = "Restore entire CSD Drive",
                    Task = NotImplementedAsync,
                    Disabled = true, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.Green
                },
                new MenuEntry() {
                    Name = "View CSD Archive Summary",
                    Task = Tasks.CSD.ShowArchiveSummaryTask.StartTaskAsync,
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                //! not implemented
                new MenuEntry() {
                    Name = "View CSD Drive Summary",
                    Task = NotImplementedAsync, // Tasks.CSD.ShowDriveSummaryTask.StartTaskAsync,         
                    Disabled = true, // TODO: remove once implemented
                    SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = ConsoleColor.Blue
                },
                //! not implemented
                new MenuEntry() {
                    Name = "Verify CSD Drive",
                    Task = NotImplementedAsync, // Tasks.CSD.VerifyTask.StartTaskAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem || true, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.DarkYellow
                },
                new MenuEntry() {
                    Name = "Clean CSD Drive - Remove files not in index",
                    Task = NotImplementedAsync, // Tasks.CSD.CleanerTask.StartTaskAsync,
                    Disabled = SysInfo.IsReadonlyFilesystem, // TODO: remove once implemented
                    ForegroundColor = ConsoleColor.Red
                },
                new MenuEntry() {
                    Name = "Run CSD Archive Process",
                    Task = Tasks.CSD.ArchiverTask.StartTaskAsync,
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
                //! not implemented
                new MenuEntry() {
                    Name = "Copy Tools to Local Disk",
                    Task = NotImplementedAsync,
                    Disabled = !SysInfo.IsReadonlyFilesystem || true // TODO: Remove once implemented 
                },
                new MenuEntry() {
                    Name = "Create Index ISO",
                    Task = NotImplementedAsync, // Helpers.CreateIndexIso,
                    Disabled = SysInfo.IsReadonlyFilesystem || true // TODO: Remove once implemented
                }//,
                // new MenuEntry() {
                //     Name = "Explode",
                //     Task = (cToken) => throw new InvalidOperationException("meow!!!!")
                // }
            };
        }

        private static async Task NotImplementedAsync(CancellationToken cToken = default)
        {
            // TODO: What to do with cToken here?
            TaskCompletionSource tcs = new TaskCompletionSource();

            Terminal.Header.UpdateHeader("Not Implemented", "Archiver");
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

            Terminal.StatusBar.Reset();
        }
    }
}