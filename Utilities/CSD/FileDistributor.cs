using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;

namespace Archiver.Utilities.CSD
{
    public delegate void Distributor_ProgressChangedDelegate(long currentFile, int csdCount, double filesPerSecond);
    public delegate void Distributor_CompleteDelegate();

    public class FileDistributor
    {
        public event Distributor_ProgressChangedDelegate OnProgressChanged;
        public event Distributor_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 1000;
        private Stopwatch _sw;

        public FileDistributor()
        {
            _sw = new Stopwatch();

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void DistributeFiles()
        {
            _sw.Start();

            long fileCount = 0;
            long lastSample = _sw.ElapsedMilliseconds;
            long lastSampleFileCount = 0;

            foreach (CsdSourceFile sourceFile in CsdGlobals._sourceFiles.Where(x => x.Archived == false).OrderByDescending(x => x.Size))
            {
                sourceFile.AssignCsd();

                if (_sw.ElapsedMilliseconds - lastSample > _sampleDurationMs)
                {
                    long filesSinceLastSample = fileCount - lastSampleFileCount;
                    long elapsedSinceLastSample =  _sw.ElapsedMilliseconds - lastSample;
                    lastSample = _sw.ElapsedMilliseconds;

                    double filesPerSecond = (double)filesSinceLastSample / ((double)elapsedSinceLastSample / 1000.0);

                    int csdCount = CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).Count();
                    OnProgressChanged(fileCount, csdCount, filesPerSecond);

                    lastSampleFileCount = fileCount;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}