/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Archiver.Shared.Classes.Disc;
using Newtonsoft.Json;
using Archiver.Utilities.Shared;
using Archiver.Shared;
using Archiver.Shared.Utilities;

namespace Archiver.Utilities.Disc
{
    public static class DiscProcessing
    {
        private static int _updateFrequencyMs = 1000;
        
        public static void GenerateHashFile(DiscDetail disc, Stopwatch masterSw)
        {
            Status.WriteDiscHashListFile(disc, masterSw.Elapsed, 0.0);

            string destinationFile = PathUtils.CleanPathCombine(disc.RootStagingPath, "/hashlist.txt");

            using (FileStream fs = File.Open(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"{"MD5 Hash".PadRight(32)}   File");

                    int currentLine = 1;
                    foreach (DiscSourceFile file in disc.Files.Where(x => x.Size > 0 && x.Hash != null).OrderBy(x => x.RelativePath))
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

        public static void SaveJsonData(DiscDetail disc, Stopwatch masterSw)
        {
            Status.WriteDiscJsonLine(disc, masterSw.Elapsed);

            disc.SaveToIndex();
        }

        public static void GenerateIndexFiles(DiscDetail disc, Stopwatch masterSw)
        {
            Status.WriteDiscIndex(disc, masterSw.Elapsed, 0.0);

            if (!Directory.Exists(SysInfo.Directories.Index))
                Directory.CreateDirectory(SysInfo.Directories.Index);

            string txtIndexPath = PathUtils.CleanPathCombine(SysInfo.Directories.Index, "/index.txt");

            bool createMasterIndex = !File.Exists(txtIndexPath);      
            string headerLine = $"Disc   {"Archive Date (UTC)".PadRight(19)}   {"Create Date (UTC)".PadRight(19)}   {"Modify Date (UTC)".PadRight(19)}   {"Size".PadLeft(12)}   Path";      

            using (FileStream masterIndexFS = File.Open(txtIndexPath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter masterIndex = new StreamWriter(masterIndexFS))
                {
                    // if we are creating the file for the firs time, write header line
                    if (createMasterIndex)
                        masterIndex.WriteLine(headerLine);

                    string discIndexTxtPath = PathUtils.CleanPathCombine(disc.RootStagingPath, "/index.txt");

                    using (FileStream discIndexFS = File.Open(discIndexTxtPath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter discIndex = new StreamWriter(discIndexFS))
                        {
                            discIndex.WriteLine(headerLine);

                            // mark this as finalized so it won't be touched again after this
                            disc.Finalized = true;

                            int currentLine = 1;

                            // Write the human readable index
                            foreach (DiscSourceFile file in disc.Files.OrderBy(x => x.RelativePath))
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

        private static void WriteDiscInfo(DiscDetail disc, Stopwatch masterSw)
        {
            Status.WriteDiscJsonLine(disc, masterSw.Elapsed);

            DiscSummary discInfo = disc.GetDiscInfo();

            JsonSerializerSettings settings = new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string jsonFilePath = PathUtils.CleanPathCombine(disc.RootStagingPath, "/disc_info.json");
            string json = JsonConvert.SerializeObject(discInfo, settings);
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);

            Status.WriteDiscJsonLine(disc, masterSw.Elapsed);
        }

        public static void CreateISOFile(DiscDetail disc, Stopwatch masterSw)
        {
            Status.WriteDiscIso(disc, masterSw.Elapsed, 0);

            if (!Directory.Exists(SysInfo.Directories.ISO))
                Directory.CreateDirectory(SysInfo.Directories.ISO);

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

        public static void ReadIsoHash(DiscDetail disc, Stopwatch masterSw)
        {
            Status.WriteDiscIsoHash(disc, masterSw.Elapsed, 0.0);

            var md5 = new MD5_Generator(disc.IsoPath);

            md5.OnProgressChanged += (currentPercent) => {
                Status.WriteDiscIsoHash(disc, masterSw.Elapsed, currentPercent);
            };

            Thread generateThread = new Thread(md5.GenerateHash);
            generateThread.Start();
            generateThread.Join();

            disc.Hash = md5.MD5_Hash;

            Status.WriteDiscIsoHash(disc, masterSw.Elapsed, 100.0);
        }

        private static void ProcessDiscs (DiscScanStats stats)
        {
            Status.InitDiscLines();

            foreach (DiscDetail disc in stats.DestinationDiscs.Where(x => x.NewDisc == true).OrderBy(x => x.DiscNumber))
            {
                Stopwatch masterSw = new Stopwatch();
                masterSw.Start();

                GenerateIndexFiles(disc, masterSw);
                GenerateHashFile(disc, masterSw);
                WriteDiscInfo(disc, masterSw);
                CreateISOFile(disc, masterSw);
                ReadIsoHash(disc, masterSw);
                SaveJsonData(disc, masterSw);

                masterSw.Stop();
                Status.WriteDiscComplete(disc, masterSw.Elapsed);
            }

            Status.ProcessComplete();
        }
    }
}