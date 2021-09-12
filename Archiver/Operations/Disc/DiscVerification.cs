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
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Operations.Disc
{
    public static class DiscVerification
    {
        public static async Task StartOperationAsync()
        {
            Terminal.Header.UpdateLeft("Verify Discs > Initializing...");
            Terminal.Clear();

            List<DiscDetail> allDiscs = await Helpers.ReadDiscIndexAsync();

            Terminal.Header.UpdateLeft("Select Drive");
            OpticalDrive selectedDrive = await Helpers.SelectCdromDriveAsync();

            if (selectedDrive == null)
                return;

            Terminal.Header.UpdateLeft("Verify Discs");
            bool? verifyAll = await AskVerifyAllDiscsAsync();

            if (verifyAll == null)
                return;

            List<DiscDetail> discsToVerify = null;
            
            if (verifyAll.Value == true)
                discsToVerify = allDiscs;
            else
                discsToVerify = await AskDiskToVerifyAsync(allDiscs);

            if (discsToVerify == null)
                return;

            discsToVerify.Sort((x, y) => x.DiscName.CompareTo(y.DiscName));

            DiscVerifier verifier = new DiscVerifier(selectedDrive, discsToVerify);

            Terminal.Clear();
            await verifier.StartVerificationAsync();
        }
        
        private static async Task<Nullable<bool>> AskVerifyAllDiscsAsync()
        {
            Terminal.Clear();
            CliMenu<bool> menu = new CliMenu<bool>(new List<CliMenuEntry<bool>>()
            {
                new CliMenuEntry<bool>() {
                    Name = "All Discs",
                    SelectedValue = true
                },
                new CliMenuEntry<bool>() {
                    Name = "Single Disc",
                    SelectedValue = false
                }
            });

            menu.EnableCancel = true;
            List<bool> result = await menu.ShowAsync(true);

            if (result == null)
                return null;

            return result[0];
        }

        public static async Task<List<DiscDetail>> AskDiskToVerifyAsync(List<DiscDetail> allDiscs)
        {
            List<CliMenuEntry<DiscDetail>> entries = new List<CliMenuEntry<DiscDetail>>();

            foreach (DiscDetail disc in allDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber))
            {
                entries.Add(new CliMenuEntry<DiscDetail>()
                {
                    // Name = $"{DiscFormatting.GetDiscName(disc)} `R|`N `BDate Archived:`N {disc.ArchiveDTM.ToString("MM-dd-yyyy")} `R|`N `BData Size:`N {Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)}",
                    Name = $"{DiscFormatting.GetDiscName(disc)} | Date Archived: {disc.ArchiveDTM.ToString("MM-dd-yyyy")} | Data Size: {Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)}",
                    SelectedValue = disc
                });
            }

            CliMenu<DiscDetail> menu = new CliMenu<DiscDetail>(entries, true);
            menu.LeftPad = 0;
            menu.EnableCancel = true;
            Terminal.Clear();
            List<DiscDetail> discsToVerify = await menu.ShowAsync(true);

            return discsToVerify;
        }
    }
}