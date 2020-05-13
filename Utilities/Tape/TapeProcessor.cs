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
            using (TapeStatus status = new TapeStatus(_tapeDetail))
            {
                IndexAndCountFiles(status);
                SizeFiles(status);
                //WriteTapeSummary(status);
                //WriteTapeJsonSummary(status);
                ArchiveFiles(status);
            }

            Console.WriteLine(_tapeDetail.TotalArchiveBytes);
        }
        
        private void IndexAndCountFiles(TapeStatus status)
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

        private void SizeFiles(TapeStatus status)
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
        }

        private void WriteTapeSummary(TapeStatus status)
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive))
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

                TapeUtils.WriteStringToTape(tape, summaryOutput, true);
                tape.WriteFilemark();
            }
        }

        private void WriteTapeJsonSummary(TapeStatus status)
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive))
            {
                string json = JsonConvert.SerializeObject(_tapeDetail.GetSummary(), Newtonsoft.Json.Formatting.Indented);

                TapeUtils.WriteStringToTape(tape, json, false);
                tape.WriteFilemark();
            }
        }
        
        private void ArchiveFiles(TapeStatus status)
        {
            using (FileStream stream = new FileStream(@"D:\tape\test.tar", FileMode.Create, FileAccess.Write))
            // using (SlidingStream bufferStream = new SlidingStream())
            using (TarOutputStream tarOut = new TarOutputStream(stream, Config.TapeBlockingFactor))
            using (CustomTarArchive archive = new CustomTarArchive(tarOut))
            {
                ArchiveTapeSourceFiles(_tapeDetail.Files, archive);

                ArchiveTapeSourceDirectory(_tapeDetail.Directories, archive);
            }
        }

        private void ArchiveTapeSourceDirectory(IEnumerable<TapeSourceDirectory> directories, CustomTarArchive archive)
        {
            foreach (TapeSourceDirectory dir in directories)
            {
                ArchiveTapeSourceDirectory(dir, archive);
            }
        }

        private void ArchiveTapeSourceDirectory(TapeSourceDirectory directory, CustomTarArchive archive)
        {
            TarEntry dirEntry = TarEntry.CreateEntryFromFile(directory.FullPath);
            archive.WriteDirectoryEntry(dirEntry);

            ArchiveTapeSourceDirectory(directory.Directories, archive);
            ArchiveTapeSourceFiles(directory.Files, archive);
        }

        private void ArchiveTapeSourceFiles(IEnumerable<TapeSourceFile> files, CustomTarArchive archive)
        {
            foreach (TapeSourceFile file in files)
            {
                TarEntry entry = TarEntry.CreateEntryFromFile(file.FullPath);
                archive.WriteFileEntry(entry, file);
            }
        }
    }
}