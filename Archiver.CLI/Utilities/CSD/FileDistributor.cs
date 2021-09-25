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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FoxHollow.Archiver.Shared.Classes.CSD;

namespace FoxHollow.Archiver.CLI.Utilities.CSD
{
    public delegate void Distributor_ProgressChangedDelegate(long currentFile, int csdCount, double filesPerSecond);
    public delegate void Distributor_CompleteDelegate();

    public class FileDistributor
    {
        public event Distributor_ProgressChangedDelegate OnProgressChanged;
        public event Distributor_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 1000;
        private Stopwatch _sw;
        private CsdScanStats _stats;

        public FileDistributor(CsdScanStats stats)
        {
            _stats = stats ?? throw new System.ArgumentNullException(nameof(stats));
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void DistributeFiles()
        {
            _sw.Start();

            long fileCount = 0;
            long lastSample = _sw.ElapsedMilliseconds;
            long lastSampleFileCount = 0;

            List<CsdSourceFile> files = _stats.SourceFileDict
                                              .Select(x => x.Value)
                                              .Where(x => x.Copied == false)
                                              .OrderByDescending(x => x.Size)
                                              .ToList();

            foreach (CsdSourceFile sourceFile in files)
            {
                sourceFile.AssignCsd(_stats);

                if (_sw.ElapsedMilliseconds - lastSample > _sampleDurationMs)
                {
                    long filesSinceLastSample = fileCount - lastSampleFileCount;
                    long elapsedSinceLastSample =  _sw.ElapsedMilliseconds - lastSample;
                    lastSample = _sw.ElapsedMilliseconds;

                    double filesPerSecond = (double)filesSinceLastSample / ((double)elapsedSinceLastSample / 1000.0);

                    int csdCount = _stats.DestinationCsds.Where(x => x.HasPendingWrites == true).Count();
                    OnProgressChanged(fileCount, csdCount, filesPerSecond);

                    lastSampleFileCount = fileCount;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}