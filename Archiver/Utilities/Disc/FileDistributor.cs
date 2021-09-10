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
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;

namespace Archiver.Utilities.Disc
{
    public delegate void Distributor_ProgressChangedDelegate(long currentFile, int discCount);
    public delegate void Distributor_CompleteDelegate();

    public class FileDistributor
    {
        public event Distributor_ProgressChangedDelegate OnProgressChanged;
        public event Distributor_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 150;
        private Stopwatch _sw;
        private long _lastSample;

        public FileDistributor()
        {
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void DistributeFiles()
        {
            _sw.Start();

            long fileCount = 0;

            foreach (DiscSourceFile sourceFile in DiscGlobals._discSourceFiles.Where(x => x.Archived == false).OrderByDescending(x => x.Size))
            {
                sourceFile.AssignDisc();

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    int discCount = DiscGlobals._destinationDiscs.Where(x => x.Finalized == false).Count();
                    OnProgressChanged(fileCount, discCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}