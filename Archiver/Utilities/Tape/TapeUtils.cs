using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Archiver.Classes.Shared;
using Archiver.Classes.Tape;
using Archiver.Shared;
using Archiver.Shared.Models;
using Archiver.Utilities.Shared;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;

namespace Archiver.Utilities.Tape
{
    public static class TapeUtils
    {
        public static bool TapeDrivePresent()
        {
            if (Config.TapeDriver.ToLower() != "auto-remote")
            {
                if (SystemInformation.OperatingSystemType == OSType.Windows)
                    return WindowsTapeDrivePresent();
                else if (SystemInformation.OperatingSystemType == OSType.Linux)
                    return false;
            }

            // TODO: finish implementing tape drive detection
            return false;
        }

        public static bool WindowsTapeDrivePresent()
        {
            try
            {
                using (TapeOperator tape = new TapeOperator(Config.TapeDrive, false))
                {}
            }
            catch (TapeOperatorWin32Exception exception)
            {
                if (exception.HResult == -2146232832)
                    return false;
            }

            return true;
        }

        public static TapeDetail GetTapeDetail(int id)
        {
            List<TapeDetail> tapes = new List<TapeDetail>();

            string jsonDir = Globals._indexDiscDir + "/json";

            if (Directory.Exists(jsonDir))
            {
                string[] jsonFiles = Directory.GetFiles(jsonDir, $"tape_{id.ToString("000")}.json");
                int totalFiles = jsonFiles.Length;
                
                if (totalFiles == 1)
                {
                    Console.CursorLeft = 0;
                    Console.Write("Reading tape index file... ");

                    TapeDetail tapeDetail = JsonConvert.DeserializeObject<TapeDetail>(File.ReadAllText(jsonFiles[0]));
                    tapeDetail.FlattenFiles().ToList().ForEach(x => x.Tape = tapeDetail);

                    Console.WriteLine("done");

                    return tapeDetail;
                }
                else
                    return null;
            }

            return null;
        }

