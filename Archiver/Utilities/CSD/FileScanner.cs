using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.CSD
{
    public delegate void Scanner_ProgressChangedDelegate(long newFiles, long existingFiles, long excludedFiles, double filesPerSecond);
    public delegate void Scanner_CompleteDelegate();

    public class FileScanner
    {
        public event Scanner_CompleteDelegate OnComplete;
        public event Scanner_ProgressChangedDelegate OnProgressChanged;

        private const int _sampleDurationMs = 1000;
        private long _totalFileCount = 0;
        private long _lastSampleFileCount = 0;
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

            foreach (string dirtySourcePath in SysInfo.Config.CSD.SourcePaths)
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

            try {
                foreach (string dir in Directory.GetDirectories(sourcePath))
                {
                    string cleanDir = PathUtils.CleanPath(dir);

                    if (!(SysInfo.Config.CSD.ExcludePaths.Any(x => cleanDir.ToLower().StartsWith(x.ToLower()))))
                        ScanDirectory(dir);
                }
            }
            catch (IOException)
            {
                return;
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                _totalFileCount++;

                string cleanFile = PathUtils.CleanPath(file);
                string fileName = PathUtils.GetFileName(cleanFile);

                if (SysInfo.Config.CSD.ExcludePaths.Any(x => cleanFile.ToLower().StartsWith(x.ToLower())))
                    CsdGlobals._excludedFileCount++;

                else if (SysInfo.Config.CSD.ExcludeFiles.Any(x => fileName.ToLower().EndsWith(x.ToLower())))
                    CsdGlobals._excludedFileCount++;

                else
                    new CsdSourceFile(cleanFile);

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    long elapsedSinceLastSample = _sw.ElapsedMilliseconds - _lastSample;
                    long filesSinceLastSample = _totalFileCount - _lastSampleFileCount;

                    double filesPerSecond = (double)filesSinceLastSample / ((double)elapsedSinceLastSample / 1000.0);

                    OnProgressChanged(CsdGlobals._newFileCount, CsdGlobals._existingFileCount, CsdGlobals._excludedFileCount, filesPerSecond);

                    _lastSample = _sw.ElapsedMilliseconds;
                    _lastSampleFileCount = _totalFileCount;
                }
            }
        }
    }
}