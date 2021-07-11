using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.CSD;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
{
    public static class ArchiveSummary
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
         
        public static void StartOperation()
        {
            List<CsdDetail> existingCsdDrives = CsdUtils.ReadIndex();
            Console.Clear();

            // IEnumerable<CsdSourceFile> allFiles = existingTapes.SelectMany(x => x.FlattenFiles());

            using (Pager pager = new Pager())
            {
                pager.Start();
                
                pager.AppendLine("Overall CSD Archive Statistics");
                pager.AppendLine("==============================================================");

                long totalDriveCapacity = existingCsdDrives.Sum(x => x.TotalSpace);
                long totalDataSize = existingCsdDrives.Sum(x => x.DataSize);

                if (existingCsdDrives.Count() > 0)
                {
                    pager.AppendLine($"         Total CSD Drives: {existingCsdDrives.Count()}");
                    // pager.AppendLine($"              Total Files: {allFiles.Count().ToString("N0")}");
                    pager.AppendLine($" Total CSD Drive Capacity: {totalDriveCapacity} Bytes ({Formatting.GetFriendlySize(totalDriveCapacity)})");
                    pager.AppendLine($"          Total Data Size: {totalDataSize} Bytes ({Formatting.GetFriendlySize(totalDataSize)})");
                    pager.AppendLine();
                    pager.AppendLine();
                }

                // foreach (CsdDetail csdDetail in existinCsdDrives)
                // {
                //     pager.AppendLine("Tape Overview: " + csdDetail.Name);
                //     pager.AppendLine("==============================================================");
                //     pager.AppendLine("          Tape ID: " + csdDetail.ID);
                //     pager.AppendLine("        Tape Name: " + csdDetail.Name);
                //     pager.AppendLine(" Tape Description: " + csdDetail.Description);
                //     pager.AppendLine("  Tape Write Date: " + csdDetail.WriteDTM.ToLocalTime().ToString());
                //     pager.AppendLine();
                //     pager.AppendLine("        Data Size: " + Formatting.GetFriendlySize(csdDetail.DataSizeBytes));
                //     pager.AppendLine("  Directory Count: " + csdDetail.FlattenDirectories().Count().ToString("N0"));
                //     pager.AppendLine("       File Count: " + csdDetail.FlattenFiles().Count().ToString("N0"));
                //     pager.AppendLine();
                //     pager.AppendLine("     Archive Size: " + Formatting.GetFriendlySize(csdDetail.TotalArchiveBytes));
                //     pager.AppendLine("     Size on Tape: " + Formatting.GetFriendlySize(csdDetail.ArchiveBytesOnTape));
                //     pager.AppendLine("Compression Ratio: " + csdDetail.CompressionRatio.ToString("0.00") + ":1");
                //     pager.AppendLine();
                //     pager.AppendLine();
                // }
                
                // pager.AppendLine("Overall File Type Statistics");
                // pager.AppendLine("==============================================================");
                // IEnumerable<FileType> fileCounts = allFiles.GroupBy(x => x.Extension)
                //                                         .Select(x => new FileType() { Extension = x.Key, Count = x.Count()})
                //                                         .OrderByDescending(x => x.Count);

                // int maxCountWidth = fileCounts.Max(x => x.Count.ToString().Length);
                // int columnWidth = new int[] { 6, fileCounts.Max(x => x.Extension.Length) }.Max() + 5;

                // foreach (FileType type in fileCounts)
                // {
                //     string extension = "<none>";

                //     if (type.Extension != String.Empty)
                //         extension = type.Extension;

                //     pager.AppendLine($"{extension.PadLeft(columnWidth)}: {type.Count.ToString().PadLeft(maxCountWidth+2)}");
                // }

                pager.WaitForExit();
            }

            existingCsdDrives.Clear();
            // allFiles = null;
        }
    }
}