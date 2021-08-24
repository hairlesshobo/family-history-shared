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
            Terminal.Header.UpdateLeft("Verify Discs");
            Terminal.Clear();

            CancellationTokenSource cts = new CancellationTokenSource();

            DiscGlobals._destinationDiscs = Helpers.ReadDiscIndex();

            Terminal.Clear();
            Terminal.WriteLine("Disc verification process beginning...");

            Terminal.Header.UpdateLeft("Verify Discs > Select Drive");
            string selectedDrive = await Helpers.SelectCdromDriveAsync();

            if (selectedDrive == null)
                return;

            Terminal.Header.UpdateLeft("Verify Discs");
            bool? verifyAll = await AskVerifyAllDiscsAsync();

            if (verifyAll == null)
                return;

            DiscVerifier verifier;
            
            if (verifyAll.Value == true)
                verifier = new DiscVerifier(selectedDrive);
            else
            {
                List<DiscDetail> discsToVerify = await AskDiskToVerifyAsync();

                if (discsToVerify == null)
                    return;

                verifier = new DiscVerifier(selectedDrive, discsToVerify);
            }

            // verifier.StartVerification();

            await Task.Delay(0);

            DiscGlobals._destinationDiscs.Clear();
        }
        
        private static async Task<Nullable<bool>> AskVerifyAllDiscsAsync()
        {
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
            menu.MenuLabel = "What do you want to verify?";
            List<bool> result = await menu.ShowAsync(true);

            if (result == null)
                return null;

            return result[0];
        }

        public static async Task<List<DiscDetail>> AskDiskToVerifyAsync()
        {
            List<CliMenuEntry<DiscDetail>> entries = new List<CliMenuEntry<DiscDetail>>();

            // TODO: Remove 10 limit
            foreach (DiscDetail disc in DiscGlobals._destinationDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber).Take(10))
            {
                entries.Add(new CliMenuEntry<DiscDetail>()
                {
                    Name = $"{DiscFormatting.GetDiscName(disc)} `R|`N `BDate Archived:`N {disc.ArchiveDTM.ToString("MM-dd-yyyy")} `R|`N `BData Size:`N {Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)}",
                    SelectedValue = disc
                });
            }

            CliMenu<DiscDetail> menu = new CliMenu<DiscDetail>(entries, true);
            menu.MenuLabel = "Select disc(s) to verify...";
            menu.EnableCancel = true;

            List<DiscDetail> discsToVerify = await menu.ShowAsync(true);

            return discsToVerify;
        }
    }
}