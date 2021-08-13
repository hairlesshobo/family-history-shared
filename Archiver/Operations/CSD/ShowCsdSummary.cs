using System;
using Archiver.Classes.CSD;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
{
    public static class ShowCsdSummary
    {
        public static void StartOperation()
        {
            // string drive = CsdUtils.SelectDrive();

            // if (drive != null)
            // {
            //     // TODO: read summary from index, not drive
            //     CsdSummary summary = CsdUtils.ReadSummaryFromCsd(drive);

            //     using (Pager pager = new Pager())
            //     {
            //         pager.Start();
                    
            //         pager.AutoScroll = false;
            //         pager.ShowLineNumbers = true;

            //         foreach (string line in text.Split("\n"))
            //         {
            //             pager.AppendLine(line);
            //         }

            //         pager.WaitForExit();
            //     }
            // }
        }
    }
}