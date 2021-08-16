using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archiver.Classes.CSD;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
{
    public static class RegisterDrive
    {
        public static void StartOperation()
        {
            CsdGlobals._destinationCsds = CsdUtils.ReadIndex();
            Console.Clear();

            string driveLetter = CsdUtils.SelectDrive(CsdUtils.CsdDriveType.Uninitialized);

            if (driveLetter == null)
            {
                Console.WriteLine();
                Console.WriteLine("In order to register a drive as a CSD drive, it must first be prepared manually as follows:");
                Console.WriteLine("  * Initialize the drive as GPT");
                Console.WriteLine("  * Create a single partition that fills the disk");
                Console.WriteLine("  * Format the partition with NTFS, all default settings");
                Console.WriteLine("  * Rename the Drive to CSD___ (the ___ will automatically be replaced with the next available CSD number)");

                return;
            }

            int blockSize = CsdUtils.GetBlockSize(driveLetter);

            DriveInfo di = DriveInfo.GetDrives().Where(x => x.Name == driveLetter + "\\").FirstOrDefault();

            int nextCsdNumber = 1;

            if (CsdGlobals._destinationCsds.Count() > 0)
                nextCsdNumber = CsdGlobals._destinationCsds.Max(x => x.CsdNumber) + 1;

            CsdDetail newCsd = new CsdDetail(nextCsdNumber, blockSize, di.TotalFreeSpace);

            di.VolumeLabel = newCsd.CsdName;

            CsdUtils.SaveSummaryToCsd(driveLetter, newCsd);
            CsdUtils.SaveDetailToIndex(newCsd);

            Formatting.WriteLineC(ConsoleColor.Green, $"Drive {driveLetter} was successfully initialized as {newCsd.CsdName}");

            CsdGlobals.Reset();
        }
    }
}