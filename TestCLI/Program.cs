using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Archiver.Shared;
using Archiver.Shared.Classes;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Native;
using Archiver.Shared.Utilities;

namespace Archiver.TestCLI
{
    class Program
    {
        static void Main()
        {
            Utils.RequireSupportedOS();
            SysInfo.InitPlatform();

            Formatting.WriteLineC(ConsoleColor.Green, $"Archive TestCLI component starting up. (PID: {SysInfo.PID})");
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
                Console.ReadLine();
                Environment.Exit(1);
            }

            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine();

            // TODO: Test this with disc in the drive
            //var drives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.CDRom);


            // Console.WriteLine(DiskUtils.LinuxGetFileSize("/home/flip/cv_debug.log"));

            // Console.WriteLine($"sr0: {OpticalDriveUtils.GenerateDiscMD5("sr0")}");
            // Console.WriteLine($"sr0: {OpticalDriveUtils.WindowsGenerateDiscMD5("A:")}");

            List<OpticalDrive> drives = OpticalDriveUtils.GetDrives();

            Console.WriteLine(JsonSerializer.Serialize(drives, new JsonSerializerOptions() { WriteIndented = true }));
            // Console.WriteLine(OpticalDriveUtils.LinuxGetMountPoint("sr0"));

            if (SysInfo.OSType == OSType.Windows)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to close the window...");
                Console.ReadKey();
            }

        // private static void MakeMD5()
        // {
        //     //% slax bootloader, known good MD5: 3c78799690d95bd975e352020fc2acb8 linux dd OK, linux archiver OK, windows dd ??, windows archiver ??
        //     //% archive 0001   , known good MD5: d8f3a48ab0205c2debe1aa55bc0bb6ea linux dd OK, linux archiver OK, windows dd ??, windows archiver ??

        //     using (LinuxNativeStreamReader reader = new LinuxNativeStreamReader(LinuxNativeStreamReader.StreamSourceType.Disk, "/dev/sr0"))
        //     {
        //         Md5StreamGenerator generator = new Md5StreamGenerator(reader);
        //         generator.OnProgressChanged += (progress) =>
        //         {
        //             Console.WriteLine($"{progress.PercentCopied}%");
        //         };

        //         generator.OnComplete += (hash) =>
        //         {
        //             Console.WriteLine(hash);
        //         };

        //         generator.GenerateAsync();

        //         // Console.WriteLine(md5hash);
        //     }

        }

        
    }
}
