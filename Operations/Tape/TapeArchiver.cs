using System;
using Archiver.Classes.Tape;
using Archiver.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Tape;

namespace Archiver.Operations.Disc
{
    public static class TapeArchiver
    {
        public static void StartOperation()
        {
            if (TapeUtils.IsTapeLoaded() == false)
                Console.WriteLine("ERROR: No tape detected in drive. Insert tape and run process again.");
            else
            {
                TapeSourceInfo tape = TapeUtils.SelectTape();

                if (tape != null)
                {
                    TapeProcessor processor = new TapeProcessor(tape);
                    processor.ProcessTape();
                }
            }
        }
    }
}