using System.Diagnostics;
using System.IO;
using System.Linq;
using DiscArchiver.Classes;

namespace DiscArchiver.Utilities
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

            foreach (string dirtySourcePath in Globals._sourcePaths)
            {
                string sourcePath = Helpers.CleanPath(dirtySourcePath);

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
                string cleanDir = Helpers.CleanPath(dir);

                if (!(Globals._excludePaths.Any(x => cleanDir.ToLower().StartsWith(x.ToLower()))))
                    ScanDirectory(dir);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string cleanFile = Helpers.CleanPath(file);

                if (Globals._excludePaths.Any(x => cleanFile.ToLower().StartsWith(x.ToLower())))
                    Globals._excludedFileCount++;

                else if (Globals._excludeFiles.Any(x => Helpers.GetFileName(cleanFile).ToLower().EndsWith(x.ToLower())))
                    Globals._excludedFileCount++;

                else
                    new SourceFile(cleanFile);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(Globals._newlyFoundFiles, Globals._existingFilesArchived, Globals._excludedFileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }
            }
        }
    }
}