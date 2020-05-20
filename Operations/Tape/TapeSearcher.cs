using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Archiver.Classes.Disc;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
{
    public static class TapeSearcher
    {
        public static void StartOperation()
        {
            List<TapeDetail> tapes = Helpers.ReadTapeIndex();
            List<TapeSourceFile> allFiles = tapes.SelectMany(x => x.FlattenFiles()).ToList();

            Console.Clear();
            
            while (true)
            {
                Console.SetCursorPosition(0, 2);
                Console.Write("Press ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("<ctrl>+C");
                Console.ResetColor();
                Console.Write(" to cancel");

                Console.SetCursorPosition(0, 0);
                Console.Write("Term to search for in file/directory: ");
                string searchString = Console.ReadLine();

                Console.Clear();

                if (String.IsNullOrWhiteSpace(searchString))
                    break;

                searchString = searchString.Trim().ToLower();

                List<TapeSourceFile> files = allFiles.Where(x => x.RelativePath.ToLower().Contains(searchString)).ToList();
                Console.WriteLine("Matching files: " + files.Count().ToString("N0"));

                int tapeNameWidth = tapes.Max(x => x.Name.Length);

                using (Pager pager = new Pager())
                {
                    pager.StartLine = 1;
                    pager.ShowHeader = true;
                    pager.HeaderText = $"{"Tape Name".PadRight(tapeNameWidth)}   {"Update Date/Time".PadRight(22)}   {"File"}";
                    pager.HighlightText = searchString;
                    pager.Highlight = true;
                    pager.HighlightColor = ConsoleColor.DarkYellow;

                    foreach (TapeSourceFile file in files)
                        pager.AppendLine($"{file.Tape.Name.PadRight(tapeNameWidth)}   {file.LastWriteTimeUtc.ToLocalTime().ToString().PadRight(22)}   {file.RelativePath}");

                    pager.Start();
                    pager.WaitForExit();
                }
            }
        }
    }
}