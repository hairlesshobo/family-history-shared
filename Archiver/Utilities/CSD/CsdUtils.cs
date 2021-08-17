using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Archiver.Classes.CSD;
using Archiver.Shared;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.CSD
{
    public static class CsdUtils
    {
        public enum CsdDriveType
        {
            Initialized,
            Uninitialized
        }

        public static CsdDetail ReadCsdIndex(string driveLetter)
        {
            string infoFilePath = PathUtils.CleanPathCombine(driveLetter, "info.json");

            if (!File.Exists(infoFilePath))
                throw new DriveNotFoundException($"Unable to find index file at the following location: {infoFilePath}");

            string jsonContent = File.ReadAllText(infoFilePath);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<CsdDetail>(jsonContent);
        }

        public static void ReadIndexToGlobal()
        {
            CsdGlobals._destinationCsds = ReadIndex();

            foreach (CsdSourceFile file in CsdGlobals._jsonReadSourceFiles)
                CsdGlobals._sourceFileDict.Add(file.RelativePath, file);

            CsdGlobals._jsonReadSourceFiles.Clear();
        }

        public static List<CsdDetail> ReadIndex()
        {
            List<CsdDetail> csds = new List<CsdDetail>();

            string jsonDir = SysInfo.Directories.JSON;

            if (Directory.Exists(jsonDir))
            {
                string[] jsonFiles = Directory.GetFiles(jsonDir, "csd_*.json");
                int totalFiles = jsonFiles.Length;
                
                if (totalFiles > 0)
                {
                    int currentFile = 0;

                    foreach (string jsonFile in jsonFiles)
                    {
                        currentFile++;

                        string line = "Reading CSD index files... ";
                        line += $"{currentFile.ToString().PadLeft(totalFiles.ToString().Length)}/{totalFiles}";

                        Console.CursorLeft = 0;
                        Console.Write(line);

                        CsdDetail csdDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<CsdDetail>(File.ReadAllText(jsonFile));
                        
                        foreach (CsdSourceFile file in csdDetail.Files)
                            file.DestinationCsd = csdDetail;

                        csds.Add(csdDetail);
                    }

                    Console.WriteLine();
                }
            }

            csds.ForEach(x => x.SyncStats());

            return csds;
        }

        public static CsdDetail GetDestinationCsd(long FileSize)
        {
            CsdDetail matchingCsd = CsdGlobals._destinationCsds
                                              .FirstOrDefault(x => x.DiskFull == false &&
                                                                   x.UsableFreeSpace > HelpersNew.RoundToNextMultiple(FileSize, x.BlockSize));

            if (matchingCsd == null)
                throw new CsdInsufficientCapacityException($"No CSD Drive with sufficient capacity to store a {FileSize} byte ({Formatting.GetFriendlySize(FileSize)}) file");
            else
                return matchingCsd;
        }

        public static int GetBlockSize(string DriveLetter)
        {
            DriveLetter = DriveLetter.Trim('/');
            DriveLetter = DriveLetter.Trim('\\');
            
            if (!DriveLetter.EndsWith(':'))
                DriveLetter += ':';

            DriveLetter += "\\\\";

            DriveLetter = DriveLetter.ToUpper();

            //var query = new WqlObjectQuery($"SELECT Id,SCSILogicalUnit,Size,Name,MediaLoaded,VolumeName FROM Win32_CDROMDrive WHERE Id='A:'");
            // var query = new WqlObjectQuery($"SELECT SCSILogicalUnit FROM Win32_CDROMDrive WHERE Id='{DriveLetter}'");
            // var query = new WqlObjectQuery($"SELECT Id, Blocksize FROM Win32_Volume WHERE FileSystem='NTFS'");
            var query = new WqlObjectQuery($"SELECT Name, Label, BlockSize FROM Win32_Volume WHERE FileSystem='NTFS' AND Name='{DriveLetter}'");
            using (var searcher = new ManagementObjectSearcher(query))
            {
                var objects = searcher.Get().OfType<ManagementObject>();


                string resultStr = objects.Select(o => o.Properties["BlockSize"].Value.ToString())
                                    .FirstOrDefault();

                if (resultStr != null)
                    return Int32.Parse(resultStr);
            }

            throw new DriveNotFoundException($"Could not find drive {DriveLetter}");
        }

        public static string SelectDrive(CsdDetail csd, bool quiet)
            => SelectDrive(CsdDriveType.Initialized, csd, quiet);

        public static string SelectDrive(CsdDriveType driveType = CsdDriveType.Initialized)
            => SelectDrive(driveType, null, false);

        private static string SelectDrive(CsdDriveType driveType, CsdDetail csd, bool quiet)
        {
            // if a CSD was provided, we MUST find only initialized drives
            if (csd != null)
                driveType = CsdDriveType.Initialized;

            IEnumerable<DriveInfo> driveQuery = DriveInfo.GetDrives()
                .Where(x => x.DriveType == DriveType.Fixed || x.DriveType == DriveType.Removable);

            if (driveType == CsdDriveType.Initialized)
            {
                if (csd != null)
                    driveQuery = driveQuery.Where(x => x.VolumeLabel == csd.CsdName);
                else
                    driveQuery = driveQuery.Where(x => x.VolumeLabel.StartsWith("CSD") && Int32.TryParse(x.VolumeLabel.Substring(3), out int _));
                    
            }
            else
                driveQuery = driveQuery.Where(x => x.VolumeLabel == "CSD___");
                
            List<DriveInfo> drives = driveQuery.ToList();

            if (drives.Count == 0) 
            {
                if (!quiet)
                {
                    Formatting.WriteC(ConsoleColor.Red, "ERROR:");
                    Console.WriteLine("No attached CSD drives were detected on this system!");
                }
                return null;
            }

            string selectedDrive = drives[0].Name.TrimEnd('\\');
            
            if (drives.Count == 1)
            {
                if (!quiet)
                {
                    Formatting.WriteLineC(ConsoleColor.DarkGreen, $"Automatically selected Drive {selectedDrive}");
                    Thread.Sleep(2000);
                    Console.Clear();
                }

                return selectedDrive;
            }

            // if we are looking for a single CSD but we found multiple attached drives with the name we
            // are looking for, we need to error out.
            // TODO: If in the future it becomes an issue to silently return, throw an error here instead
            if (csd != null && drives.Count > 1)
                return null;

            List<CliMenuEntry<string>> entries = new List<CliMenuEntry<string>>();

            foreach (DriveInfo drive in drives)
            {
                string driveLetter = drive.Name.TrimEnd('\\');

                entries.Add(new CliMenuEntry<string>()
                {
                    Name = $"{driveLetter} (Volume name: {drive.VolumeLabel} | Format: {drive.DriveFormat})",
                    Action = () => {
                        selectedDrive = driveLetter;
                    }
                });
            }

            CliMenu<string> menu = new CliMenu<string>(entries);
            menu.MenuLabel = "Select drive...";
            menu.OnCancel += Operations.MainMenu.StartOperation;

            menu.Show(true);

            return selectedDrive;
        }

        private static void SaveCsdDetailToJson(string jsonFilePath, CsdDetail csd)
        {
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings() {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(csd.TakeSnapshot(), settings);
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);

        }

        public static void SaveSummaryToCsd(string driveLetter, CsdDetail csd)
            => SaveCsdDetailToJson(PathUtils.CleanPathCombine(driveLetter, "info.json"), csd);

        public static void SaveDetailToIndex(CsdDetail csd)
        {
            string destDir = SysInfo.Directories.JSON;
            string jsonFilePath = PathUtils.CleanPathCombine(destDir, $"csd_{csd.CsdNumber.ToString("000")}.json");

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            SaveCsdDetailToJson(jsonFilePath, csd);
        }
    }
}