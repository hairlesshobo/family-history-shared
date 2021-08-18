using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Archiver.Shared;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;

namespace Archiver.TestCLI
{
    class Program
    {
        [DllImport("libc.so.6", EntryPoint = "getpid")]
        private static extern int GetPid();

        static void Main()
        {
            Utils.RequireSupportedOS();
            SysInfo.InitPlatform();

            int pid = GetPid();
            
            Formatting.WriteLineC(ConsoleColor.Green, $"Archive TestCLI component starting up. (PID: {pid})");
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            SysInfo.WriteSystemInfo(true);

            if (SysInfo.ConfigErrors.Count > 0)
            {
                Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Formatting.WriteLineC(ConsoleColor.Red, $"{SysInfo.ConfigErrors.Count} Configuration ERROR(s) Found!");
                Console.WriteLine();

                int fieldWidth = SysInfo.ConfigErrors.Max(x => x.Field.Length)+2;

                foreach (ValidationError error in SysInfo.ConfigErrors)
                {
                    Formatting.WriteC(ConsoleColor.Cyan, "Field: ");
                    Console.WriteLine(error.Field.PadRight(fieldWidth));
                    Formatting.WriteC(ConsoleColor.Red, "Error: ");
                    Console.WriteLine(error.Error);
                    Console.WriteLine();
                }

                Formatting.WriteLineC(ConsoleColor.Red, "Application terminating due to error!");
                Environment.Exit(1);
            }

            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine();

            // TODO: Test this with disc in the drive
            //var drives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.CDRom);

            // Console.WriteLine($"sr0: {OpticalDriveUtils.GetDriveLabel("sr0")}");
            Console.WriteLine($"sr0: {OpticalDriveUtils.WindowsGenerateDiscMD5("A:")}");

        }

        
    }
}
