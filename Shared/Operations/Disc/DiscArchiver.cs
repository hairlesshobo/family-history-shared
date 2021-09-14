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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;
using Archiver.Shared.Utilities.Disc;

namespace Archiver.Shared.Operations.Disc
{
    public class DiscArchiver
    {
        public enum ProcessStep
        {
            FirstStart = 0,
            ScanFiles = 1,
            SizeFiles = 2,
            DistributeFiles = 3,
            CopyFiles = 4
        }

        public enum Status
        {
            NothingToDo = 0,
            Completed = 1,
            Canceled = 2
        }

        public struct FileCopyProgress
        {
            public TimeSpan Elapsed;
            public int CurrentFile;
            public double InstantTransferRate;
            public double AverageTransferRate;
            public double CurrentPercent;
        }

        public delegate void StepStateDelegate(DiscDetail disc, DiscScanStats stats, ProcessStep step);
        public delegate void UpdateStatsDelegate(DiscScanStats stats);
        public delegate void UpdateSizingDelegate(DiscScanStats stats, long currentFile);
        public delegate void UpdateDistributeDelegate(DiscScanStats stats, long currentFile);
        public delegate void FileCopyProgressDelegate(DiscDetail disc, DiscScanStats stats, FileCopyProgress progress);
        // public delegate void StatusUpdateDelegate(string statusText);

        public event StepStateDelegate OnStepStart;
        public event StepStateDelegate OnStepComplete;
        // public event StatusUpdateDelegate OnStatusUpdate;
        public event UpdateStatsDelegate OnUpdateStats;
        public event UpdateSizingDelegate OnUpdateSizing;
        public event UpdateDistributeDelegate OnUpdateDistribute;
        public event FileCopyProgressDelegate OnFileCopyProgress;

        public Func<DiscScanStats, Task<Nullable<bool>>> AskToArchiveCallback = null;

        public bool Canceled => _cToken.IsCancellationRequested;

        public Status ResultStatus { get; private set; } = DiscArchiver.Status.NothingToDo;

        private List<DiscDetail> _discs;
        private DiscScanStats _stats;
        private CancellationToken _cToken = default;
        private const int _updateFrequencyMs = 1000;

        public DiscArchiver(List<DiscDetail> discs)
        {
            _discs = discs ?? throw new ArgumentNullException(nameof(discs));
            _stats = new DiscScanStats(_discs);

            this.OnStepStart += delegate { };
            this.OnStepComplete += delegate { };
            // this.OnStatusUpdate += delegate { };
            this.OnUpdateStats += delegate { };
            this.OnUpdateSizing += delegate { };
            this.OnUpdateDistribute += delegate { };
            this.OnFileCopyProgress += delegate { };
        }

        public async Task RunArchiveAsync(bool askBeforeArchive = false, CancellationToken cToken = default)
        {
            if (askBeforeArchive && AskToArchiveCallback == null)
                throw new InvalidOperationException("When 'askBeforeArchive' is true, an 'AskToArchiveCallback' must be provided!");

            _stats.ProcessSw.Restart();

            _cToken = cToken;
            
            
            await this.IndexAndCountFilesAsync();
            if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }

            if (_stats.NewlyFoundFiles == 0) { this.ResultStatus = Status.NothingToDo; return; }

            await this.SizeFilesAsync();
            if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }

