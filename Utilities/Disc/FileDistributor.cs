using System.Diagnostics;
using System.IO;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;

namespace Archiver.Utilities.Disc
{
    public delegate void Distributor_ProgressChangedDelegate(long currentFile, int discCount);
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

            foreach (DiscSourceFile sourceFile in DiscGlobals._discSourceFiles.Where(x => x.Archived == false).OrderByDescending(x => x.Size))
            {
                sourceFile.AssignDisc();

                if (_sw.ElapsedMilliseconds - _lastSample > _sampleDurationMs)
                {
                    int discCount = DiscGlobals._destinationDiscs.Where(x => x.Finalized == false).Count();
                    OnProgressChanged(fileCount, discCount);
                    _lastSample = _sw.ElapsedMilliseconds;
                }

                fileCount++;
            }

            _sw.Stop();
            OnComplete();
        }
    }
}