using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Archiver.Classes.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.CSD
{
    public static class Processing
    {
        public static void IndexAndCountFiles()
        {
            Console.WriteLine();

            FileScanner scanner = new FileScanner();

            scanner.OnProgressChanged += (newFiles, existingFiles, excludedFiles) => {
                // TODO: Add deleted count here
                Status.FileScanned(newFiles, existingFiles, excludedFiles, 0);
            };

            scanner.OnComplete += () => {
                Status.FileScanned(CsdGlobals._newFileCount, CsdGlobals._existingFileCount, CsdGlobals._excludedFileCount, CsdGlobals._deletedFileCount, true);
            };

            Thread scanThread = new Thread(scanner.ScanFiles);
            scanThread.Start();
            scanThread.Join();
        }

        public static void SizeFiles()
        {
            Console.WriteLine();

            FileSizer sizer = new FileSizer();

            sizer.OnProgressChanged += (currentFile, totalSize) => {
                Status.FileSized(currentFile, totalSize);
            };

            sizer.OnComplete += () => {
                Status.FileSized(CsdGlobals._newFileCount, CsdGlobals._totalSize, true);
            };

            Thread sizeThread = new Thread(sizer.SizeFiles);
            sizeThread.Start();
            sizeThread.Join();
        }

        public static bool VerifyFreeSpace()
        {
            long requiredBytes = CsdGlobals._newFileEntries.Sum(x => x.Size);
            long freeSpace = CsdGlobals._destinationCsds.Sum(x => x.FreeSpace);
            bool sufficientSpace = false;

            if (freeSpace > requiredBytes)
                sufficientSpace = true;

            Status.WriteVerifyFreeSpace(requiredBytes, freeSpace, sufficientSpace);

            return sufficientSpace;
        }

        public static void DistributeFiles()
        {
            FileDistributor distributor = new FileDistributor();

            distributor.OnProgressChanged += (currentFile, csdCount) => {
                Status.FileDistributed(currentFile, csdCount);
            };

            distributor.OnComplete += () => {
                int csdCount = CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).Count();
                Status.FileDistributed(CsdGlobals._newFileCount, csdCount, true);
            };

            Thread distributeThread = new Thread(distributor.DistributeFiles);
            distributeThread.Start();
            distributeThread.Join();
        }

        public static void CopyFiles(string driveLetter, CsdDetail csd, Stopwatch masterSw)
        {
            long bytesCopied = 0;
            int currentFile = 1;
            // int currentCsd = 0;
            double averageTransferRate = 0;
            long sampleCount = 1;

            csd.WriteDtmUtc.Add(DateTime.UtcNow);

            IEnumerable<CsdSourceFile> sourceFiles = csd.Files.Where(x => x.Archived == false).OrderBy(x => x.RelativePath);

            int totalFilesToCopy = sourceFiles.Count();
            long totalSizeToCopy = sourceFiles.Sum(x => x.Size);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (CsdSourceFile file in sourceFiles)
            {
                // if we moved to another csd, we reset the csd counter
                // if (file.DestinationCsd.CsdNumber > currentCsd)
                // {
                //     currentCsd = file.DestinationCsd.CsdNumber;
                //     currentFile = 1;
                // }

                var copier = file.ActivateCopy(driveLetter);

                copier.OnProgressChanged += (progress) => {
                    bytesCopied += progress.BytesCopiedSinceLastupdate;
                    file.DestinationCsd.BytesCopied += progress.BytesCopiedSinceLastupdate;

                    // not sure why we get the occasional infinite or invalid number, so lets just filter them out for now
                    if (!Double.IsNaN(progress.InstantTransferRate) 
                      && Double.IsFinite(progress.InstantTransferRate) 
                      && progress.InstantTransferRate > 0.0)
                    {
                        if (sampleCount == 1)
                            averageTransferRate = progress.InstantTransferRate;
                        else
                            averageTransferRate = averageTransferRate + (progress.InstantTransferRate - averageTransferRate) / sampleCount;

                        sampleCount++;
                    }

                    Status.WriteCsdCopyLine(csd, driveLetter, masterSw.Elapsed, currentFile, totalFilesToCopy, bytesCopied, totalSizeToCopy, progress.InstantTransferRate, averageTransferRate);
                };

                copier.OnComplete += (progress) => {
                    file.Hash = copier.MD5_Hash;
                };

                Thread copyThread = new Thread(copier.Copy);
                copyThread.Start();
                copyThread.Join();

                file.Copied = true;
                file.Archived = true;
                file.ArchiveTimeUtc = DateTime.UtcNow;

                currentFile++;
            }

            sw.Stop();
        }

        public static void GenerateHashFile(string driveLetter, CsdDetail csd, Stopwatch masterSw)
        {
            Status.WriteHashListFile(csd, masterSw.Elapsed, 0.0);

            string destinationFile = $"{driveLetter}/hashlist.txt";

            using (FileStream fs = File.Open(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"{"MD5 Hash".PadRight(32)}   File");

                    int currentLine = 1;
                    foreach (CsdSourceFile file in csd.Files.Where(x => x.Size > 0 && x.Hash != null).OrderBy(x => x.RelativePath))
                    {
                        double currentPercent = ((double)currentLine / (double)csd.TotalFiles) * 100.0;
                        sw.WriteLine($"{file.Hash.PadRight(32)}   {file.RelativePath}");
                        Status.WriteHashListFile(csd, masterSw.Elapsed, currentPercent);
                        currentLine++;
                    }

                    sw.Flush();
                }
            }

            Status.WriteHashListFile(csd, masterSw.Elapsed, 100.0);
        }

        public static void SaveJsonData(string driveLetter, CsdDetail csd, Stopwatch masterSw)
        {
            Status.WriteJsonLine(csd, masterSw.Elapsed);

            CsdUtils.SaveDetailToIndex(csd);
        }

        public static void GenerateIndexFiles(string driveLetter, CsdDetail csd, Stopwatch masterSw)
        {
            Status.WriteCsdIndex(csd, masterSw.Elapsed, 0.0);

            if (!Directory.Exists(Globals._indexDiscDir))
                Directory.CreateDirectory(Globals._indexDiscDir);

            string txtIndexPath = Globals._indexDiscDir + "/index.txt";

            bool createMasterIndex = !File.Exists(txtIndexPath);      
            string headerLine = $"CSD   {"Archive Date (UTC)".PadRight(19)}   {"Create Date (UTC)".PadRight(19)}   {"Modify Date (UTC)".PadRight(19)}   {"Size".PadLeft(12)}   Path";      

            using (FileStream masterIndexFS = File.Open(txtIndexPath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter masterIndex = new StreamWriter(masterIndexFS))
                {
                    // if we are creating the file for the firs time, write header line
                    if (createMasterIndex)
                        masterIndex.WriteLine(headerLine);

                    string csdIndexTxtPath = $"{driveLetter}/index.txt";

                    using (FileStream csdIndexFS = File.Open(csdIndexTxtPath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter csdIndex = new StreamWriter(csdIndexFS))
                        {
                            csdIndex.WriteLine(headerLine);

                            int currentLine = 1;

                            // Write the human readable index
                            foreach (CsdSourceFile file in csd.Files.OrderBy(x => x.RelativePath))
                            {
                                string line = "";
                                line += csd.CsdNumber.ToString("000");
                                line += "   ";
                                line += file.ArchiveTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
                                line += "   ";
                                line += file.CreationTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
                                line += "   ";
                                line += file.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
                                line += "   ";
                                line += file.Size.ToString().PadLeft(12);
                                line += "   ";
                                line += file.RelativePath;

                                csdIndex.WriteLine(line);
                                masterIndex.WriteLine(line);

                                double currentPercent = ((double)currentLine / (double)csd.TotalFiles) * 100.0;
                                Status.WriteCsdIndex(csd, masterSw.Elapsed, currentPercent);

                                currentLine++;
                            }
                        }
                    }
                }
            }

            Status.WriteCsdIndex(csd, masterSw.Elapsed, 100.0);
        }

        private static void WriteCsdSummary(string driveLetter, CsdDetail csd, Stopwatch masterSw)
        {
            Status.WriteJsonLine(csd, masterSw.Elapsed);

            CsdUtils.SaveSummaryToCsd(driveLetter, csd);

            Status.WriteJsonLine(csd, masterSw.Elapsed);
        }

        private static string DetectDriveLetter(CsdDetail csd, Stopwatch masterSw)
        {
            while (1 == 1)
            {
                Status.WriteAttachCsdLine(csd, masterSw.Elapsed);

                string driveLetter = CsdUtils.SelectDrive(csd, true);

                if (driveLetter == null)
                    Thread.Sleep(1000);
                else
                    return driveLetter;
            }
        }

        public static void ProcessCsdDrives()
        {
            Status.InitCsdDriveLines();

            foreach (CsdDetail csd in CsdGlobals._destinationCsds.Where(x => x.HasPendingWrites == true).OrderBy(x => x.CsdNumber))
            {
                Stopwatch masterSw = new Stopwatch();
                masterSw.Start();

                string driveLetter = DetectDriveLetter(csd, masterSw);

                if (driveLetter == null)
                {
                    Status.ProcessComplete();

                    Formatting.WriteC(ConsoleColor.Yellow, "WARNING: ");
                    Console.WriteLine("Process canceled prematurely, not all data was archived!");
                    return;
                }

                CopyFiles(driveLetter, csd, masterSw);
                GenerateIndexFiles(driveLetter, csd, masterSw);
                GenerateHashFile(driveLetter, csd, masterSw);
                WriteCsdSummary(driveLetter, csd, masterSw);
            //     // CreateISOFile(csd, masterSw);
            //     // ReadIsoHash(csd, masterSw);
                SaveJsonData(driveLetter, csd, masterSw);

                masterSw.Stop();
                Status.WriteCsdComplete(csd, masterSw.Elapsed);

                // write: CSD xxx finished writing, please "safely remove" drive then press enter
            }

            Status.ProcessComplete();

            Formatting.WriteLineC(ConsoleColor.Green, "Process complete!");
        }
    }
}