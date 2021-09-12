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
using Archiver.Shared.Classes.Disc;
using Archiver.Shared;
using Archiver.Shared.Utilities;

namespace Archiver.Utilities.Disc
{
    public delegate void Scanner_ProgressChangedDelegate(long newFiles, long existingFiles, long excludedFiles);
    public delegate void Scanner_CompleteDelegate();

    public class FileScanner
    {
        public event Scanner_CompleteDelegate OnComplete;
        public event Scanner_ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 100;
        private Stopwatch _sw;
        private long _lastSample;
        private DiscScanStats _stats;

        public FileScanner(DiscScanStats stats)
        {
            _stats = stats ?? throw new System.ArgumentNullException(nameof(stats));
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void ScanFiles()
        {
            _sw.Start();

            foreach (string dirtySourcePath in SysInfo.Config.Disc.SourcePaths)
            {
                string sourcePath = PathUtils.CleanPath(dirtySourcePath);

                ScanDirectory(dirtySourcePath);
            }

            _sw.Stop();
            OnComplete();
        }

        private void ScanDirectory(string sourcePath)
        {
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourcePath}");

            foreach (string dir in Directory.GetDirectories(sourcePath))
            {
                string cleanDir = PathUtils.CleanPath(dir);

                if (!(SysInfo.Config.Disc.ExcludePaths.Any(x => cleanDir.ToLower().StartsWith(x.ToLower()))))
                    ScanDirectory(dir);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string cleanFile = PathUtils.CleanPath(file);

                if (SysInfo.Config.Disc.ExcludePaths.Any(x => cleanFile.ToLower().StartsWith(x.ToLower())))
                    _stats.ExcludedFileCount++;

                else if (SysInfo.Config.Disc.ExcludeFiles.Any(x => PathUtils.GetFileName(cleanFile).ToLower().EndsWith(x.ToLower())))
                    _stats.ExcludedFileCount++;

                else
                    new DiscSourceFile(_stats, cleanFile);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(_stats.NewlyFoundFiles, _stats.ExistingFilesArchived, _stats.ExcludedFileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }
            }
        }
    }
}