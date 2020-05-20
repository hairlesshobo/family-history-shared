using System;
using Archiver.Utilities.Shared;
using Archiver.Utilities.Tape;

namespace Archiver.Operations.Disc
{
    public static class ShowTapeSummary
    {
        public static void StartOperation()
        {
            if (TapeUtils.IsTapeLoaded() == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                Console.ResetColor();
                Console.WriteLine("No tape is present in the drive, please insert tape and run this operation again.");
                Console.ReadLine();
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