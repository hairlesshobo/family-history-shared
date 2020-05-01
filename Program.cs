using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DiscArchiver.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;

namespace Archiver
{
    class Program
    {
        private static IConfiguration _config;   

        private static void ReadConfig()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            Globals._discCapacityLimit = _config.GetSection("DiscCapacityLimit").Get<long>();
            Globals._stagingDir = _config["StagingDir"];
            Globals._cdbxpPath = _config["CdbxpPath"];
            Globals._excludeFiles = _config.GetSection("ExcludeFiles").Get<string[]>().ToList();
            Globals._sourcePaths = _config.GetSection("SourcePaths").Get<string[]>();
            Array.Sort(Globals._sourcePaths);

            foreach (string excPath in _config.GetSection("ExcludePaths").Get<string[]>())
            {
                string cleanExcPath = Helpers.CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    Globals._excludePaths.Add(cleanExcPath);
            }

            Globals._indexDiscDir = Globals._stagingDir + $"/index";
        }

        private static void ReadIndex()
        {
            string jsonDir = Globals._indexDiscDir + "/json";

            if (Directory.Exists(jsonDir))
            {
                string[] jsonFiles = Directory.GetFiles(jsonDir, "*.json");

                foreach (string jsonFile in jsonFiles)
                    Globals._destinationDiscs.Add(JsonConvert.DeserializeObject<DestinationDisc>(File.ReadAllText(jsonFile)));
            }
        }

        private static void IndexAndCountFiles()
        {
            Console.WriteLine();

            FileScanner scanner = new FileScanner();

            scanner.OnProgressChanged += (newFiles, existingFiles, excludedFiles) => {
                Status.FileScanned(newFiles, existingFiles, excludedFiles);
            };

            scanner.OnComplete += () => {
                Status.FileScanned(Globals._newlyFoundFiles, Globals._existingFilesArchived, Globals._excludedFileCount, true);
            };

            Thread scanThread = new Thread(scanner.ScanFiles);
            scanThread.Start();
            scanThread.Join();
        }

        private static void SizeFiles()
        {
            Console.WriteLine();

            FileSizer sizer = new FileSizer();

            sizer.OnProgressChanged += (currentFile, totalSize) => {
                Status.FileSized(currentFile, totalSize);
            };

            sizer.OnComplete += () => {
                Status.FileSized(Globals._newlyFoundFiles, Globals._totalSize, true);
            };

            Thread sizeThread = new Thread(sizer.SizeFiles);
            sizeThread.Start();
            sizeThread.Join();
        }

        private static void DistributeFiles()
        {
            FileDistributor distributor = new FileDistributor();

            distributor.OnProgressChanged += (currentFile, discCount) => {
                Status.FileDistributed(currentFile, discCount);
            };

            distributor.OnComplete += () => {
                int discCount = Globals._destinationDiscs.Where(x => x.Finalized == false).Count();
                Status.FileDistributed(Globals._newlyFoundFiles, discCount, true);
            };

            Thread distributeThread = new Thread(distributor.DistributeFiles);
            distributeThread.Start();
            distributeThread.Join();
        }

