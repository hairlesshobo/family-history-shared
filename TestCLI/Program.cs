using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared;
using Archiver.Shared.Classes;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Native;
using Archiver.Shared.Utilities;
using LibSMB2Sharp;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.TestCLI
{
    class Program
    {
        static void Main()
        {
            Utils.RequireSupportedOS();
            SysInfo.InitPlatform();

            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                };
                Console.TreatControlCAsInput = true;

                Terminal.ResetColor();
                Terminal.Clear(true);

                Console.CursorVisible = false;

                Terminal.InitHeader("", "TestCLI");
                Terminal.InitStatusBar();
                Terminal.RootPoint.MoveTo();

                // var detail = Windows.WMI.GetCdromDetail("A:");

                // Console.WriteLine(JsonSerializer.Serialize(detail, new JsonSerializerOptions()
                // {
                //     IgnoreNullValues = false,
                //     IncludeFields = true,
                //     WriteIndented = true
                // }));


                //% slax bootloader, known good MD5: 3c78799690d95bd975e352020fc2acb8 linux dd OK, linux archiver OK, windows dd ??, windows archiver OK
                //% archive 0001   , known good MD5: d8f3a48ab0205c2debe1aa55bc0bb6ea linux dd OK, linux archiver OK, windows dd ??, windows archiver OK
                //% archive 0012   , known good MD5: 0ee70125b6a67db3487116844b6c861b linux dd OK, linux archiver OK, windows dd ??, windows archiver OK

                string knownHash = "0fbd7bf76639134005460f9321ae1d25";

                CancellationTokenSource cts = new CancellationTokenSource();

                Terminal.InitStatusBar(
                    new StatusBarItem(
                        "Cancel",
                        (key) => {
                            cts.Cancel();
                            Terminal.Stop();
                            return Task.Delay(0);
                        },
                        Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                    )
                );
                
                //! archive 0012
                //! actual raw size: 24,935,661,568 
                //! windows IOCTL_STORAGE_READ_CAPACITY reported length: 24,935,661,568
                //! WMI reported size: 24,935,110,656
                //! windows explorer reported size: 24,935,110,656

                var kvtElapsedTime = new KeyValueText("Elapsed Time", null, -16);
                Terminal.NextLine();

                var kvtVerified = new KeyValueText("Verified", Formatting.GetFriendlySize(0), -16);
                Terminal.NextLine();

                var kvtCurrentRate = new KeyValueText("Current Rate", Formatting.GetFriendlyTransferRate(0), -16);
                Terminal.NextLine();

                var kvtAvgRate = new KeyValueText("Average Rate", Formatting.GetFriendlyTransferRate(0), -16);
                Terminal.NextLine();
                Terminal.NextLine();

                var progressBar = new ProgressBar();
                Terminal.NextLine();
                Terminal.NextLine();

                kvtElapsedTime.Show();
                kvtVerified.Show();
                kvtCurrentRate.Show();
                kvtAvgRate.Show();
                progressBar.Show();

                Stopwatch sw = Stopwatch.StartNew();

                Task terminalTask = Terminal.Start();

                Task mainTask = Task.Run(async () => 
                {
                    //smb://den;admin@nas.cz.foxhollow.cc/Studio/Video/Final Discs/EVA Basketball DVDs/2007/07-11-20 -- Episcopal.iso
                    using (Smb2Context smb2 = new Smb2Context(connectionString: "smb://den;admin@nas.cz.foxhollow.cc/Movies", password: "Lh2Oog2y"))
                    {
                        Smb2Share share = smb2.OpenShare();

                        Smb2FileEntry entry = share.GetFile("/HD/Guardians of the Galaxy/Guardians of the Galaxy.mkv");
                        
                        using (Stream reader = entry.OpenReader())
                        {
                            Md5StreamGenerator generator = new Md5StreamGenerator(reader, (int)smb2.MaxReadSize);
                            generator.OnProgressChanged += (progress) =>
                            {
                                kvtVerified.UpdateValue($"{Formatting.GetFriendlySize(progress.TotalCopiedBytes)} / {Formatting.GetFriendlySize(progress.TotalBytes)}");
                                kvtAvgRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.AverageTransferRate));
                                kvtCurrentRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.InstantTransferRate));

                                progressBar.UpdateProgress(progress.PercentCopied / 100.0);
                                kvtElapsedTime.UpdateValue(sw.Elapsed.ToString());
                            };

                            generator.OnComplete += (hash) =>
                            {
                                sw.Stop();

                                bool discValid = (knownHash.ToLower() == hash.ToLower());
                            };

                            await generator.GenerateAsync(cts.Token);
                        }
                    }
                });


                Task.WaitAll(mainTask, terminalTask);
                // Terminal.WaitForStop();
            }
            catch (Exception e)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = Console.CursorTop + 5;
                Formatting.WriteLineC(ConsoleColor.Red, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine();
                Formatting.WriteLineC(ConsoleColor.Red, $"Unhandled exception occurred: {e.Message}");
                Console.WriteLine();
                if (e.InnerException != null)
                {
                    Formatting.WriteLineC(ConsoleColor.Red, $"Inner Exception: {e.InnerException.ToString()}");
                    Console.WriteLine();
                }
                Formatting.WriteLineC(ConsoleColor.Red, e.StackTrace);
                Console.WriteLine();
                Formatting.WriteLineC(ConsoleColor.Red, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine();
                SysInfo.WriteSystemInfo();
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("Press ");
                Formatting.WriteC(ConsoleColor.DarkYellow, "<any key>");
                Console.WriteLine(" to terminate application");

                Console.ReadKey(true);

                return;
            }
        }
        
        static void MainOld()
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
