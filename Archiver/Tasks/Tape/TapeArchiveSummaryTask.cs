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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.Archiver.Shared.Classes.Disc;
using FoxHollow.Archiver.Shared.Classes.Tape;
using FoxHollow.Archiver.Shared.Operations.Disc;
using FoxHollow.Archiver.Shared.Operations.Tape;
using FoxHollow.Archiver.Utilities.Shared;
using FoxHollow.TerminalUI;
using FoxHollow.TerminalUI.Elements;

namespace FoxHollow.Archiver.Tasks.Tape
{
    internal static class TapeArchiveSummaryTask
    {
        internal static async Task StartTaskAsync(CancellationToken cToken)
        {
            // TODO: use cToken here
            List<TapeDetail> allTapes = await Helpers.ReadTapeIndexAsync(cToken);
            
            Terminal.Clear();
            Terminal.Header.UpdateLeft("Disc Archive Summary");

            TapeArchiveSummary summary = new TapeArchiveSummary();

            using (Pager pager = Pager.StartNew())
            {
                summary.OnLineGenerated += pager.AppendLine;
                summary.GenerateSummary(allTapes);

                await pager.WaitForQuitAsync();
            }
        }
    }
}