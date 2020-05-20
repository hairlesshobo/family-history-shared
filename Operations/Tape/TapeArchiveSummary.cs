using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.Disc;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(header);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }
        public static void StartOperation()
        {
            List<TapeDetail> existingTapes = Helpers.ReadTapeIndex();
            Console.Clear();

            IEnumerable<TapeSourceFile> allFiles = existingTapes.SelectMany(x => x.FlattenFiles());

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

                pager.WaitForExit();
            }

            existingTapes.Clear();
            allFiles = null;
        }
    }
}