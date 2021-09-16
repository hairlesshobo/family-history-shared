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
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Operations.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;
using TerminalUI.Types;

namespace Archiver.Tasks.Disc
{
    internal static class DiscArchiverTask
    {
        private static CancellationTokenSource _cts;

        private static void UpdateElapsed(DiscScanStats stats)
            => _kvtElapsed.UpdateValue(Formatting.FormatElapsedTime(stats.ProcessSw.Elapsed));

        private static async Task RunArchiveAsync(bool askBeforeArchive, CancellationToken cToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cToken);

            // TODO: handle cancel during disc index read in other tasks
            List<DiscDetail> discs = await Helpers.ReadDiscIndexAsync(_cts.Token);

            if (discs == null)
                return;

            SetupUI();

            DiscArchiver archiver = new DiscArchiver(discs);

            archiver.OnUpdateStats += (stats) => {
                UpdateElapsed(stats);

                _kvtNewFileCount.UpdateValue(stats.NewlyFoundFiles.ToString());
                _kvtExistingFileCount.UpdateValue(stats.ExistingFilesArchived.ToString());
                _kvtExcludedFileCount.UpdateValue(stats.ExcludedFileCount.ToString());
            };

            archiver.OnStepStart += (disc, stats, step) => {
                UpdateElapsed(stats);
                
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
                else if (step == DiscArchiver.ProcessStep.CopyFiles)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    _progress.SetMode(ProgressMode.ExplicitCountLeft);
                    _progress.UpdateProgress(0, disc.TotalFiles, true);
                    _kvtDiscName.UpdateValue(disc.DiscName);
                    _kvtDiscName.Show();
                    _kvtElapsedTime.Show();
                    _kvtDataCopied.Show();
                    _kvtCurrentRate.Show();
                    _kvtAvgRate.Show();

                    SetStatus("Copying files to staging");
                }
                else if (step == DiscArchiver.ProcessStep.CreateDiscIndex)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    _progress.SetMode(ProgressMode.ExplicitCountLeft);
                    _progress.UpdateProgress(0, disc.TotalFiles, true);
                    _kvtDiscName.UpdateValue(disc.DiscName);
                    _kvtDiscName.Show();
                    _kvtElapsedTime.Show();

                    SetStatus("Generating disc index");
                }
                else if (step == DiscArchiver.ProcessStep.GenerateHashFile)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    _progress.SetMode(ProgressMode.ExplicitCountLeft);
                    _progress.UpdateProgress(0, disc.TotalFiles, true);
                    _kvtDiscName.UpdateValue(disc.DiscName);
                    _kvtDiscName.Show();
                    _kvtElapsedTime.Show();

