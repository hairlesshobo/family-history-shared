using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DiscArchiver.Shared
{
    public class Helpers
    {
        public static string CleanPath(string inPath)
        {
            return inPath.Replace(@"\", "/").TrimEnd('/');
        }

        public static string DirtyPath(string inPath)
        {
            return inPath.Replace("/", @"\");
        }

        public static string GetRelativePath(string inPath)
        {
            if (inPath.StartsWith("//"))
            {
                inPath = inPath.TrimStart('/');
                return inPath.Substring(inPath.IndexOf('/'));
            }
                
            return inPath.Split(':')[1];
        }

        public static DestinationDisc GetDestinationDisc(long FileSize)
        {
            DestinationDisc matchingDisc = Globals._destinationDiscs.FirstOrDefault(x => x.NewDisc == true && (x.DataSize + FileSize) < Globals._discCapacityLimit);

            if (matchingDisc == null)
            {
                int nextDiscNumber = 1;

                if (Globals._destinationDiscs.Count() > 0)
                    nextDiscNumber = Globals._destinationDiscs.Max(x => x.DiscNumber) + 1;

                DestinationDisc newDisc = new DestinationDisc(nextDiscNumber);
                Globals._destinationDiscs.Add(newDisc);
                return newDisc;
            }
            else
                return matchingDisc;
        }

        public static string GetFriendlyTransferRate(double Rate)
        {
            return GetFriendlySize(Rate, true);
        }

        public static string GetFriendlySize(long Size)
        {
            return GetFriendlySize((double)Size);
        }

        private static string GetFriendlySize(double Size, bool IsRate = false)
        {
            string suffix = "B";

            if (Size > 1024)
            {
                suffix = "KiB";
                Size /= 1024.0;
            }

            if (Size > 1024)
            {
                suffix = "MiB";
                Size /= 1024.0;
            }

            if (Size > 1024)
            {
                suffix = "GiB";
                Size /= 1024.0;
            }

            if (Size > 1024)
            {
                suffix = "TiB";
                Size /= 1024.0;
            }

            string currentSizeFriendly = $"{Math.Round((double)Size, 1).ToString("0.0")} {suffix}";

            if (IsRate == true)
                currentSizeFriendly += "/s";

            return currentSizeFriendly;
        }

        public static void CreateIndexIso()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Creating index iso file...");
            Console.ResetColor();
            Console.WriteLine();

            string isoPath = Globals._stagingDir + "/iso/index.iso";
            string isoName = "Archive Index";

            ISO_Creator creator = new ISO_Creator(isoName, Helpers.DirtyPath(Globals._indexDiscDir), isoPath);

            creator.OnProgressChanged += (currentPercent) => {
                string line = Status.GeneratePercentBar(Console.WindowWidth, 0, 0, currentPercent, (currentPercent == 100));
                Console.CursorLeft = 0;
                Console.Write(line);
            };

            creator.OnComplete += () => {
                string line = Status.GeneratePercentBar(Console.WindowWidth, 0, 0, 100, true);
                Console.CursorLeft = 0;
                Console.Write(line);
            };

            Thread isoThread = new Thread(creator.CreateISO);
            isoThread.Start();
            isoThread.Join();

            Console.CursorLeft = 0;
            Console.CursorTop = Console.CursorTop+2;
        }

        public static string GetFileName(string FullPath)
        {
            FullPath = Helpers.CleanPath(FullPath);

            string[] nameParts = FullPath.Split('/');
            
            return nameParts[nameParts.Length-1];
        }
    }
}