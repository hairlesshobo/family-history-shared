using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.Disc;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Operations
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
                pager.AppendLine("Current Tape Archive Statistics");
                pager.AppendLine("==============================================================");

                if (existingTapes.Count() > 0)
                {
                    pager.AppendLine($"          Total Tapes: {existingTapes.Count()}");
                    pager.AppendLine($"          Total Files: {allFiles.Count().ToString("N0")}");
                    pager.AppendLine($"      Total Data Size: {Formatting.GetFriendlySize(existingTapes.Sum(x => x.DataSizeBytes))}");
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

                pager.WaitForExit();
            }

            existingTapes.Clear();
            allFiles = null;
        }
    }
}