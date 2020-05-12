using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Tape
{
    public delegate void Scanner_ProgressChangedDelegate(long newFiles, long excludedFiles);
    public delegate void Scanner_CompleteDelegate();

    public class FileScanner
    {
        public event Scanner_CompleteDelegate OnComplete;
        public event Scanner_ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 100;
        private Stopwatch _sw;
        private long _lastSample;
        private TapeDetail _tapeDetail;
        private TapeStats _tapeStats;

        public FileScanner(TapeDetail tapeDetail)
        {
            _tapeDetail = tapeDetail;
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void ScanFiles()
        {
            _sw.Start();

            _tapeDetail.Directories = new List<TapeSourceDirectory>();

            foreach (string dirtySourcePath in _tapeDetail.SourceInfo.SourcePaths)
                _tapeDetail.Directories.Add(ScanDirectory(dirtySourcePath));

            _sw.Stop();
            OnComplete();
        }

        private TapeSourceDirectory ScanDirectory(string sourcePath)
        {
            TapeSourceDirectory directory = new TapeSourceDirectory(Helpers.CleanPath(sourcePath));
            return ScanDirectory(directory);
        }

        private TapeSourceDirectory ScanDirectory(TapeSourceDirectory directory)
        {
            if (!Directory.Exists(directory.FullPath))
                throw new DirectoryNotFoundException($"Source directory does not exist: {directory.FullPath}");

            // _tapeDetail.Stats.DirectoryCount++;

            foreach (string dir in Directory.GetDirectories(directory.FullPath))
            {
                string cleanDir = Helpers.CleanPath(dir);

                if (!(_tapeDetail.SourceInfo.ExcludePaths.Any(x => cleanDir.ToLower().StartsWith(x.ToLower()))))
                    directory.Directories.Add(ScanDirectory(dir));
            }

            foreach (string file in Directory.GetFiles(directory.FullPath))
            {
                string cleanFile = Helpers.CleanPath(file);

                if (_tapeDetail.SourceInfo.ExcludePaths.Any(x => cleanFile.ToLower().StartsWith(x.ToLower())))
                    _tapeDetail.ExcludedFileCount++;

                else if (_tapeDetail.SourceInfo.ExcludeFiles.Any(x => Helpers.GetFileName(cleanFile).ToLower().EndsWith(x.ToLower())))
                    _tapeDetail.ExcludedFileCount++;

                else
                {
                    TapeSourceFile sourceFile = new TapeSourceFile(cleanFile);
                    
                    //_tapeDetail.Stats.FileCount++;

                    directory.Files.Add(sourceFile);
                }

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(_tapeDetail.FileCount, _tapeDetail.ExcludedFileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }
            }

            return directory;
        }
    }
}