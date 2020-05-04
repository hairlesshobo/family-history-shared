using System;
using System.Collections.Generic;
using System.Linq;
using DiscArchiver.Classes;
using DiscArchiver.Utilities;

namespace DiscArchiver.Operations
{
    public static class Summary
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


            Console.WriteLine();
            WriteHeader("Current Archive Statistics...");

            IEnumerable<DestinationDisc> existingDiscs = Globals._destinationDiscs.Where(x => x.NewDisc == false);
            if (existingDiscs.Count() > 0)
            {

                Console.WriteLine($"                Discs: {existingDiscs.Count()}");
                Console.WriteLine($"                Files: {existingDiscs.Sum(x => x.Files.Count())}");
                Console.WriteLine($"            Data Size: {Formatting.GetFriendlySize(existingDiscs.Sum(x => x.DataSize))}");
                Console.WriteLine($"    Last Archive Date: {existingDiscs.Max(x => x.ArchiveDTM).ToShortDateString()}");
                Console.WriteLine();
            }
            
            WriteHeader("File Type Statistics...");
            IEnumerable<FileType> fileCounts = Globals._sourceFiles
                                                      .GroupBy(x => x.Extension)
                                                      .Select(x => new FileType() { Extension = x.Key, Count = x.Count()})
                                                      .OrderByDescending(x => x.Count);

            int maxCountWidth = fileCounts.Max(x => x.Count.ToString().Length);
            int columnWidth = fileCounts.Max(x => x.Extension.Length) + 5;

            foreach (FileType type in fileCounts)
            {
                string extension = "<no extension>";

                if (type.Extension != String.Empty)
                    extension = type.Extension;

                Console.WriteLine($"{extension.PadLeft(columnWidth)}: {type.Count.ToString().PadLeft(maxCountWidth+2)}");
            }

            //var jsonFiles = Globals._sourceFiles.Where(x => x.Extension == "json").Select(x => x.FullPath);
        }
    }
}