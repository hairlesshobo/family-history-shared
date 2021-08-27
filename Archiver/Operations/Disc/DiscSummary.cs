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
            Terminal.Header.UpdateLeft("Disc Archive Summary");
            DiscGlobals._destinationDiscs = await Helpers.ReadDiscIndexAsync();
            
            Terminal.Clear();

            using (Pager pager = new Pager())
            {
                pager.Start();
                
                pager.AppendLine("Current Disc Archive Statistics");
                pager.AppendLine("==============================================================");

                IEnumerable<DiscDetail> existingDiscs = DiscGlobals._destinationDiscs.Where(x => x.NewDisc == false);
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
                IEnumerable<FileType> fileCounts = DiscGlobals._discSourceFiles
                                                        .GroupBy(x => x.Extension)
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

            DiscGlobals._destinationDiscs.Clear();
        }
    }
}