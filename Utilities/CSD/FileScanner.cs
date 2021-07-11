using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.CSD
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

            foreach (string dirtySourcePath in CsdGlobals._csdSourcePaths)
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

                if (!(CsdGlobals._csdExcludePaths.Any(x => cleanDir.ToLower().StartsWith(x.ToLower()))))
                    ScanDirectory(dir);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string cleanFile = Helpers.CleanPath(file);

                if (CsdGlobals._csdExcludePaths.Any(x => cleanFile.ToLower().StartsWith(x.ToLower())))
                    CsdGlobals._excludedFileCount++;

                else if (CsdGlobals._csdExcludeFiles.Any(x => Helpers.GetFileName(cleanFile).ToLower().EndsWith(x.ToLower())))
                    CsdGlobals._excludedFileCount++;

                else
                    new CsdSourceFile(cleanFile);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(CsdGlobals._newFileCount, CsdGlobals._existingFileCount, CsdGlobals._excludedFileCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }
            }
        }
    }
}