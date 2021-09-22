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
using FoxHollow.Archiver.Shared.Classes.CSD;

namespace FoxHollow.Archiver.Utilities.CSD
{
    public delegate void Sizer_ProgressChangedDelegate(long currentFile, long totalSize, double filesPerSecond);
    public delegate void Sizer_CompleteDelegate();

    public class FileSizer
    {
        public event Sizer_ProgressChangedDelegate OnProgressChanged;
        public event Sizer_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 1000;
        private Stopwatch _sw;
        private CsdScanStats _stats;

        public FileSizer(CsdScanStats stats)
        {
            _stats = stats ?? throw new System.ArgumentNullException(nameof(stats));
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void SizeFiles()
        {
            _sw.Start();

            long fileCount = 0;
            long lastSampleFileCount = 0;
            long lastSample = _sw.ElapsedMilliseconds;

            foreach (CsdSourceFile sourceFile in _stats.SourceFileDict.Select(x => x.Value).Where(x => x.Copied == false))
            {
                sourceFile.ReadSizeAndAttribs(_stats);

                if (_sw.ElapsedMilliseconds - lastSample > _sampleDurationMs)
                {
                    long filesSinceLastSample = fileCount - lastSampleFileCount;
                    long elapsedSinceLastSample =  _sw.ElapsedMilliseconds - lastSample;
                    lastSample = _sw.ElapsedMilliseconds;

                    double filesPerSecond = (double)filesSinceLastSample / ((double)elapsedSinceLastSample / 1000.0);

                    OnProgressChanged(fileCount, _stats.TotalSizePending, filesPerSecond);
                    
                    lastSampleFileCount = fileCount;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}