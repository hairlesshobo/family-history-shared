using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Archiver.Classes;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
{
    public static class DiscVerification
    {
        public static void StartOperation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            DiscGlobals._destinationDiscs = Helpers.ReadDiscIndex();
            Console.Clear();

            Console.WriteLine("Disc verification process beginning...");
            Console.WriteLine();

            string selectedDrive = Helpers.SelectCdromDrive(cts);

            if (cts.IsCancellationRequested == true)
                return;

            bool verifyAll = AskVerifyAllDiscs(cts);

            if (cts.IsCancellationRequested == true)
                return;

            DiscVerifier verifier;
            
            if (verifyAll)
                verifier = new DiscVerifier(selectedDrive);
            else
                verifier = new DiscVerifier(selectedDrive, AskDiskToVerify());

            verifier.StartVerification();

            DiscGlobals._destinationDiscs.Clear();
        }
        
        private static bool AskVerifyAllDiscs(CancellationTokenSource cts)
        {
            bool verifyAll = false;

            CliMenu<string> menu = new CliMenu<string>(new List<CliMenuEntry<string>>()
            {
                new CliMenuEntry() {
                    Name = "All Discs",
                    Action = () => verifyAll = true
                },
                new CliMenuEntry() {
                    Name = "Single Disc",
                    Action = () => verifyAll = false
                }
            });

            menu.MenuLabel = "What do you want to verify?";
            menu.OnCancel += () => cts.Cancel();
            menu.Show(true);

            return verifyAll;
        }

        public static List<DiscDetail> AskDiskToVerify()
        {
            List<CliMenuEntry<DiscDetail>> entries = new List<CliMenuEntry<DiscDetail>>();

            foreach (DiscDetail disc in DiscGlobals._destinationDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber))
            {
                entries.Add(new CliMenuEntry<DiscDetail>()
                {
                    Name = $"{DiscFormatting.GetDiscName(disc)} `R|`N `BDate Archived:`N {disc.ArchiveDTM.ToString("MM-dd-yyyy")} `R|`N `BData Size:`N {Formatting.GetFriendlySize(disc.DataSize).PadLeft(10)}",
                    SelectedValue = disc
                });
            }

            CliMenu<DiscDetail> menu = new CliMenu<DiscDetail>(entries, true);
            menu.MenuLabel = "Select disc(s) to verify...";
            menu.OnCancel += Operations.MainMenu.StartOperation;

            List<DiscDetail> discsToVerify = menu.Show(true);

            return discsToVerify;
        }
    }
}