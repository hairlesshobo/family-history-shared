using System;
using System.Collections.Generic;
using System.Linq;
using DiscArchiver.Classes;
using DiscArchiver.Utilities;

namespace DiscArchiver.Operations
{
    public static class DiscVerification
    {
        private static bool AskVerifyAllDiscs()
        {
            bool verifyAll = false;

            CliMenu menu = new CliMenu(new List<CliMenuEntry>()
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
            menu.Show(true);

            return verifyAll;
        }

        private static DestinationDisc AskDiskToVerify()
        {
            DestinationDisc selectedDisc = Globals._destinationDiscs.FirstOrDefault(x => x.NewDisc);
            List<CliMenuEntry> entries = new List<CliMenuEntry>();

            foreach (DestinationDisc disc in Globals._destinationDiscs.Where(x => x.NewDisc == false).OrderBy(x => x.DiscNumber))
            {
                CliMenuEntry newEntry = new CliMenuEntry();
                newEntry.Name = $"{Formatting.GetDiscName(disc)} | Data Size: {Formatting.GetFriendlySize(disc.DataSize)}";
                newEntry.Action = () => {
                    selectedDisc = disc;
                };

                entries.Add(newEntry);
            }

            CliMenu menu = new CliMenu(entries);
            menu.MenuLabel = "Select disc to verify...";

            menu.Show(true);

            return selectedDisc;
        }

        public static void StartOperation()
        {
            Console.WriteLine("Disc verification process beginning...");
            Console.WriteLine();

            string selectedDrive = Helpers.SelectDrive();

            bool verifyAll = AskVerifyAllDiscs();

            DiscVerifier verifier;
            
            if (verifyAll)
                verifier = new DiscVerifier(selectedDrive);
            else
            {
                DestinationDisc selectedDisc = AskDiskToVerify();
                verifier = new DiscVerifier(selectedDrive, selectedDisc);
            }

            verifier.StartVerification();
        }
    }
}