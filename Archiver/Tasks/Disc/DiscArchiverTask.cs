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
using Archiver.Shared.Operations.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Tasks.Disc
{
    internal static class DiscArchiverTask
    {
        private static CancellationTokenSource _cts;

        internal static async Task RunArchiveAsync(bool askBeforeArchive = false)
        {
            // TODO: handle cancel during disc index read in other tasks
            List<DiscDetail> discs = await Helpers.ReadDiscIndexAsync();

            if (discs == null)
                return;

            SetupUI();

            DiscArchiver archiver = new DiscArchiver(discs);

            archiver.OnUpdateStats += (stats) => {
                _kvtElapsed.UpdateValue(Formatting.FormatElapsedTime(stats.ProcessSw.Elapsed));
                _kvtNewFileCount.UpdateValue(stats.NewlyFoundFiles.ToString());
                _kvtExistingFileCount.UpdateValue(stats.ExistingFilesArchived.ToString());
                _kvtExcludedFileCount.UpdateValue(stats.ExcludedFileCount.ToString());
            };

            archiver.OnStepStart += (stats, step) => {
                if (step == DiscArchiver.ProcessStep.ScanFiles)
                {
                    _kvtNewFileCount.Show();
                    _kvtExistingFileCount.Show();
                    _kvtExcludedFileCount.Show();

                    SetStatus("Scanning for files");
                }
                else if (step == DiscArchiver.ProcessStep.SizeFiles)
                {
                    _kvtSize.Show();
                    _progress.UpdateProgress(0, stats.NewlyFoundFiles, true);

                    SetStatus("Sizing files");
                }
                else if (step == DiscArchiver.ProcessStep.DistributeFiles)
                {
                    _kvtDistribute.Show();
                    _progress.UpdateProgress(0, stats.NewlyFoundFiles, true);

                    SetStatus("Distributing files");
                }
            };

            archiver.OnStepComplete += (stats, step) => {
                if (step == DiscArchiver.ProcessStep.SizeFiles)
                    _progress.Hide();
                else if (step == DiscArchiver.ProcessStep.DistributeFiles)
                    _progress.Hide();
            };

            archiver.OnUpdateSizing += (stats, currentFile) => {
                _kvtSize.UpdateValue(Formatting.GetFriendlySize(stats.TotalSize));
                _progress.UpdateProgress(currentFile, stats.NewlyFoundFiles);
            };

            archiver.OnUpdateDistribute += (stats, currentFile) => {
                _kvtDistribute.UpdateValue($"{stats.NewDiscCount} discs");
                _progress.UpdateProgress(currentFile, stats.NewlyFoundFiles);
            };

            archiver.AskToArchiveCallback = async (stats) => {
                bool? result = await _ynqRunArchive.QueryAsync(_cts.Token);

                _ynqRunArchive.Hide();

                return result;
            };

            await archiver.RunArchiveAsync(askBeforeArchive, _cts.Token);

            // ShowAll();

            if (archiver.ResultStatus == DiscArchiver.Status.Completed)
                Terminal.WriteLineColor(ConsoleColor.Green, "Process complete... don't forget to burn the ISOs to disc!");
            else if (archiver.ResultStatus == DiscArchiver.Status.NothingToDo)
                Terminal.WriteLine("No new files found to archive. Nothing to do.");
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
        private static ProgressBar _progress;
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
            Terminal.NextLine();

            _progress = new ProgressBar(mode: ProgressMode.ExplicitCountLeft);
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
            _kvtDistribute.Show();
            _progress.Show();
        }

        private static void SetStatus(string text)
            => _kvtStatus.UpdateValue(text);

    }
}