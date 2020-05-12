using System.Diagnostics;
using Archiver.Classes.Tape;

namespace Archiver.Utilities.Tape
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