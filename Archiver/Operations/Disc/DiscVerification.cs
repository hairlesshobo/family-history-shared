using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Classes;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities;
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
            string selectedDrive = await Helpers.SelectCdromDriveAsync();

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