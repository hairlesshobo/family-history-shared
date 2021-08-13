using System.Diagnostics;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;

namespace Archiver.Utilities.CSD
{
    public delegate void Sizer_ProgressChangedDelegate(long currentFile, long totalSize, double filesPerSecond);
    public delegate void Sizer_CompleteDelegate();

    public class FileSizer
    {
        public event Sizer_ProgressChangedDelegate OnProgressChanged;
        public event Sizer_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 1000;
        private Stopwatch _sw;

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
            long lastSampleFileCount = 0;
            long lastSample = _sw.ElapsedMilliseconds;

            foreach (CsdSourceFile sourceFile in CsdGlobals._sourceFileDict.Select(x => x.Value).Where(x => x.Copied == false))
            {
                sourceFile.ReadSizeAndAttribs();

                if (_sw.ElapsedMilliseconds - lastSample > _sampleDurationMs)
                {
                    long filesSinceLastSample = fileCount - lastSampleFileCount;
                    long elapsedSinceLastSample =  _sw.ElapsedMilliseconds - lastSample;
                    lastSample = _sw.ElapsedMilliseconds;

                    double filesPerSecond = (double)filesSinceLastSample / ((double)elapsedSinceLastSample / 1000.0);

                    OnProgressChanged(fileCount, CsdGlobals._totalSizePending, filesPerSecond);
                    
                    lastSampleFileCount = fileCount;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}