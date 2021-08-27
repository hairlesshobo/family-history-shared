using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Classes.Disc;
using Archiver.Shared;
using Archiver.Shared.Classes.Tape;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Disc;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Utilities.Shared
{
    public class Helpers
    {
        public static int GetCdromId(string DriveLetter)
        {
            DriveLetter = DriveLetter.Trim('/');
            DriveLetter = DriveLetter.Trim('\\');
            
            if (!DriveLetter.EndsWith(':'))
                DriveLetter += ':';

            DriveLetter = DriveLetter.ToUpper();

            //var query = new WqlObjectQuery($"SELECT Id,SCSILogicalUnit,Size,Name,MediaLoaded,VolumeName FROM Win32_CDROMDrive WHERE Id='A:'");
            var query = new WqlObjectQuery($"SELECT SCSILogicalUnit FROM Win32_CDROMDrive WHERE Id='{DriveLetter}'");
            using (var searcher = new ManagementObjectSearcher(query))
            {
                var objects = searcher.Get().OfType<ManagementObject>();

                string resultStr = objects.Select(o => o.Properties["SCSILogicalUnit"].Value.ToString())
                                    .FirstOrDefault();

                if (resultStr != null)
                    return Int32.Parse(resultStr);
            }

            throw new DriveNotFoundException($"Could not find drive {DriveLetter}");
        }

        public static async Task<string> SelectCdromDriveAsync()
        {
            List<OpticalDrive> drives = OpticalDriveUtils.GetDrives();

            if (drives.Count() == 0)
                throw new DriveNotFoundException("No optical drives were detected on this system!");
        
            if (drives.Count() == 1)
                return drives[0].Name;

            Terminal.Clear();
            CliMenu<string> menu = new CliMenu<string>();
            menu.EnableCancel = true;

            List<CliMenuEntry<string>> entries = new List<CliMenuEntry<string>>();

            foreach (OpticalDrive drive in drives)
            {
                CliMenuEntry<string> newEntry = new CliMenuEntry<string>();
                newEntry.Name = drive.Name + " (Disc Loaded: ";
                newEntry.SelectedValue = drive.Name;

                if (drive.IsReady)
                    newEntry.Name += $"YES | Volume name: {drive.VolumeLabel} | Format: {drive.VolumeFormat})";
                else
                    newEntry.Name += "NO)";

                entries.Add(newEntry);
            }

            menu.SetMenuItems(entries);

            List<string> selectedDrives = await menu.ShowAsync(true);

            if (selectedDrives == null)
                return null;
            
            return selectedDrives[0];
        }



        

        public static async Task<List<DiscDetail>> ReadDiscIndexAsync()
        {
            bool cancel = false;
            
            Terminal.Header.UpdateLeft("Read Disc Index...");
            Terminal.StatusBar.ShowItems(
                new StatusBarItem(
                    "Cancel",
                    (key) => {
                        cancel = true;
                        return Task.Delay(0);
                    },
                    Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                )
            );

            if (!Directory.Exists(SysInfo.Directories.JSON))
                return null;

            List<DiscDetail> discs = new List<DiscDetail>();

            string[] jsonFiles = Directory.GetFiles(SysInfo.Directories.JSON, "disc_*.json");
            int totalFiles = jsonFiles.Length;
            
            if (totalFiles == 0)
                return null;

            Text text = new Text();
            text.Show();
            Terminal.NextLine();
            ProgressBar progress = new ProgressBar();
            progress.Show();

            int currentFile = 0;

            foreach (string jsonFile in jsonFiles)
            {
                if (cancel)
                    return null;

                currentFile++;

                double currentPct = (double)currentFile / (double)totalFiles;

                progress.UpdateProgress(currentPct);

                text.UpdateValue($"Reading disc index files... {currentFile.ToString().PadLeft(totalFiles.ToString().Length)}/{totalFiles}");

                DiscDetail discDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscDetail>(File.ReadAllText(jsonFile));
                discDetail.Files.ForEach(x => x.DestinationDisc = discDetail);

                discs.Add(discDetail);
            }

            Terminal.WriteLine();

            await Task.Delay(0);

            return discs;
        }

        public static List<TapeDetail> ReadTapeIndex()
        {
            List<TapeDetail> tapes = new List<TapeDetail>();

            if (Directory.Exists(SysInfo.Directories.JSON))
            {
                string[] jsonFiles = Directory.GetFiles(SysInfo.Directories.JSON, "tape_*.json");
                int totalFiles = jsonFiles.Length;
                
                if (totalFiles > 0)
                {
                    int currentFile = 0;

                    foreach (string jsonFile in jsonFiles)
                    {
                        currentFile++;

                        string line = "Reading tape index files... ";
                        line += $"{currentFile.ToString().PadLeft(totalFiles.ToString().Length)}/{totalFiles}";

                        Console.CursorLeft = 0;
                        Console.Write(line);

                        TapeDetail tapeDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<TapeDetail>(File.ReadAllText(jsonFile));
                        tapeDetail.FlattenFiles().ToList().ForEach(x => x.Tape = tapeDetail);
                        
                        tapes.Add(tapeDetail);
                    }

                    Console.WriteLine();
                }
            }

            return tapes;
        }


        public static DiscDetail GetDestinationDisc(long FileSize)
        {
            DiscDetail matchingDisc = DiscGlobals._destinationDiscs.FirstOrDefault(x => x.NewDisc == true && (x.DataSize + FileSize) < SysInfo.Config.Disc.CapacityLimit);

            if (matchingDisc == null)
            {
                int nextDiscNumber = 1;

                if (DiscGlobals._destinationDiscs.Count() > 0)
                    nextDiscNumber = DiscGlobals._destinationDiscs.Max(x => x.DiscNumber) + 1;

                DiscDetail newDisc = new DiscDetail(nextDiscNumber);
                DiscGlobals._destinationDiscs.Add(newDisc);
                return newDisc;
            }
            else
                return matchingDisc;
        }

        

        public static void CreateIndexIso()
        {
            Console.WriteLine();
            Formatting.WriteC(ConsoleColor.Magenta, "Creating index iso file...");
            Console.WriteLine();

            string isoPath = PathUtils.CleanPathCombine(SysInfo.Directories.ISO, "index.iso");
            string isoName = "Archive Index";

            ISO_Creator creator = new ISO_Creator(isoName, PathUtils.DirtyPath(SysInfo.Directories.Index), isoPath);

            creator.OnProgressChanged += (currentPercent) => {
                string line = StatusHelpers.GeneratePercentBar(Console.WindowWidth, 0, 0, currentPercent, (currentPercent == 100));
                Console.CursorLeft = 0;
                Console.Write(line);
            };

            creator.OnComplete += () => {
                string line = StatusHelpers.GeneratePercentBar(Console.WindowWidth, 0, 0, 100, true);
                Console.CursorLeft = 0;
                Console.Write(line);
            };

            Thread isoThread = new Thread(creator.CreateISO);
            isoThread.Start();
            isoThread.Join();

            Console.CursorLeft = 0;
            Console.CursorTop = Console.CursorTop+2;
        }

        

        public static void SaveDestinationDisc(DiscDetail disc, string destinationDir = null, string fileName = null)
        {
            if (destinationDir == null)
                destinationDir = SysInfo.Directories.JSON;

            if (fileName == null)
                fileName = $"disc_{disc.DiscNumber.ToString("0000")}.json";

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            string jsonFilePath = destinationDir + "/" + fileName;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(disc, new Newtonsoft.Json.JsonSerializerSettings() {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });

            // Write the json data needed for future runs of this app
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }

        public static void SaveTape(TapeDetail tape)
        {
            string fileName = $"tape_{tape.ID.ToString("000")}.json";

            if (!Directory.Exists(SysInfo.Directories.JSON))
                Directory.CreateDirectory(SysInfo.Directories.JSON);

            string jsonFilePath = PathUtils.CleanPathCombine(SysInfo.Directories.JSON, fileName);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(tape, new Newtonsoft.Json.JsonSerializerSettings() {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented
            });

            // Write the json data needed for future runs of this app
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }
    }
}