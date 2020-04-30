using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

            string procArgs = $"--burn-data -folder[\\]:\"{Helpers.DirtyPath(Globals._indexDiscDir)}\" -name:\"Archive Index\" -udf:2.5 -iso:\"{Helpers.DirtyPath(isoPath)}\"";

            Process process = new Process();
            process.StartInfo.FileName = Globals._cdbxpPath;
            process.StartInfo.Arguments = procArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            while (process.HasExited == false)
            {
                string output = process.StandardOutput.ReadLine();

                if (output != null && output.EndsWith("%"))
                {
                    output = output.TrimEnd('%');

                    int currentPercent = Int32.Parse(output);

                    string line = Status.GeneratePercentBar(Console.WindowWidth, 0, 0, currentPercent, (currentPercent == 100));
                    Console.CursorLeft = 0;
                    Console.Write(line);
                }

            }

            process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            Console.CursorLeft = 0;
            Console.CursorTop = Console.CursorTop+2;
        }

        public static void CreateISOFromDisc(DestinationDisc disc, Stopwatch masterSw)
        {
            Status.WriteDiscIso(disc, masterSw.Elapsed, 0);

            string procArgs = $"--burn-data -folder[\\]:\"{Helpers.DirtyPath(disc.RootStagingPath)}\" -name:\"{disc.DiscName}\" -udf:2.5 -iso:\"{Helpers.DirtyPath(disc.IsoPath)}\"";

            Process process = new Process();
            process.StartInfo.FileName = Globals._cdbxpPath;
            process.StartInfo.Arguments = procArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            while (process.HasExited == false)
            {
                string output = process.StandardOutput.ReadLine();

                if (output != null && output.EndsWith("%"))
                {
                    output = output.TrimEnd('%');

                    int currentPercent = Int32.Parse(output);

                    Status.WriteDiscIso(disc, masterSw.Elapsed, currentPercent);
                }

            }

            process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            disc.IsoCreated = true;
        }

        public static string GetFileName(string FullPath)
        {
            FullPath = Helpers.CleanPath(FullPath);

            string[] nameParts = FullPath.Split('/');
            
            return nameParts[nameParts.Length-1];
        }
    }
}