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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.CSD;
using Archiver.Shared.Utilities;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;
using TerminalUI;

namespace Archiver.Operations.CSD
{
    public static class RegisterDrive
    {
        public static async Task StartOperationAsync(CancellationToken cToken)
        {
            // TODO: use cToken here
            List<CsdDetail> allCsds = await Helpers.ReadCsdIndexAsync(cToken);
            Terminal.Clear();
            Terminal.Header.UpdateLeft("Register New CSD");

            // TODO: Continue here
            string driveLetter = null; //CsdUtils.SelectDrive(CsdUtils.CsdDriveType.Uninitialized);

            if (driveLetter == null)
            {
                Terminal.WriteLine();
                Terminal.WriteLine("In order to register a new drive as a CSD, it must first be prepared manually as follows:");
                Terminal.WriteLine("  * Initialize the drive as GPT");
                Terminal.WriteLine("  * Create a single partition that fills the disk");
                Terminal.WriteLine("  * Format the partition with NTFS, all default settings");
                Terminal.WriteLine("  * Rename the Drive to CSD___ (the ___ will automatically be replaced with the next available CSD number)");

                return;
            }

            int blockSize = CsdUtils.GetBlockSize(driveLetter);

            DriveInfo di = DriveInfo.GetDrives().Where(x => x.Name == driveLetter + "\\").FirstOrDefault();

            int nextCsdNumber = 1;

            if (allCsds.Count() > 0)
                nextCsdNumber = allCsds.Max(x => x.CsdNumber) + 1;

            CsdDetail newCsd = new CsdDetail(nextCsdNumber, blockSize, di.TotalFreeSpace);

            di.VolumeLabel = newCsd.CsdName;

            newCsd.SavetoCsd(driveLetter);
            newCsd.SaveToIndex();

            Formatting.WriteLineC(ConsoleColor.Green, $"Drive {driveLetter} was successfully initialized as {newCsd.CsdName}");
        }
    }
}