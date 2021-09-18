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
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Tape;
using Archiver.Shared.Operations.Tape;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;
using TerminalUI.Types;

namespace Archiver.Tasks.Tape
{
    internal static class TapeSearcherTask
    {
        public async static Task StartTaskAsync(CancellationToken cToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cToken);

            List<TapeDetail> tapes = await Helpers.ReadTapeIndexAsync(cToken);

            Terminal.Header.UpdateHeader("Search Tape Archive", "Archiver");
            Terminal.InitStatusBar(
                new StatusBarItem(
                    "Cancel",
                    (key) => 
                    {
                        cts.Cancel();
                        return Task.CompletedTask;
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

            TapeSearcher searcher = new TapeSearcher(tapes);

            int tapeNameWidth = tapes.Max(x => x.Name.Length);

            using (Pager pager = new Pager())
            {
                pager.HeaderText = $"{"Tape".PadRight(tapeNameWidth)}   {"Update Date/Time".PadRight(22)}   {"File"}";
                pager.HighlightText = searchString;
                pager.Start();

                await foreach (TapeSourceFile file in searcher.FindFilesAsync(searchString).WithCancellation(pager.CancellationToken))
                    pager.AppendLine($"{file.Tape.Name.PadRight(tapeNameWidth)}   {file.LastWriteTimeUtc.ToLocalTime().ToString().PadRight(22)}   {file.RelativePath}");

                await pager.WaitForQuitAsync();
            }
        }
    }
}