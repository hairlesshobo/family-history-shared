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
using System.Linq;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Classes.Tape;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Operations.Tape
{
    public static class TapeArchiveSummary
    {
        public class FileType
        {
            public string Extension { get; set; }
            public long Count { get; set; }
        }

        private static void WriteHeader(string header)
        {
            Formatting.WriteLineC(ConsoleColor.Magenta, header);
            Console.WriteLine();
        }
        
        public static async Task StartOperationAsync()
        {
            List<TapeDetail> existingTapes = await Helpers.ReadTapeIndexAsync();
            
            Terminal.Clear();
            Terminal.Header.UpdateLeft("Tape Archive Summary");

            List<TapeSourceFile> allFiles = existingTapes.SelectMany(x => x.FlattenFiles()).ToList();

            using (Pager pager = new Pager())
            {
                pager.Start();
                
                pager.AppendLine("Overall Tape Archive Statistics");
                pager.AppendLine("==============================================================");

                if (existingTapes.Count() > 0)
                {
                    pager.AppendLine($"          Total Tapes: {existingTapes.Count()}");
                    pager.AppendLine($"          Total Files: {allFiles.Count().ToString("N0")}");
                    pager.AppendLine($"      Total Data Size: {Formatting.GetFriendlySize(existingTapes.Sum(x => x.DataSizeBytes))}");
                    pager.AppendLine();
                    pager.AppendLine();
                }

                foreach (TapeDetail tapeDetail in existingTapes)
                {
                    pager.AppendLine("Tape Overview: " + tapeDetail.Name);
                    pager.AppendLine("==============================================================");
                    pager.AppendLine("          Tape ID: " + tapeDetail.ID);
                    pager.AppendLine("        Tape Name: " + tapeDetail.Name);
                    pager.AppendLine(" Tape Description: " + tapeDetail.Description);
                    pager.AppendLine("  Tape Write Date: " + tapeDetail.WriteDTM.ToLocalTime().ToString());
                    pager.AppendLine();
                    pager.AppendLine("        Data Size: " + Formatting.GetFriendlySize(tapeDetail.DataSizeBytes));
                    pager.AppendLine("  Directory Count: " + tapeDetail.FlattenDirectories().Count().ToString("N0"));
                    pager.AppendLine("       File Count: " + tapeDetail.FlattenFiles().Count().ToString("N0"));
                    pager.AppendLine();
                    pager.AppendLine("     Archive Size: " + Formatting.GetFriendlySize(tapeDetail.TotalArchiveBytes));
                    pager.AppendLine("     Size on Tape: " + Formatting.GetFriendlySize(tapeDetail.ArchiveBytesOnTape));
                    pager.AppendLine("Compression Ratio: " + tapeDetail.CompressionRatio.ToString("0.00") + ":1");

                    if (tapeDetail.SourceInfo.SourcePaths.Length > 0)
                    {
                        pager.AppendLine();
                        pager.AppendLine("     Source Paths: " + tapeDetail.SourceInfo.SourcePaths[0]);

                        for (int i = 1; i < tapeDetail.SourceInfo.SourcePaths.Length; i++)
                            pager.AppendLine("                   " + tapeDetail.SourceInfo.SourcePaths[i]);
                    }

                    if (tapeDetail.SourceInfo.ExcludePaths.Length > 0)
                    {
                        pager.AppendLine();
                        pager.AppendLine("   Excluded Paths: " + tapeDetail.SourceInfo.ExcludePaths[0]);

                        for (int i = 1; i < tapeDetail.SourceInfo.ExcludePaths.Length; i++)
                            pager.AppendLine("                   " + tapeDetail.SourceInfo.ExcludePaths[i]);
                    }

                    if (tapeDetail.SourceInfo.ExcludeFiles.Length > 0)
                    {
                        pager.AppendLine();
                        pager.AppendLine("   Excluded Files: " + tapeDetail.SourceInfo.ExcludeFiles[0]);

                        for (int i = 1; i < tapeDetail.SourceInfo.ExcludeFiles.Length; i++)
                            pager.AppendLine("                   " + tapeDetail.SourceInfo.ExcludeFiles[i]);
                    }
                    
                    pager.AppendLine("==============================================================");
                    pager.AppendLine();
                    pager.AppendLine();
                    pager.AppendLine();
                }
                
                pager.AppendLine("Overall File Type Statistics");
                pager.AppendLine("==============================================================");
                IEnumerable<FileType> fileCounts = allFiles.GroupBy(x => x.Extension)
                                                        .Select(x => new FileType() { Extension = x.Key, Count = x.Count()})
                                                        .OrderByDescending(x => x.Count);

                int maxCountWidth = fileCounts.Max(x => x.Count.ToString().Length);
                int columnWidth = new int[] { 6, fileCounts.Max(x => x.Extension.Length) }.Max() + 5;

                foreach (FileType type in fileCounts)
                {
                    string extension = "<none>";

                    if (type.Extension != String.Empty)
                        extension = type.Extension;

                    pager.AppendLine($"{extension.PadLeft(columnWidth)}: {type.Count.ToString().PadLeft(maxCountWidth+2)}");
                }

                await Task.Run(() => pager.WaitForExit());
            }
        }
    }
}