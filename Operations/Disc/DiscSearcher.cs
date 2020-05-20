using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Archiver.Classes.Disc;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
{
    public static class DiscSearcher
    {
        public static void StartOperation()
        {
            List<DiscDetail> discs = Helpers.ReadDiscIndex();

            Console.Clear();
            
            while (true)
            {
                Console.Write("Search term (leave blank to quit): ");
                string searchString = Console.ReadLine();

                Console.Clear();

                if (String.IsNullOrWhiteSpace(searchString))
                    break;

                searchString = searchString.Trim().ToLower();

                List<DiscSourceFile> files = discs.SelectMany(x => x.Files).Where(x => x.RelativePath.ToLower().Contains(searchString)).ToList();
                Console.WriteLine("Results: " + files.Count().ToString("N0"));

                using (Pager pager = new Pager())
                {
                    pager.ShowHeader = true;
                    pager.HeaderText = "Matching files: " + files.Count().ToString("N0");
                    pager.HighlightText = searchString;
                    pager.Highlight = true;
                    pager.HighlightColor = ConsoleColor.DarkYellow;

                    foreach (DiscSourceFile file in files)
                        pager.AppendLine(file.RelativePath);

                    pager.Start();
                    pager.WaitForExit();
                }
            }
        }
    }
}