        private static void CopyFiles(DestinationDisc disc, Stopwatch masterSw)
        {
            long bytesCopied = 0;
            int currentFile = 1;
            int currentDisc = 0;
            double averageTransferRate = 0;
            long sampleCount = 1;

            // if the stage dir already exists, we need to remove it so we don't accidentally end up with data
            // on the final disc that doesn't belong there
            if (Directory.Exists(disc.RootStagingPath))
                Directory.Delete(disc.RootStagingPath, true);

            IEnumerable<SourceFile> sourceFiles = disc.Files.Where(x => x.Archived == false).OrderBy(x => x.RelativePath);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (SourceFile file in sourceFiles)
            {
                // if we moved to another disc, we reset the disc counter
                if (file.DestinationDisc.DiscNumber > currentDisc)
                {
                    currentDisc = file.DestinationDisc.DiscNumber;
                    currentFile = 1;
                }

                var copier = file.ActivateCopy();

                copier.OnProgressChanged += (progress) => {
                    bytesCopied += progress.BytesCopiedSinceLastupdate;
                    file.DestinationDisc.BytesCopied += progress.BytesCopiedSinceLastupdate;

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

                    Status.WriteDiscCopyLine(disc, masterSw.Elapsed, currentFile, progress.InstantTransferRate, averageTransferRate);
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

        private static void GenerateHashFile(DestinationDisc disc, Stopwatch masterSw)
        {
            Status.WriteDiscHashListFile(disc, masterSw.Elapsed, 0.0);

            string destinationFile = disc.RootStagingPath + "/hashlist.txt";

            using (FileStream fs = File.Open(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"{"MD5 Hash".PadRight(32)}   File");

                    int currentLine = 1;
                    foreach (SourceFile file in disc.Files.Where(x => x.Size > 0 && x.Hash != null).OrderBy(x => x.RelativePath))
                    {
                        double currentPercent = ((double)currentLine / (double)disc.TotalFiles) * 100.0;
                        sw.WriteLine($"{file.Hash.PadRight(32)}   {file.RelativePath}");
                        Status.WriteDiscHashListFile(disc, masterSw.Elapsed, currentPercent);
                        currentLine++;
                    }

                    sw.Flush();
                }
            }

            Status.WriteDiscHashListFile(disc, masterSw.Elapsed, 100.0);
        }

        private static void SaveJsonData(DestinationDisc disc, Stopwatch masterSw)
        {
            Status.WriteDiscJsonLine(disc, masterSw.Elapsed);

            JsonSerializerSettings settings = new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string indexDir = Globals._indexDiscDir + "/json";

            if (!Directory.Exists(indexDir))
                Directory.CreateDirectory(indexDir);

            // Write the json data needed for future runs of this app
            string jsonFilePath = indexDir + $"/disc_{disc.DiscNumber.ToString("0000")}.json";
            string json = JsonConvert.SerializeObject(disc, settings);
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }

        private static void GenerateIndexFiles(DestinationDisc disc, Stopwatch masterSw)
        {
            Status.WriteDiscIndex(disc, masterSw.Elapsed, 0.0);

            if (!Directory.Exists(Globals._indexDiscDir))
                Directory.CreateDirectory(Globals._indexDiscDir);

            string txtIndexPath = Globals._indexDiscDir + "/index.txt";

            bool createMasterIndex = !File.Exists(txtIndexPath);      
            string headerLine = $"Disc   {"Archive Date (UTC)".PadRight(19)}   {"Create Date (UTC)".PadRight(19)}   {"Modify Date (UTC)".PadRight(19)}   {"Size".PadLeft(12)}   Path";      

            using (FileStream masterIndexFS = File.Open(txtIndexPath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter masterIndex = new StreamWriter(masterIndexFS))
                {
                    // if we are creating the file for the firs time, write header line
                    if (createMasterIndex)
                        masterIndex.WriteLine(headerLine);

                    string discIndexTxtPath = disc.RootStagingPath + "/index.txt";

                    using (FileStream discIndexFS = File.Open(discIndexTxtPath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter discIndex = new StreamWriter(discIndexFS))
                        {
                            discIndex.WriteLine(headerLine);

                            // mark this as finalized so it won't be touched again after this
                            disc.Finalized = true;

                            int currentLine = 1;

                            // Write the human readable index
                            foreach (SourceFile file in disc.Files.OrderBy(x => x.RelativePath))
                            {
                                string line = "";
                                line += disc.DiscNumber.ToString("0000");
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

                                discIndex.WriteLine(line);
                                masterIndex.WriteLine(line);

                                double currentPercent = ((double)currentLine / (double)disc.TotalFiles) * 100.0;
                                Status.WriteDiscIndex(disc, masterSw.Elapsed, currentPercent);

                                currentLine++;
                            }
                        }
                    }
                }
            }

            Status.WriteDiscIndex(disc, masterSw.Elapsed, 100.0);
        }

        private static void CreateISOFile(DestinationDisc disc, Stopwatch masterSw)
        {
            Status.WriteDiscIso(disc, masterSw.Elapsed, 0);
            string isoRoot = Globals._stagingDir + "/iso";

            if (!Directory.Exists(isoRoot))
                Directory.CreateDirectory(isoRoot);

            ISO_Creator creator = new ISO_Creator(disc.DiscName, disc.RootStagingPath, disc.IsoPath);

            creator.OnProgressChanged += (currentPercent) => {
                Status.WriteDiscIso(disc, masterSw.Elapsed, currentPercent);
            };

            creator.OnComplete += () => {
                disc.IsoCreated = true;
                Directory.Delete(disc.RootStagingPath, true);
            };

            Thread isoThread = new Thread(creator.CreateISO);
            isoThread.Start();
            isoThread.Join();
        }

        private static void ReadIsoHash(DestinationDisc disc, Stopwatch masterSw)
        {
            Status.WriteDiscIsoHash(disc, masterSw.Elapsed, 0.0);

            var md5 = new MD5_Generator(disc.IsoPath);

            md5.OnProgressChanged += (currentPercent) => {
                Status.WriteDiscIsoHash(disc, masterSw.Elapsed, currentPercent);
            };

            md5.OnComplete += (hash) => {
                disc.Hash = hash;
            };

            Thread generateThread = new Thread(md5.GenerateHash);
            generateThread.Start();
            generateThread.Join();

            Status.WriteDiscIsoHash(disc, masterSw.Elapsed, 100.0);
        }

        static void ProcessDiscs ()
        {
            Status.InitDiscLines();

            foreach (DestinationDisc disc in Globals._destinationDiscs.Where(x => x.NewDisc == true).OrderBy(x => x.DiscNumber))
            {
                Stopwatch masterSw = new Stopwatch();
                masterSw.Start();

                CopyFiles(disc, masterSw);
                GenerateIndexFiles(disc, masterSw);
                GenerateHashFile(disc, masterSw);
                CreateISOFile(disc, masterSw);
                ReadIsoHash(disc, masterSw);
                SaveJsonData(disc, masterSw);

                masterSw.Stop();
                Status.WriteDiscComplete(disc, masterSw.Elapsed);
            }

            Status.ProcessComplete();
        }

        static void Main(string[] args)
        {
            ReadConfig();

            Console.WriteLine();
            Console.Write("Reading existing index... ");
            ReadIndex();
            Console.WriteLine("done");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Preparing...");
            Console.ResetColor();
            Status.Initialize();

            IndexAndCountFiles();

            if (Globals._newlyFoundFiles > 0)
            {
                SizeFiles();
                DistributeFiles();

                ProcessDiscs();

                Helpers.CreateIndexIso();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Process complete... don't forget to burn the ISOs to disc!");
                Console.ResetColor();
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found to archive. Nothing to do.");
            }


            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