        public static void RewindTape()
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, false))
            {
                tape.RewindTape();
            }
        }

        public static TapeSourceInfo SelectTape()
        {
            TapeSourceInfo tape = default(TapeSourceInfo);

            string searchPath = Path.Join(Directory.GetCurrentDirectory(), "config", "tapes");

            string[] files = Directory.GetFiles(searchPath, "*.json");

            List<CliMenuEntry<TapeSourceInfo>> entries = new List<CliMenuEntry<TapeSourceInfo>>();
            List<TapeSourceInfo> sources = new List<TapeSourceInfo>();

            foreach (string file in files)
            {
                sources.Add(JsonConvert.DeserializeObject<TapeSourceInfo>(File.ReadAllText(file)));
            }

            foreach (TapeSourceInfo source in sources.OrderBy(x => x.ID))
            {
                entries.Add(new CliMenuEntry<TapeSourceInfo>() {
                    Name = $"{source.ID.ToString().PadLeft(3)}: {source.Name}",
                    SelectedValue = source
                });
            }

            CliMenu<TapeSourceInfo> menu = new CliMenu<TapeSourceInfo>(entries);
            menu.MenuLabel = "Select tape...";
            menu.OnCancel += Operations.MainMenu.StartOperation;
            
            List<TapeSourceInfo> selectedItems = menu.Show(true);

            if (selectedItems != null && selectedItems.Count() >= 1)
                return selectedItems.First();

            return tape;
        }

        public static bool TapeHasJsonRecord()
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive))
            {
                // lets test if the second file record is the start of a tar, or a json file. if
                // a json file, then we know that the tape is a new style with the tar located
                // at file 3 instead of 2
                tape.SetTapeFilePosition(1);

                byte[] buffer = new byte[tape.BlockSize];

                // we just want to read one block.. because we only even need the first 2 bytes to determine if the file is a json record
                tape.Read(buffer);

                char firstChar = Encoding.UTF8.GetChars(buffer, 0, 1)[0];

                if (firstChar == '{')
                    return true;
                else
                    return false;
            }
        }

        public static string ReadTxtSummaryFromTape()
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive))
            {
                byte[] buffer = new byte[tape.BlockSize];

                // seek the tape to the beginning of the file marker
                tape.SetTapeFilePosition(0);

                string summary = String.Empty;
                bool endOfData = false;

                do
                {
                    endOfData = tape.Read(buffer);

                    int strlen = Array.IndexOf(buffer, (byte)0);

                    if (!endOfData)
                    {
                        if (strlen > 0)
                            summary += Encoding.UTF8.GetString(buffer, 0, strlen);
                        else
                            summary += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    }
                }
                while (!endOfData);

                return summary;
            }
        }

        public static TapeInfo GetTapeInfo(TapeOperator tape)
        {
            if (tape.TapeLoaded())
            {
                return new TapeInfo()
                {
                    DriveInfo = tape.DriveInfo,
                    TapePosition = tape.GetTapeBlockPosition(),
                    MediaInfo = tape.MediaInfo
                };
            }
            else
            {
                return new TapeInfo()
                {
                    DriveInfo = tape.DriveInfo,
                    TapePosition = -1,
                    MediaInfo = default (TapeMediaInfo)
                };
            }

        }

        public static TapeInfo GetTapeInfo()
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, false))
            {
                return GetTapeInfo(tape);
            }
        }

        public static TapeSummary ReadTapeSummaryFromTape()
        {
            bool hasJson = TapeHasJsonRecord();

            if (!hasJson)
                throw new InvalidOperationException("Tape does not have json record!");

            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, (uint)Config.TapeTextBlockSize, false))
            {
                byte[] buffer = new byte[tape.BlockSize];

                // seek the tape to the beginning of the file marker
                tape.RewindTape();
                tape.SetTapeFilePosition(1);

                string json = String.Empty;
                bool endOfData = false;

                do
                {
                    endOfData = tape.Read(buffer);

                    int strlen = Array.IndexOf(buffer, (byte)0);

                    if (!endOfData)
                    {
                        if (strlen > 0)
                            json += Encoding.UTF8.GetString(buffer, 0, strlen);
                        else
                            json += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    }
                }
                while (!endOfData);

                return JsonConvert.DeserializeObject<TapeSummary>(json);
            }
        }

        public static void ListContentsFromTape()
        {
            bool hasJson = TapeHasJsonRecord();

            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, Config.TapeBlockingFactor * 512))
            {
                byte[] buffer = new byte[tape.BlockSize];

                // seek the tape to the beginning of the file marker
                tape.SetTapeFilePosition(hasJson ? 2 : 1);

                TarArchive archive = TarArchive.CreateInputTarArchive(tape.Stream, Config.TapeBlockingFactor);
                archive.ProgressMessageEvent += ShowTarProgressMessage;
                archive.ListContents();
            }
        }

        public static void ShowTarProgressMessage(TarArchive archive, TarEntry entry, string message)
        {
            if (entry.TarHeader.TypeFlag != TarHeader.LF_NORMAL && entry.TarHeader.TypeFlag != TarHeader.LF_OLDNORM) {
                Console.WriteLine("Entry type " + (char)entry.TarHeader.TypeFlag + " found!");
            }

            if (message != null)
                Console.Write(entry.Name + " " + message);
            else {
                // if (this.verbose) {
                //     string modeString = DecodeType(entry.TarHeader.TypeFlag, entry.Name.EndsWith("/")) + DecodeMode(entry.TarHeader.Mode);
                //     string userString = (string.IsNullOrEmpty(entry.UserName)) ? entry.UserId.ToString() : entry.UserName;
                //     string groupString = (string.IsNullOrEmpty(entry.GroupName)) ? entry.GroupId.ToString() : entry.GroupName;

                //     Console.WriteLine(string.Format("{0} {1}/{2} {3,8} {4:yyyy-MM-dd HH:mm:ss} {5}", modeString, userString, groupString, entry.Size, entry.ModTime.ToLocalTime(), entry.Name));
                // } else {
                    Console.WriteLine(entry.Name);
                //}
            }
        }

        public static void WriteBytesToTape(TapeOperator tape, byte[] inputBuffer, bool appendToTape)
        {
            if (inputBuffer.Length % (int)tape.BlockSize != 0)
                throw new InvalidDataException("Provided input buffer must be a multiple of tape blocksize");

            //if (appendToTape)
            //    tape.SetTapeToEndOfData();

            using (MemoryStream reader = new MemoryStream(inputBuffer, 0, inputBuffer.Length, false))
            {
                int currentPosition = 0;
                byte[] buffer = new byte[tape.BlockSize];

                while (reader.Read(buffer, 0, (int)tape.BlockSize) > 0)
                {
                    currentPosition += (int)tape.BlockSize;

                    tape.Write(buffer);
                }
            }
        }

        public static bool IsTapeLoaded()
        {
            using (TapeOperator tape = new TapeOperator(Config.TapeDrive, false))
            {
                return tape.TapeLoaded();
            }
        }

        public static byte[] GetStringPaddedBytes(string Input, uint BlockSize)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(Input);
            int lengthNeeded = (int)Helpers.RoundToNextMultiple(rawData.Length, (int)BlockSize);
            Array.Resize(ref rawData, lengthNeeded);

            return rawData;
        }
    }
}