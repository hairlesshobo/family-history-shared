using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Operations.Disc
{
    public static class DiscSearcher
    {
        public static void StartOperation()
        {
            List<DiscDetail> discs = Helpers.ReadDiscIndex();

            Terminal.InitHeader("Search Disc Archive", "Archiver");
            Terminal.InitStatusBar();
            Terminal.Clear();
            
            // while (true)
            // {
                // TODO: handle ctrl+c

                // Console.SetCursorPosition(0, 2);
                // Console.Write("Press ");
                // Formatting.WriteC(ConsoleColor.DarkYellow, "<ctrl>+C");
                // Console.Write(" to cancel");

                // Console.SetCursorPosition(0, 0);
                Console.Write("Term to search for in file/directory: ");
                // Console.TreatControlCAsInput = false;
                // string searchString = Console.ReadLine();
                // Console.TreatControlCAsInput = true;
                KeyInput.ReadString((searchString) =>
                {
                    Terminal.Clear();

                    if (String.IsNullOrWhiteSpace(searchString))
                        return;

                    searchString = searchString.Trim().ToLower();

                    List<DiscSourceFile> files = discs.SelectMany(x => x.Files).Where(x => x.RelativePath.ToLower().Contains(searchString)).ToList();
                    Console.WriteLine("Matching files: " + files.Count().ToString("N0"));

                    using (Pager pager = new Pager())
                    {
                        pager.StartLine = 1;
                        pager.ShowHeader = true;
                        pager.HeaderText = $"{"Disc".PadRight(4)}   {"Update Date/Time".PadRight(22)}   {"File"}";
                        pager.HighlightText = searchString;
                        pager.Highlight = true;
                        pager.HighlightColor = ConsoleColor.DarkYellow;

                        foreach (DiscSourceFile file in files)
                            pager.AppendLine($"{file.DestinationDisc.DiscNumber.ToString("0000")}   {file.LastWriteTimeUtc.ToLocalTime().ToString().PadRight(22)}   {file.RelativePath}");

                        pager.Start();
                        pager.WaitForExit();
                    }
                });
            // }
        }
    }
}