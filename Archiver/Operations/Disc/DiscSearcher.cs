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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Operations.Disc
{
    public static class DiscSearcher
    {
        public async static Task StartOperationAsync()
        {
            List<DiscDetail> discs = await Helpers.ReadDiscIndexAsync();

            var cts = new CancellationTokenSource();

            Terminal.InitHeader("Search Disc Archive", "Archiver");
            Terminal.InitStatusBar(
                new StatusBarItem(
                    "Cancel",
                    (key) => 
                    {
                        cts.Cancel();
                        return Task.Delay(0);
                    },
                    Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                )
            );

            Terminal.Clear();
            
            Terminal.Write("Term to search for in file/directory: ");

            string searchString = await KeyInput.ReadStringAsync(cts.Token);
            
            searchString = searchString?.Trim()?.ToLower();

            if (String.IsNullOrWhiteSpace(searchString))
                return;
            
            Terminal.Clear();

            List<DiscSourceFile> files = discs.SelectMany(x => x.Files).Where(x => x.RelativePath.ToLower().Contains(searchString)).ToList();
            Console.WriteLine("Matching files: " + files.Count().ToString("N0"));

            using (Pager pager = new Pager())
            {
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
        }
    }
}