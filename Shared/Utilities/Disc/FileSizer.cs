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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;

namespace Archiver.Shared.Utilities.Disc
{
    public class FileSizer
    {
        public delegate void ProgressChangedDelegate(DiscScanStats stats, long currentFile);

        public event ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 250;
        private Stopwatch _sw;
        private long _lastSample;
        private DiscScanStats _stats;

        public FileSizer(DiscScanStats stats)
        {
            _stats = stats ?? throw new System.ArgumentNullException(nameof(stats));
            _sw = new Stopwatch();

            this.OnProgressChanged += delegate { };
        }

        public Task SizeFilesAsync(CancellationToken cToken)
            => Task.Run(() => SizeFiles(cToken));

        public void SizeFiles(CancellationToken cToken = default)
        {
            _sw.Start();

            long fileCount = 0;

            IEnumerable<DiscSourceFile> files = _stats.SourceFileDict
                                                      .Select(x => x.Value)
                                                      .Where(x => x.Archived == false);

            foreach (DiscSourceFile sourceFile in files)
            {
                if (cToken.IsCancellationRequested)
                    return;

                sourceFile.ReadSizeAndAttribs(_stats);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(_stats, fileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                fileCount++;
            }

            OnProgressChanged(_stats, fileCount);

            _sw.Stop();
        }
    }
}