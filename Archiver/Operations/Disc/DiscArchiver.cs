/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020leftWidth21 Steve Cross <flip@foxhollow.cc>
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Operations.Disc
{
    internal static class DiscArchiver
    {
        private static CancellationTokenSource _cts;

        private static Stopwatch _sw;

        internal static async Task RunArchiveAsync(bool askBeforeArchive = false)
        {
            // TODO: handle cancel during disc index read in other tasks
            List<DiscDetail> discs = await Helpers.ReadDiscIndexAsync();

            if (discs == null)
                return;

            DiscScanStats stats = new DiscScanStats(discs);
            Status.SetStats(stats);

            _sw = Stopwatch.StartNew();

            SetupUI();

            // ShowAll();

            await IndexAndCountFilesAsync(stats);

            if (_cts.Token.IsCancellationRequested) return;

            if (stats.NewlyFoundFiles > 0)
            {
                await SizeFilesAsync(stats);

                if (_cts.Token.IsCancellationRequested) return;

                await DistributeFilesAsync(stats);

                if (_cts.Token.IsCancellationRequested) return;

                bool doProcess = true;

                if (askBeforeArchive)
                {
                    bool? result = await _ynqRunArchive.QueryAsync(_cts.Token);

                    if (result == null) return;

                    doProcess = result.Value;

                    _ynqRunArchive.Hide();
                }
                

            //     bool doProcess = true;

            //     List<DiscDetail> newDiscs = stats.DestinationDiscs
            //                                      .Where(x => x.NewDisc == true)
            //                                      .ToList();

            //     Console.WriteLine();
            //     Console.WriteLine();
            //     Console.WriteLine($"    New files found: {stats.NewlyFoundFiles.ToString("N0")}");
            //     Console.WriteLine($"New Discs to Create: {newDiscs.Count}");
            //     Console.WriteLine();

                // if (askBeforeArchive)
                // {
                //     Console.Write("Do you want to run the archive process now? (yes/");
                //     Formatting.WriteC(ConsoleColor.Blue, "NO");
                //     Console.Write(") ");

                //     Console.CursorVisible = true;
                //     string response = Console.ReadLine();
                //     Console.CursorVisible = false;

                //     int endLine = Console.CursorTop;

                //     doProcess = response.ToLower().StartsWith("yes");
                //     Console.WriteLine();
                //     Console.WriteLine();
                // }

                // if (doProcess)
                // {
                //     DiscProcessing.ProcessDiscs(stats);

                //     Formatting.WriteLineC(ConsoleColor.Green, "Process complete... don't forget to burn the ISOs to disc!");
                // }
            }
            else
            {
                // Status.ProcessComplete();

                // Console.WriteLine("No new files found to archive. Nothing to do.");
            }

            _sw.Stop();
        }

        internal static Task StartScanOnlyAsync()
            => RunArchiveAsync(true);

        internal static Task StartOperationAsync()
            => RunArchiveAsync(false);


        private static KeyValueText _kvtStatus;
        private static KeyValueText _kvtElapsed;
        private static KeyValueText _kvtNewFileCount;
        private static KeyValueText _kvtExistingFileCount;
        private static KeyValueText _kvtExcludedFileCount;
        private static KeyValueText _kvtSize;
        private static KeyValueText _kvtDistribute;
        private static ProgressBar _progressSize;
        private static ProgressBar _progressDistribute;
        private static QueryYesNo _ynqRunArchive;
        private const int leftWidth = -15;


        private static void SetupUI()
        {
            _cts = new CancellationTokenSource();

            Terminal.InitStatusBar(
                new StatusBarItem(
                    "Cancel",
                    (key) => {
                        _cts.Cancel();
                        return Task.CompletedTask;
                    },
                    Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                )
            );

            Terminal.Clear();
            Terminal.Header.UpdateLeft("Disc Archiver");

            _kvtStatus = new KeyValueText("Status", "Starting...", leftWidth);
            Terminal.NextLine();
            _kvtElapsed = new KeyValueText("Elapsed", null, leftWidth);
            Terminal.NextLine();
            Terminal.NextLine();

            _kvtNewFileCount = new KeyValueText("New Files", "0", leftWidth);
            Terminal.NextLine();

            _kvtExistingFileCount = new KeyValueText("Existing Files", "0", leftWidth);
            Terminal.NextLine();

            _kvtExcludedFileCount = new KeyValueText("Excluded Files", "0", leftWidth);
            Terminal.NextLine();
            Terminal.NextLine();

            _kvtSize = new KeyValueText("New Data Size", "0", leftWidth);
            Terminal.NextLine();
            _kvtDistribute = new KeyValueText("New Disc Count", "0", leftWidth);
            Terminal.NextLine();

            _progressSize = new ProgressBar(mode: ProgressMode.ExplicitCountLeft);
            Terminal.NextLine();
            _progressDistribute = new ProgressBar(mode: ProgressMode.ExplicitCountLeft);
            Terminal.NextLine();

            _ynqRunArchive = new QueryYesNo("Do you want to run the archive process now?");
            Terminal.NextLine();

            _kvtStatus.Show();
            _kvtElapsed.Show();
        }

        private static void ShowAll()
        {
            _kvtStatus.Show();
            _kvtElapsed.Show();
            _kvtNewFileCount.Show();
            _kvtExistingFileCount.Show();
            _kvtExcludedFileCount.Show();
            _kvtSize.Show();
            _progressSize.Show();
        }

        private static void UpdateStats(DiscScanStats stats) 
        {
            _kvtElapsed.UpdateValue(Formatting.FormatElapsedTime(_sw.Elapsed));
            _kvtNewFileCount.UpdateValue(stats.NewlyFoundFiles.ToString());
            _kvtExistingFileCount.UpdateValue(stats.ExistingFilesArchived.ToString());
            _kvtExcludedFileCount.UpdateValue(stats.ExcludedFileCount.ToString());
        }

        private static void SetStatus(string text)
            => _kvtStatus.UpdateValue(text);

        private static async Task IndexAndCountFilesAsync(DiscScanStats stats)
        {
            _kvtNewFileCount.Show();
            _kvtExistingFileCount.Show();
            _kvtExcludedFileCount.Show();

            SetStatus("Scanning for files");

            FileScanner scanner = new FileScanner(stats);

            scanner.OnProgressChanged += (newFiles, existingFiles, excludedFiles) => UpdateStats(stats);

            await scanner.ScanFilesAsync(_cts.Token);
            UpdateStats(stats);
        }

        public static async Task SizeFilesAsync(DiscScanStats stats)
        {
            _kvtSize.Show();
            _progressSize.UpdateProgress(0, stats.NewlyFoundFiles, true);

            SetStatus("Sizing files");

            FileSizer sizer = new FileSizer(stats);

            FileSizer.ProgressChangedDelegate updateSize = (stats, currentFile, totalSize) => {
                _kvtSize.UpdateValue(Formatting.GetFriendlySize(totalSize));
                _progressSize.UpdateProgress(currentFile, stats.NewlyFoundFiles);
            };

            sizer.OnProgressChanged += updateSize;

            await sizer.SizeFilesAsync(_cts.Token);

            updateSize(stats, stats.NewlyFoundFiles, stats.TotalSize);

            _progressSize.Hide();
        }

        public static async Task DistributeFilesAsync(DiscScanStats stats)
        {
            _kvtDistribute.Show();
            _progressDistribute.UpdateProgress(0, stats.NewlyFoundFiles, true);

            SetStatus("Sizing files");

            FileDistributor distributor = new FileDistributor(stats);

            FileDistributor.ProgressChangedDelegate updateDistribute = (stats, currentFile, discCount) => {
                _kvtDistribute.UpdateValue($"{discCount} discs");
                _progressDistribute.UpdateProgress(currentFile, stats.NewlyFoundFiles);
            };

            distributor.OnProgressChanged += updateDistribute;

            await distributor.DistributeFilesAsync(_cts.Token);

            updateDistribute(stats, stats.NewlyFoundFiles, stats.DestinationDiscs.Where(x => x.Finalized == false).Count());

            _progressDistribute.Hide();
        }
    }
}