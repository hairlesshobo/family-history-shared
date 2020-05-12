using System;
using System.Threading;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Tape
{
    public class TapeProcessor
    {
        private TapeSourceInfo _sourceInfo;
        private TapeDetail _tapeDetail;

        //private TapeStats _tapeStats;

        public TapeProcessor()
        {
            Initialize(TapeUtils.SelectTape());
        }

        public TapeProcessor(TapeSourceInfo sourceInfo)
        {
            Initialize(sourceInfo);
        }

        private void Initialize(TapeSourceInfo sourceInfo)
        {
            //_tapeStats = new TapeStats();
            _sourceInfo = sourceInfo;
            _tapeDetail = new TapeDetail()
            {
                Name = sourceInfo.Name,
                SourceInfo = sourceInfo,
                //Stats = _tapeStats
            };
        }

        public void ProcessTape()
        {
            using (TapeStatus status = new TapeStatus(_tapeDetail))
            {
                IndexAndCountFiles(status);
                SizeFiles(status);
            }
        }
        
        public void IndexAndCountFiles(TapeStatus status)
        {
            Console.WriteLine();

            FileScanner scanner = new FileScanner(_tapeDetail);

            scanner.OnProgressChanged += (newFiles, excludedFiles) => {
                status.FileScanned(newFiles, excludedFiles);
            };

            scanner.OnComplete += () => {
                status.FileScanned(_tapeDetail.FileCount, _tapeDetail.ExcludedFileCount, true);
            };

            Thread scanThread = new Thread(scanner.ScanFiles);
            scanThread.Start();
            scanThread.Join();
        }

        public void SizeFiles(TapeStatus status)
        {
            Console.WriteLine();

            FileSizer sizer = new FileSizer(_tapeDetail);

            sizer.OnProgressChanged += (currentFile) => {
                status.FileSized(currentFile);
            };

            sizer.OnComplete += () => {
                status.FileSized(_tapeDetail.FileCount, true);
            };

            Thread sizeThread = new Thread(sizer.SizeFiles);
            sizeThread.Start();
            sizeThread.Join();

            // factor in the headers for all the directories
            // _tapeStats.TotalArchiveSize += (_tapeStats.DirectoryCount * 512);

            // factor in the end of archive padding
            // _tapeStats.TotalArchiveSize += 1024;

            // and round up to the next block size
            // _tapeStats.TotalArchiveSize = Helpers.RoundToNextMultiple(_tapeStats.TotalArchiveSize, (512 * Config.TapeBlockingFactor));
        }
    }
}