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
using FoxHollow.Archiver.Shared.Classes.Tape;

namespace FoxHollow.Archiver.CLI.Utilities.Tape
{
    public delegate void Sizer_ProgressChangedDelegate(long currentFile);
    public delegate void Sizer_CompleteDelegate();

    public class FileSizer
    {
        public event Sizer_ProgressChangedDelegate OnProgressChanged;
        public event Sizer_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 100;
        private Stopwatch _sw;
        private long _lastSample;
        private long _fileCount = 0;
        private TapeDetail _tapeDetail;

        public FileSizer(TapeDetail tapeDetail)
        {
            _tapeDetail = tapeDetail;
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void SizeFiles()
        {
            _sw.Start();

            foreach (TapeSourceDirectory dir in _tapeDetail.Directories)
                SizeFilesInDirectory(dir);

            _sw.Stop();
            OnComplete();
        }

        private void SizeFilesInDirectory(TapeSourceDirectory directory)
        {
            foreach (TapeSourceDirectory subdir in directory.Directories)
                SizeFilesInDirectory(subdir);

            foreach (TapeSourceFile sourceFile in directory.Files)
            {
                sourceFile.ReadSizeAndAttribs(_tapeDetail);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(_fileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                _fileCount++;
            }
        }
    }
}