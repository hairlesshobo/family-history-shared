using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Archiver.Classes;
using Archiver.Classes.CSD;
using Archiver.Classes.Disc;
using Archiver.Classes.Tape;
using Archiver.Utilities.Disc;
using Newtonsoft.Json;

namespace Archiver.Utilities.Shared
{
    public class Helpers
    {
        public static long RoundToNextMultiple(long value, int multiple)
        {
            if (value == 0)
                return 0;
                
            long nearestMultiple = (long)Math.Round((value / (double)multiple), MidpointRounding.ToPositiveInfinity) * multiple;

            return nearestMultiple;
        }

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

        public static string SelectCdromDrive()
        {
            List<DriveInfo> drives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.CDRom).ToList();

            if (drives.Count() == 0)
                throw new DriveNotFoundException("No optical drives were detected on this system!");
            
            string selectedDrive = drives[0].Name.TrimEnd('\\');

            if (drives.Count() == 1)
                return selectedDrive;

            List<CliMenuEntry<string>> entries = new List<CliMenuEntry<string>>();

            foreach (DriveInfo drive in drives)
            {
                CliMenuEntry<string> newEntry = new CliMenuEntry<string>();
                newEntry.Name = drive.Name.TrimEnd('\\') + " (Disc Loaded: ";
                newEntry.Action = () => {
                    selectedDrive = drive.Name.TrimEnd('\\');
                };

                if (drive.IsReady)
                    newEntry.Name += $"YES | Volume name: {drive.VolumeLabel} | Format: {drive.DriveFormat})";
                else
                    newEntry.Name += "NO)";

                    entries.Add(newEntry);
            }

            CliMenu<string> menu = new CliMenu<string>(entries);
            menu.MenuLabel = "Select drive...";
            menu.OnCancel += Operations.MainMenu.StartOperation;

            menu.Show(true);

            return selectedDrive;
        }



        

        public static List<DiscDetail> ReadDiscIndex()
        {
            List<DiscDetail> discs = new List<DiscDetail>();

            string jsonDir = Globals._indexDiscDir + "/json";

            if (Directory.Exists(jsonDir))
            {
                string[] jsonFiles = Directory.GetFiles(jsonDir, "disc_*.json");
                int totalFiles = jsonFiles.Length;
                
                if (totalFiles > 0)
                {
                    int currentFile = 0;

                    foreach (string jsonFile in jsonFiles)
                    {
                        currentFile++;

                        string line = "Reading disc index files... ";
                        line += $"{currentFile.ToString().PadLeft(totalFiles.ToString().Length)}/{totalFiles}";

                        Console.CursorLeft = 0;
                        Console.Write(line);

                        DiscDetail discDetail = JsonConvert.DeserializeObject<DiscDetail>(File.ReadAllText(jsonFile));
                        discDetail.Files.ForEach(x => x.DestinationDisc = discDetail);

                        discs.Add(discDetail);
                    }

                    Console.WriteLine();
                }
            }

            return discs;
        }

        public static List<TapeDetail> ReadTapeIndex()
        {
            List<TapeDetail> tapes = new List<TapeDetail>();

            string jsonDir = Globals._indexDiscDir + "/json";

            if (Directory.Exists(jsonDir))
            {
                string[] jsonFiles = Directory.GetFiles(jsonDir, "tape_*.json");
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

                        TapeDetail tapeDetail = JsonConvert.DeserializeObject<TapeDetail>(File.ReadAllText(jsonFile));
                        tapeDetail.FlattenFiles().ToList().ForEach(x => x.Tape = tapeDetail);
                        
                        tapes.Add(tapeDetail);
                    }

                    Console.WriteLine();
                }
            }

            return tapes;
        }

        

        public static string CleanPath(string inPath)
            => inPath.Replace(@"\", "/").TrimEnd('/');

        public static string DirtyPath(string inPath)
            => inPath.Replace("/", @"\");

        public static string GetRelativePath(string inPath)
        {
            if (inPath.StartsWith("//"))
            {
                inPath = inPath.TrimStart('/');
                return inPath.Substring(inPath.IndexOf('/'));
            }
                
            return inPath.Split(':')[1];
        }

        public static string GetFileName(string FullPath)
        {
            FullPath = Helpers.CleanPath(FullPath);

            return FullPath.Substring(FullPath.LastIndexOf('/')+1);

            // string[] nameParts = FullPath.Split('/');
            
            // return nameParts[nameParts.Length-1];
        }

        public static DiscDetail GetDestinationDisc(long FileSize)
        {
            DiscDetail matchingDisc = DiscGlobals._destinationDiscs.FirstOrDefault(x => x.NewDisc == true && (x.DataSize + FileSize) < DiscGlobals._discCapacityLimit);

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

            string isoPath = DiscGlobals._discStagingDir + "/iso/index.iso";
            string isoName = "Archive Index";

            ISO_Creator creator = new ISO_Creator(isoName, Helpers.DirtyPath(Globals._indexDiscDir), isoPath);

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
                destinationDir = Globals._indexDiscDir + "/json";

            if (fileName == null)
                fileName = $"disc_{disc.DiscNumber.ToString("0000")}.json";

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            string jsonFilePath = destinationDir + "/" + fileName;

            string json = JsonConvert.SerializeObject(disc, new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            // Write the json data needed for future runs of this app
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }

        public static void SaveTape(TapeDetail tape)
        {
            string destinationDir = Globals._indexDiscDir + "/json";
            string fileName = $"tape_{tape.ID.ToString("000")}.json";

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            string jsonFilePath = destinationDir + "/" + fileName;

            string json = JsonConvert.SerializeObject(tape, new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented
            });

            // Write the json data needed for future runs of this app
            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }


        public static bool YesNoConfirm(string message, bool yesDefault, bool clearScreen)
        {
            if (clearScreen)
                Console.Clear();

            if (!message.EndsWith("?"))
                message += "?";

            Console.Write($"{message} (");
            
            if (yesDefault)
            {
                Formatting.WriteC(ConsoleColor.Blue, "YES");
                Console.Write("/no");
            }
            else
            {
                Console.Write("yes/");
                Formatting.WriteC(ConsoleColor.Blue, "NO");
            }

            Console.Write(") ");

            bool prevCtrlValue = Console.TreatControlCAsInput;

            Console.TreatControlCAsInput = false;
            Console.CursorVisible = true;
            string response = Console.ReadLine();
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = prevCtrlValue;

            bool responseWasYes = response.ToLower().StartsWith("yes");

            if (!responseWasYes && yesDefault && response.Trim().Length == 0)
                responseWasYes = true;

            if (clearScreen)
                Console.Clear();

            return responseWasYes;
        }
    }
}