                    SetStatus("Generating disc hashlist");
                }
                else if (step == DiscArchiver.ProcessStep.WriteDiscInfo)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    _progress.SetMode(ProgressMode.ExplicitCountLeft);
                    _progress.UpdateProgress(0, disc.TotalFiles, true);
                    _kvtDiscName.UpdateValue(disc.DiscName);
                    _kvtDiscName.Show();
                    _kvtElapsedTime.Show();

                    SetStatus("Write disc info file");
                }
                else if (step == DiscArchiver.ProcessStep.CreateISOFile)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    _progress.SetMode(ProgressMode.Default);
                    _progress.UpdateProgress(0);
                    _kvtDiscName.UpdateValue(disc.DiscName);
                    _kvtDiscName.Show();
                    _kvtElapsedTime.Show();

                    SetStatus("Creating ISO");
                }
                else if (step == DiscArchiver.ProcessStep.ReadIsoHash)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    _progress.SetMode(ProgressMode.Default);
                    _progress.UpdateProgress(0.0);
                    _kvtDiscName.UpdateValue(disc.DiscName);
                    
                    _kvtDiscName.Show();
                    _kvtElapsedTime.Show();
                    _kvtDataCopied.Show();
                    _kvtCurrentRate.Show();
                    _kvtAvgRate.Show();

                    SetStatus("Reading ISO MD5 Hash");
                }
                else if (step == DiscArchiver.ProcessStep.SaveToIndex)
                {
                    _textDiscProcessHeader.Show();
                    _lineProcessingHeader.Show();

                    SetStatus("Saving disc to index");
                }
                else if (step == DiscArchiver.ProcessStep.DiscComplete)
                {
                    _textDiscProcessHeader.Hide();
                    _lineProcessingHeader.Hide();

                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                    _kvtDataCopied.Hide();
                    _kvtCurrentRate.Hide();
                    _kvtAvgRate.Hide();

                    SetStatus($"Disc {disc.DiscNumber} complete");

                    UpdateDiscCount(stats);
                }
            };

            archiver.OnStepComplete += (disc, stats, step) => {
                UpdateElapsed(stats);
                
                if (step == DiscArchiver.ProcessStep.ScanFiles)
                {

                }
                else if (step == DiscArchiver.ProcessStep.SizeFiles)
                    _progress.Hide();
                else if (step == DiscArchiver.ProcessStep.DistributeFiles)
                {
                    _progress.Hide();
                    UpdateDiscCount(stats);
                }
                else if (step == DiscArchiver.ProcessStep.CopyFiles)
                {
                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                    _kvtDataCopied.Hide();
                    _kvtCurrentRate.Hide();
                    _kvtAvgRate.Hide();
                }
                else if (step == DiscArchiver.ProcessStep.CreateDiscIndex)
                {
                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                }
                else if (step == DiscArchiver.ProcessStep.GenerateHashFile)
                {
                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                }
                else if (step == DiscArchiver.ProcessStep.WriteDiscInfo)
                {
                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                }
                else if (step == DiscArchiver.ProcessStep.CreateISOFile)
                {
                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                }
                else if (step == DiscArchiver.ProcessStep.ReadIsoHash)
                {
                    _progress.Hide();
                    _kvtDiscName.Hide();
                    _kvtElapsedTime.Hide();
                    _kvtDataCopied.Hide();
                    _kvtCurrentRate.Hide();
                    _kvtAvgRate.Hide();
                }
            };

            archiver.OnUpdateSizing += (stats, currentFile) => {
                UpdateElapsed(stats);
                
                _kvtSize.UpdateValue(Formatting.GetFriendlySize(stats.TotalSize));
                _progress.UpdateProgress(currentFile, stats.NewlyFoundFiles);
            };

            archiver.OnUpdateDistribute += (stats, currentFile) => {
                UpdateElapsed(stats);
                
                _kvtDistribute.UpdateValue($"{stats.NewDiscCount} discs");
                _progress.UpdateProgress(currentFile, stats.NewlyFoundFiles);
            };

            archiver.AskToArchiveCallback = async (stats) => {
                bool? result = await _ynqRunArchive.QueryAsync(_cts.Token);

                _ynqRunArchive.Hide();

                return result;
            };

            archiver.OnFileCopyProgress += (disc, stats, progress) => {
                UpdateElapsed(stats);
                
                _progress.UpdateProgress(progress.CurrentFile, disc.TotalFiles, progress.CurrentPercent);
                _kvtElapsedTime.UpdateValue(Formatting.FormatElapsedTime(progress.Elapsed));
                _kvtDataCopied.UpdateValue(Formatting.GetFriendlySize(disc.BytesCopied));
                _kvtCurrentRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.InstantTransferRate));
                _kvtAvgRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.AverageTransferRate));
            };

            archiver.OnDiscIndexProgress += (disc, stats, progress) => {
                UpdateElapsed(stats);
                
                _progress.UpdateProgress(progress.CurrentFile, disc.TotalFiles, progress.CurrentPercent);
                _kvtElapsedTime.UpdateValue(Formatting.FormatElapsedTime(progress.Elapsed));
            };

            archiver.OnGenerateHashProgress += (disc, stats, progress) => {
                UpdateElapsed(stats);
                
                _progress.UpdateProgress(progress.CurrentFile, disc.TotalFiles, progress.CurrentPercent);
                _kvtElapsedTime.UpdateValue(Formatting.FormatElapsedTime(progress.Elapsed));
            };

            archiver.OnCreateIsoProgress += (disc, stats, progress) => {
                UpdateElapsed(stats);
                
                int currentFile = (int)(progress.CurrentPercent * 100);
                _progress.UpdateProgress(currentFile, 100, progress.CurrentPercent);
                _kvtElapsedTime.UpdateValue(Formatting.FormatElapsedTime(progress.Elapsed));
            };

            archiver.OnReadIsoMd5Progress += (disc, stats, elapsed, progress) => {
                UpdateElapsed(stats);
                
                _progress.UpdateProgress(progress.PercentCompleted);
                _kvtElapsedTime.UpdateValue(Formatting.FormatElapsedTime(elapsed));
                _kvtDataCopied.UpdateValue(Formatting.GetFriendlySize(progress.TotalBytesProcessed));
                _kvtCurrentRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.InstantRate));
                _kvtAvgRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.AverageRate));
            };

            await archiver.RunArchiveAsync(askBeforeArchive, _cts.Token);

            if (archiver.ResultStatus == DiscArchiver.Status.Completed)
                Terminal.WriteLineColor(ConsoleColor.Green, "Process complete... don't forget to burn the ISOs to disc!");
            else if (archiver.ResultStatus == DiscArchiver.Status.NothingToDo)
                Terminal.WriteLine("No new files found to archive. Nothing to do.");
            else if (archiver.ResultStatus == DiscArchiver.Status.Canceled)
                Terminal.WriteLineColor(ConsoleColor.Red, "Process canceled! Not all data has been archived!");
        }

        private static void UpdateDiscCount(DiscScanStats stats)
        {
            int newDiscs = stats.NewDiscCount;
            int completeDiscs = newDiscs - stats.DestinationDiscs.Count(x => x.Finalized == false);

            _kvtDiscProgress.UpdateValue($"{completeDiscs} / {newDiscs}");
            _kvtDiscProgress.Show();
        }

        internal static Task StartScanOnlyAsync(CancellationToken cToken = default)
            => RunArchiveAsync(true, cToken);

        internal static Task StartOperationAsync(CancellationToken cToken = default)
            => RunArchiveAsync(false, cToken);


        private static Text _textFileStatsHeader;
        private static Text _textDiscProcessHeader;
        private static HorizontalLine _lineStatsHeader;
        private static HorizontalLine _lineProcessingHeader;
        private static KeyValueText _kvtStatus;
        private static KeyValueText _kvtElapsed;
        private static KeyValueText _kvtNewFileCount;
        private static KeyValueText _kvtExistingFileCount;
        private static KeyValueText _kvtExcludedFileCount;
        private static KeyValueText _kvtSize;
        private static KeyValueText _kvtDistribute;
        private static ProgressBar _progress;
        private static QueryYesNo _ynqRunArchive;
        private static KeyValueText _kvtDiscName;
        private static KeyValueText _kvtElapsedTime;
        private static KeyValueText _kvtDataCopied;
        private static KeyValueText _kvtCurrentRate;
        private static KeyValueText _kvtAvgRate;
        private static KeyValueText _kvtDiscProgress;
        private const int leftWidth = -15;
        private const int rightWidth = -12;


        private static void SetupUI()
        {
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

            // TODO: add "discs x / x completed"

            Terminal.Clear();
            Terminal.Header.UpdateLeft("Disc Archiver");

            _kvtStatus = new KeyValueText("Status", "Starting...", area: TerminalArea.LeftHalf);
            _kvtElapsed = new KeyValueText("Total Time", area: TerminalArea.RightHalf);
            Terminal.NextLine();
            Terminal.NextLine();


            // line 1
            _textFileStatsHeader = new Text(ConsoleColor.Cyan, "Statistics", area: TerminalArea.LeftHalf); //left
            _textDiscProcessHeader = new Text(ConsoleColor.Cyan, "Processing", area: TerminalArea.RightHalf); //right
            Terminal.NextLine();

            // line 2
            _lineStatsHeader = new HorizontalLine(width: -5, area: TerminalArea.LeftHalf);
            _lineProcessingHeader = new HorizontalLine(width: -5, area: TerminalArea.RightHalf);
            Terminal.NextLine();

            // line 3
            _kvtNewFileCount = new KeyValueText("New Files", "0", leftWidth, area: TerminalArea.LeftHalf);
            _kvtDiscName = new KeyValueText("Disc Name", null, rightWidth, area: TerminalArea.RightHalf);
            Terminal.NextLine();

            // line 4
            _kvtExistingFileCount = new KeyValueText("Existing Files", "0", leftWidth, area: TerminalArea.LeftHalf);
            _kvtElapsedTime = new KeyValueText("Elapsed Time", null, rightWidth, area: TerminalArea.RightHalf);
            Terminal.NextLine();

            // line 5
            _kvtExcludedFileCount = new KeyValueText("Excluded Files", "0", leftWidth, area: TerminalArea.LeftHalf);
            Terminal.NextLine();

            // line 6
            _kvtDataCopied = new KeyValueText("Data Copied", Formatting.GetFriendlySize(0), rightWidth, area: TerminalArea.RightHalf);
            Terminal.NextLine();

            // line 7
            _kvtSize = new KeyValueText("New Data Size", "0", leftWidth, area: TerminalArea.LeftHalf);
            _kvtCurrentRate = new KeyValueText("Current Rate", Formatting.GetFriendlyTransferRate(0), rightWidth, area: TerminalArea.RightHalf);
            Terminal.NextLine();

            // line 8
            _kvtDistribute = new KeyValueText("New Disc Count", "0", leftWidth, area: TerminalArea.LeftHalf);
            _kvtAvgRate = new KeyValueText("Average Rate", Formatting.GetFriendlyTransferRate(0), rightWidth, area: TerminalArea.RightHalf);
            Terminal.NextLine();

            // line 9
            Terminal.NextLine();

            // line 10
            _kvtDiscProgress = new KeyValueText("Disc Progress", "0 / 0", leftWidth, area: TerminalArea.LeftHalf);
            Terminal.NextLine();

            // line 11
            Terminal.NextLine();

            // line 12
            _progress = new ProgressBar(mode: ProgressMode.ExplicitCountLeft);
            _ynqRunArchive = new QueryYesNo("Do you want to run the archive process now?");
            Terminal.NextLine();
            Terminal.NextLine();

            _textFileStatsHeader.Show();
            _kvtStatus.Show();
            _kvtElapsed.Show();
            _lineStatsHeader.Show();
        }

        private static void SetStatus(string text)
            => _kvtStatus.UpdateValue(text);
    }
}