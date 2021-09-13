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

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;

namespace Archiver.Utilities.Disc
{
    public class FileDistributor
    {
        public delegate void ProgressChangedDelegate(DiscScanStats stats, long currentFile, int discCount);

        public event ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 250;
        private Stopwatch _sw;
        private long _lastSample;
        private DiscScanStats _stats;

        public FileDistributor(DiscScanStats stats)
        {
            _stats = stats ?? throw new System.ArgumentNullException(nameof(stats));
            _sw = new Stopwatch();

            this.OnProgressChanged += delegate { };
        }

        public Task DistributeFilesAsync(CancellationToken ctoken)
            => Task.Run(() => this.DistributeFiles(ctoken));

        public void DistributeFiles(CancellationToken cToken = default)
        {
            _sw.Start();

            long fileCount = 0;

            foreach (DiscSourceFile sourceFile in _stats.DiscSourceFiles.Where(x => x.Archived == false).OrderByDescending(x => x.Size))
            {
                if (cToken.IsCancellationRequested)
                    return;

                sourceFile.AssignDisc(_stats);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    int discCount = _stats.DestinationDiscs.Where(x => x.Finalized == false).Count();
                    OnProgressChanged(_stats, fileCount, discCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                fileCount++;
            }

            _sw.Stop();
        }
    }
}