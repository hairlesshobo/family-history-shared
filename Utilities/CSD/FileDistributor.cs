using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;

namespace Archiver.Utilities.CSD
{
    public delegate void Distributor_ProgressChangedDelegate(long currentFile, int csdCount);
    public delegate void Distributor_CompleteDelegate();

    public class FileDistributor
    {
        public event Distributor_ProgressChangedDelegate OnProgressChanged;
        public event Distributor_CompleteDelegate OnComplete;

        private const int _sampleDurationMs = 150;
        private Stopwatch _sw;
        private long _lastSample;

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

            foreach (CsdSourceFile sourceFile in CsdGlobals._sourceFiles.Where(x => x.Archived == false).OrderByDescending(x => x.Size))
            {
                sourceFile.AssignCsd();

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    int csdCount = CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).Count();
                    OnProgressChanged(fileCount, csdCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}