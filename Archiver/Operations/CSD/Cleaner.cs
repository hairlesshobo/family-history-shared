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
using FoxHollow.Archiver.Shared.Classes.CSD;
using FoxHollow.Archiver.Shared.Utilities;
using FoxHollow.Archiver.Utilities.CSD;

namespace FoxHollow.Archiver.Operations.CSD
{
    public static class Cleaner 
    {
        public static void StartOperation()
        {
            Console.WriteLine("This process will remove any files that are present on the CSD but not in the CSD index.");
            Formatting.WriteC(ConsoleColor.Red, "WARNING: ");
            Console.WriteLine("This cannot be undone!");

            bool doProcess = ConsoleUtils.YesNoConfirm("Do you want to continue?", false, true);

            if (!doProcess)
            {
                Console.WriteLine("Process canceled");
                return;
            }

            string driveLetter = CsdUtils.SelectDrive();

            if (driveLetter == null)
                throw new DriveNotFoundException("Unable to find a CSD drive attached to this system");

            string root = $"{driveLetter}/data";

            CsdDetail csd = CsdUtils.ReadCsdIndex(driveLetter);

            string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);

            List<string> filesNotInIndex = new List<string>();

            foreach (string file in files)
            {
                string relativePath = PathUtils.CleanPath(file).Substring(root.Length);

                bool fileExists = csd.Files.Any(x => x.RelativePath.ToLower() == relativePath.ToLower());

                if (!fileExists)
                    filesNotInIndex.Add(relativePath);
            }

            foreach (string extraFileRelative in filesNotInIndex)
            {
                string realPath = $"{root}{extraFileRelative}";

                Console.WriteLine($"Removing extra file: {extraFileRelative}");
                FileInfo fi = new FileInfo(realPath);
                File.Delete(fi.FullName);

                bool noFilesInDir = fi.Directory
                                      .GetFiles()
                                      .Length == 0;

                if (noFilesInDir)
                {
                    string relativeDirPath = PathUtils.CleanPath(fi.Directory.FullName).Substring(root.Length);

                    Console.WriteLine($"Removing extra directory: {relativeDirPath}");

                    Directory.Delete(fi.Directory.FullName);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Extra files removed: {filesNotInIndex.Count}");
            Console.WriteLine();
        }
    }
}