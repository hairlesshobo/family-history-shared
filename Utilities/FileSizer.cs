using System.Diagnostics;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;

namespace Archiver.Utilities
{
    public delegate void Sizer_ProgressChangedDelegate(long currentFile, long totalSize);
    public delegate void Sizer_CompleteDelegate();

    public class FileSizer
    {
        public event Sizer_ProgressChangedDelegate OnProgressChanged;
        public event Sizer_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 100;
        private Stopwatch _sw;
        private long _lastSample;

        public FileSizer()
        {
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void SizeFiles()
        {
            _sw.Start();

            long fileCount = 0;

            foreach (DiscSourceFile sourceFile in DiscGlobals._discSourceFiles.Where(x => x.Archived == false))
            {
                sourceFile.ReadSizeAndAttribs();

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    OnProgressChanged(fileCount, DiscGlobals._totalSize);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}