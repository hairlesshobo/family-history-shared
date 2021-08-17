using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

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

        public FileScanner()
        {
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
                    DiscGlobals._excludedFileCount++;

                else if (SysInfo.Config.Disc.ExcludeFiles.Any(x => PathUtils.GetFileName(cleanFile).ToLower().EndsWith(x.ToLower())))
                    DiscGlobals._excludedFileCount++;

                else
                    new DiscSourceFile(cleanFile);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(DiscGlobals._newlyFoundFiles, DiscGlobals._existingFilesArchived, DiscGlobals._excludedFileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }
            }
        }
    }
}