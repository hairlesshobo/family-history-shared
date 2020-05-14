using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;

namespace Archiver.Utilities.Tape
{
    public class TapeProcessor
    {
        private TapeSourceInfo _sourceInfo;
        private TapeDetail _tapeDetail;
        private TapeStatus _status;

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
            _sourceInfo = sourceInfo;
            _tapeDetail = new TapeDetail()
            {
                Name = sourceInfo.Name,
                SourceInfo = sourceInfo,
                WriteDTM = DateTime.UtcNow
            };
        }

        public void ProcessTape()
        {
            _status = new TapeStatus(_tapeDetail);
            _status.StartTimer();

            Thread rewindThread = new Thread(RewindTape);
            rewindThread.Start();

            IndexAndCountFiles();
            SizeFiles();

            if (rewindThread.IsAlive)
            {
                lock (_status)
                {
                    _status.WriteStatus("Rewinding tape");
                }
                rewindThread.Join();
            }

            WriteTapeSummary();
            WriteTapeJsonSummary();
            ArchiveFiles();
            RewindTape();

            _status.Dispose();
        }

        private void RewindTape()
        {
            lock (_status)
            {
                _status.WriteStatus("Rewinding tape");
            }

            using (TapeOperator tape = new TapeOperator(Config.TapeDrive))
            {
                tape.RewindTape();
            }
        }
        
        private void IndexAndCountFiles()
        {
            lock (_status)
            {
                _status.WriteStatus("Counting source files");
            }

            FileScanner scanner = new FileScanner(_tapeDetail);

            scanner.OnProgressChanged += (newFiles, excludedFiles) => {
                _status.FileScanned(newFiles, excludedFiles);
            };

            scanner.OnComplete += () => {
                _status.FileScanned(_tapeDetail.FileCount, _tapeDetail.ExcludedFileCount, true);
            };

            Thread scanThread = new Thread(scanner.ScanFiles);
            scanThread.Start();
            scanThread.Join();
        }

        private void SizeFiles()
        {
            lock (_status)
            {
                _status.WriteStatus("Calculating source file sizes");
            }

            FileSizer sizer = new FileSizer(_tapeDetail);

            sizer.OnProgressChanged += (currentFile) => {
                _status.FileSized(currentFile);
            };

            sizer.OnComplete += () => {
                _status.FileSized(_tapeDetail.FileCount, true);
            };

            Thread sizeThread = new Thread(sizer.SizeFiles);
            sizeThread.Start();
            sizeThread.Join();
        }

        private void WriteTapeSummary()
        {
            lock (_status)
            {
                _status.WriteStatus("Writing summary to tape");
            }

            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, 64 * 1024))
            {
                string templatePath = Path.Join(Directory.GetCurrentDirectory(), "templates", "tape_summary.txt");
                string[] lines = File.ReadAllLines(templatePath);
                string summaryOutput = String.Empty;
                string dirList = String.Join("\n", _tapeDetail.FlattenDirectories().Select(x => "  " + x.RelativePath).ToArray());

                foreach (string line in lines)
                {
                    summaryOutput += line.Replace("<%TAPE_NAME%>", _tapeDetail.Name)
                                         .Replace("<%WRITE_DATE%>", _tapeDetail.WriteDTM.ToString())
                                         .Replace("<%FILE_COUNT%>", String.Format("{0:n0}", _tapeDetail.FileCount))
                                         .Replace("<%DIR_COUNT%>", String.Format("{0:n0}", _tapeDetail.DirectoryCount))
                                         .Replace("<%SIZE_FRIENDLY%>", Shared.Formatting.GetFriendlySize(_tapeDetail.DataSizeBytes))
                                         .Replace("<%SIZE_BYTES%>", String.Format("{0:n0}", _tapeDetail.DataSizeBytes))
                                         .Replace("<%ARCHIVE_SIZE_FRIENDLY%>", Shared.Formatting.GetFriendlySize(_tapeDetail.TotalArchiveBytes))
                                         .Replace("<%ARCHIVE_SIZE_BYTES%>", String.Format("{0:n0}", _tapeDetail.TotalArchiveBytes))
                                         .Replace("<%DIRECTORY_LIST%>", dirList)
                                         + "\n";
                }

                TapeUtils.WriteStringToTape(tape, summaryOutput, false);
                tape.WriteFilemark();
            }
        }

        private void WriteTapeJsonSummary()
        {
            lock (_status)
            {
                _status.WriteStatus("Writing json record to tape");
            }

            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, 64 * 1024))
            {
                string json = JsonConvert.SerializeObject(_tapeDetail.GetSummary(), Newtonsoft.Json.Formatting.Indented);

                TapeUtils.WriteStringToTape(tape, json, true);
                tape.WriteFilemark();
            }
        }

        
        
        private void ArchiveFiles()
        {
            uint blockSize = (uint)(512 * Config.TapeBlockingFactor);
            uint bufferBlockCount = 4096 * 3; // results in a 3gb buffer

            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, blockSize))
            // using (FileStream fileStream = new FileStream(@"D:\tape\test.tar", FileMode.Create, FileAccess.Write))
            {
                lock (_status)
                {
                    _status.WriteStatus("Positioning tape");
                }

                tape.SetTapeToEndOfData();

                lock (_status)
                {
                    _status.WriteStatus("Allocating memory buffer");
                }

                TapeTarWriter writer = new TapeTarWriter(_tapeDetail, tape, blockSize, bufferBlockCount);

                lock (_status)
                {
                    _status.WriteStatus("Writing data to tape");
                }

                writer.OnProgressChanged += (progress) => {
                    _status.WriteElapsed();
                    _status.UpdateBuffer(progress);
                    _status.UpdateTarInput(progress);
                    _status.UpdateTarWrite(progress);
                    _status.UpdateTapeWrite(progress);
                };
                
                writer.Start();
                tape.WriteFilemark();
            }
        }
    }
}