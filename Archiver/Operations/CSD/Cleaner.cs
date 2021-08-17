using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archiver.Classes.CSD;
using Archiver.Shared.Utilities;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
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