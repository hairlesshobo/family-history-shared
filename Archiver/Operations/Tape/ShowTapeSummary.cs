using System;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using Archiver.Utilities.Tape;

namespace Archiver.Operations.Tape
{
    public static class ShowTapeSummary
    {
        public static void StartOperation()
        {
            if (TapeUtils.IsTapeLoaded() == false)
            {
                Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                Console.WriteLine("No tape is present in the drive, please insert tape and run this operation again.");
            }
            else
            {
                string text = TapeUtils.ReadTxtSummaryFromTape();

                using (Pager pager = new Pager())
                {
                    pager.Start();
                    
                    pager.AutoScroll = false;
                    pager.ShowLineNumbers = true;

                    foreach (string line in text.Split("\n"))
                    {
                        pager.AppendLine(line);
                    }

                    pager.WaitForExit();
                }
            }
        }
    }
}