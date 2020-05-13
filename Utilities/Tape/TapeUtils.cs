using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Archiver.Classes.Tape;
using Archiver.Utilities.Shared;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;

namespace Archiver.Utilities.Tape
{
    public static class TapeUtils
    {
        public static TapeSourceInfo SelectTape()
        {
            TapeSourceInfo tape = default(TapeSourceInfo);

            string searchPath = Path.Join(Directory.GetCurrentDirectory(), "config", "tapes");

            string[] files = Directory.GetFiles(searchPath, "*.json");

            List<CliMenuEntry<TapeSourceInfo>> entries = new List<CliMenuEntry<TapeSourceInfo>>();

            foreach (string file in files)
            {
                TapeSourceInfo source = JsonConvert.DeserializeObject<TapeSourceInfo>(File.ReadAllText(file));

                entries.Add(new CliMenuEntry<TapeSourceInfo>() {
                    Name = source.Name,
                    SelectedValue = source
                });
            }

            CliMenu<TapeSourceInfo> menu = new CliMenu<TapeSourceInfo>(entries);
            menu.MenuLabel = "Select tape...";
            
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

        public static string ReadSummaryFromTape()
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

                    if (strlen > 0)
                        summary += Encoding.UTF8.GetString(buffer, 0, strlen);
                }
                while (!endOfData);

                return summary;
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

        public static void WriteStringToTape(TapeOperator tape, string input, bool beginningOfTape)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(input);
            int lengthNeeded = (int)Helpers.RoundToNextMultiple(rawData.Length, (int)tape.BlockSize);
            Array.Resize(ref rawData, lengthNeeded);

            if (beginningOfTape)
                tape.SetTapeFilePosition(0);
            else
                tape.SetTapeToEndOfData();

            using (MemoryStream reader = new MemoryStream(rawData, 0, rawData.Length, false))
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
    }
}