            await this.DistributeFilesAsync();
            if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }

            if (askBeforeArchive && AskToArchiveCallback != null && await AskToArchiveCallback(_stats) != true)
            { this.ResultStatus = Status.Canceled; return; }

            await this.ProcessDiscsAsync();
            if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }

            this.ResultStatus = Status.Completed;

            _stats.ProcessSw.Stop();
        }

        private async Task IndexAndCountFilesAsync()
        {
            this.OnStepStart(null, _stats, ProcessStep.ScanFiles);
            // this.OnStatusUpdate("Scanning for files");

            FileScanner scanner = new FileScanner(_stats);
            scanner.OnProgressChanged += (stats) => this.OnUpdateStats(stats);
            await scanner.ScanFilesAsync(_cToken);

            this.OnStepComplete(null, _stats, ProcessStep.ScanFiles);
        }

        private async Task SizeFilesAsync()
        {
            this.OnStepStart(null, _stats, ProcessStep.SizeFiles);
            // this.OnStatusUpdate("Sizing files");

            FileSizer sizer = new FileSizer(_stats);
            sizer.OnProgressChanged += (stats, currentFile) => this.OnUpdateSizing(stats, currentFile);
            await sizer.SizeFilesAsync(_cToken);

            this.OnStepComplete(null, _stats, ProcessStep.SizeFiles);
        }

        private async Task DistributeFilesAsync()
        {
            this.OnStepStart(null, _stats, ProcessStep.DistributeFiles);

            FileDistributor distributor = new FileDistributor(_stats);

            distributor.OnProgressChanged += (stats, currentFile) => this.OnUpdateDistribute(stats, currentFile);

            await distributor.DistributeFilesAsync(_cToken);

            this.OnStepComplete(null, _stats, ProcessStep.DistributeFiles);
        }

        private async Task ProcessDiscsAsync()
        {
            List<DiscDetail> discsToProcess = _stats.DestinationDiscs.Where(x => x.NewDisc == true)
                                                                     .OrderBy(x => x.DiscNumber)
                                                                     .ToList();

            foreach (DiscDetail disc in discsToProcess)
            {
                Stopwatch discSw = Stopwatch.StartNew();

                await CopyFilesAsync(disc, discSw);
                if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }
                
                // GenerateIndexFiles(disc, masterSw);
                // if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }
                
                // GenerateHashFile(disc, masterSw);
                // if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }
                
                // WriteDiscInfo(disc, masterSw);
                // if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }
                
                // CreateISOFile(disc, masterSw);
                // if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }
                
                // ReadIsoHash(disc, masterSw);
                // if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }

                // SaveJsonData(disc, masterSw);
                // if (_cToken.IsCancellationRequested) { this.ResultStatus = Status.Canceled; return; }

                discSw.Stop();
                // Status.WriteDiscComplete(disc, masterSw.Elapsed);
            }

            // Status.ProcessComplete();
        }

        private async Task CopyFilesAsync(DiscDetail disc, Stopwatch discSw)
        {
            this.OnStepStart(disc, _stats, ProcessStep.CopyFiles);
            
            // TODO: implement cToken

            // if the stage dir already exists, we need to remove it so we don't accidentally end up with data
            // on the final disc that doesn't belong there
            // TODO: find some way to make sure this doesn't delete stuff it shouldn't..
            if (Directory.Exists(disc.RootStagingPath))
                Directory.Delete(disc.RootStagingPath, true);

            disc.ArchiveDTM = DateTime.UtcNow;

            IEnumerable<DiscSourceFile> sourceFiles = disc.Files.Where(x => x.Archived == false).OrderBy(x => x.RelativePath);

            long bytesCopied = 0;
            long bytesCopiedSinceLastupdate = 0;
            int currentFile = 1;
            int currentDisc = 0;
            double averageTransferRate = 0;
            long sampleCount = 0;
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            long lastSample = sw.ElapsedMilliseconds;

            foreach (DiscSourceFile file in sourceFiles)
            {
                if (_cToken.IsCancellationRequested == true)
                    return;

                // if we moved to another disc, we reset the disc counter
                if (file.DestinationDisc.DiscNumber > currentDisc)
                {
                    currentDisc = file.DestinationDisc.DiscNumber;
                    currentFile = 1;
                }

                var copier = file.ActivateCopy();

                copier.OnProgressChanged += (progress) => {
                    bytesCopied += progress.BytesCopiedSinceLastupdate;
                    bytesCopiedSinceLastupdate += progress.BytesCopiedSinceLastupdate;
                    file.DestinationDisc.BytesCopied += progress.BytesCopiedSinceLastupdate;

                    if ((sw.ElapsedMilliseconds - lastSample) > _updateFrequencyMs)
                    {
                        sampleCount++;

                        double timeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastSample) / 1000.0;
                        double instantTransferRate = (double)bytesCopiedSinceLastupdate / timeSinceLastUpdate;

                        if (sampleCount == 1)
                            averageTransferRate = instantTransferRate;
                        else
                            averageTransferRate = averageTransferRate + (instantTransferRate - averageTransferRate) / sampleCount;

                        this.OnFileCopyProgress(disc, _stats, new FileCopyProgress()
                        {
                            Elapsed = discSw.Elapsed,
                            CurrentFile = currentFile, 
                            InstantTransferRate = instantTransferRate,
                            AverageTransferRate = averageTransferRate,
                            CurrentPercent = ((double)disc.BytesCopied / (double)disc.DataSize)
                        });

                        bytesCopiedSinceLastupdate = 0;
                        lastSample = sw.ElapsedMilliseconds;
                    }
                };

                copier.OnComplete += (progress) => {
                    file.Hash = copier.MD5_Hash;
                };

                // TODO: port to task instead of thread
                await copier.CopyAsync(_cToken);

                file.Copied = true;
                file.Archived = true;
                file.ArchiveTimeUtc = DateTime.UtcNow;

                currentFile++;
            }

            sw.Stop();

            this.OnStepComplete(disc, _stats, ProcessStep.CopyFiles);
        }
    }
}