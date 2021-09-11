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
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Operations.Disc
{
    public static class DiscSummary
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
        
        public static async Task StartOperation()
        {
            List<DiscDetail> allDiscs = await Helpers.ReadDiscIndexAsync();
            List<DiscSourceFile> allFiles = allDiscs.SelectMany(x => x.Files).ToList();
            
            Terminal.Clear();
            Terminal.Header.UpdateLeft("Disc Archive Summary");

            using (Pager pager = new Pager())
            {
                pager.Start();
                
                pager.AppendLine("Current Disc Archive Statistics");
                pager.AppendLine("==============================================================");

                IEnumerable<DiscDetail> existingDiscs = allDiscs.Where(x => x.NewDisc == false);
                if (existingDiscs.Count() > 0)
                {

                    pager.AppendLine($"                Discs: {existingDiscs.Count()}");
                    pager.AppendLine($"                Files: {existingDiscs.Sum(x => x.Files.Count())}");
                    pager.AppendLine($"            Data Size: {Formatting.GetFriendlySize(existingDiscs.Sum(x => x.DataSize))}");
                    pager.AppendLine($"    Last Archive Date: {existingDiscs.Max(x => x.ArchiveDTM).ToShortDateString()}");
                    pager.AppendLine();
                }
                
                pager.AppendLine("File Type Statistics");
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