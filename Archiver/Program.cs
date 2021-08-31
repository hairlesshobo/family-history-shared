using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Classes.Disc;
using Archiver.Operations;
using Archiver.Shared;
using Archiver.Shared.Classes;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Native;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using Microsoft.Win32.SafeHandles;
using TerminalUI;
using TerminalUI.Elements;
using Native = Archiver.Shared.Native;

namespace Archiver
{
    class Program
    {
        static void Main(string[] args)
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

                // Terminal.InitHeader("Verify Disc MD5 Hash", "Archiver");
                Terminal.InitHeader("Loading...", "Archiver");
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

                // string knownHash = "d8f3a48ab0205c2debe1aa55bc0bb6ea";

                // CancellationTokenSource cts = new CancellationTokenSource();

                // Terminal.InitStatusBar(
                //     new StatusBarItem(
                //         "Cancel",
                //         (key) => {
                //             cts.Cancel();
                //             return Task.Delay(0);
                //         },
                //         Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                //     )
                // );
                
                //! archive 0012
                //! actual raw size: 24,935,661,568 
                //! windows IOCTL_STORAGE_READ_CAPACITY reported length: 24,935,661,568
                //! WMI reported size: 24,935,110,656
                //! windows explorer reported size: 24,935,110,656

                // var kvtElapsedTime = new KeyValueText("Elapsed Time", null, -16);
                // Terminal.NextLine();

                // var kvtVerified = new KeyValueText("Verified", Formatting.GetFriendlySize(0), -16);
                // Terminal.NextLine();

                // var kvtCurrentRate = new KeyValueText("Current Rate", Formatting.GetFriendlyTransferRate(0), -16);
                // Terminal.NextLine();

                // var kvtAvgRate = new KeyValueText("Average Rate", Formatting.GetFriendlyTransferRate(0), -16);
                // Terminal.NextLine();
                // Terminal.NextLine();

                // var progressBar = new ProgressBar();
                // Terminal.NextLine();
                // Terminal.NextLine();

                // kvtElapsedTime.Show();
                // kvtVerified.Show();
                // kvtCurrentRate.Show();
                // kvtAvgRate.Show();
                // progressBar.Show();

                // Stopwatch sw = Stopwatch.StartNew();

                // using (Stream reader = OpticalDriveUtils.GetDriveRawStream("A:"))
                // {
                //     Md5StreamGenerator generator = new Md5StreamGenerator(reader);
                //     generator.OnProgressChanged += (progress) =>
                //     {
                //         kvtVerified.UpdateValue($"{Formatting.GetFriendlySize(progress.TotalCopiedBytes)} / {Formatting.GetFriendlySize(progress.TotalBytes)}");
                //         kvtAvgRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.AverageTransferRate));
                //         kvtCurrentRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.InstantTransferRate));

                //         progressBar.UpdateProgress(progress.PercentCopied / 100.0);
                //         kvtElapsedTime.UpdateValue(sw.Elapsed.ToString());
                //     };

                //     generator.OnComplete += (hash) =>
                //     {
                //         sw.Stop();

                //         bool discValid = (knownHash.ToLower() == hash.ToLower());
                //     };

                //     await generator.GenerateAsync(cts.Token);
                // }

                // string _devicePath = @"\\.\CDRom0";

                // SafeFileHandle fileHandle = Windows.CreateFile(
                //     _devicePath,
                //     Native.Windows.EFileAccess.GenericRead,
                //     Native.Windows.EFileShare.Read | Native.Windows.EFileShare.Write,
                //     IntPtr.Zero,
                //     Native.Windows.ECreationDisposition.OpenExisting,
                //     Native.Windows.EFileAttributes.Write_Through
                //         | Native.Windows.EFileAttributes.NoBuffering
                //         | Native.Windows.EFileAttributes.RandomAccess,
                //     IntPtr.Zero
                // );

                // if (fileHandle.IsInvalid)
                //     throw new NativeMethodException("CreateFile");

                // Windows.DISK_GEOMETRY_EX geometry = DiskUtils.Windows.ReadDiskGeometry(fileHandle);

                // int sectorBufferCount = 20;
                // int bufferBytes = sectorBufferCount * 2352; //geometry.Geometry.BytesPerSector;
                // byte[] buffer = new byte[bufferBytes];

                // uint returnedBytes = 0;
                // bool result = false;

                // DiskUtils.Windows.SetAllowExtendedIO(fileHandle);

                // Windows.CloseHandle(fileHandle);

                // OpticalDriveUtils.EjectDrive(OpticalDriveUtils.GetDriveByName("A:"));



                Task mainTask = Task.Run(() => MainMenu.StartOperationAsync());
                Terminal.Start();

                // mainTask.Wait();

                // await RunTestAsync();
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

        private static async Task RunTestAsync()
        {
            List<DiscDetail> allDiscs = await Helpers.ReadDiscIndexAsync();

            allDiscs.Sort((x, y) => x.DiscName.CompareTo(y.DiscName));

            Terminal.Clear();
            Terminal.RootPoint.AddY(3).MoveTo();
            NotificationBox box = new NotificationBox(10);
            box.SetTextJustify(0, TextJustify.Center);
            box.SetLineColor(0, ConsoleColor.Green);

            box.Show();


            Terminal.RootPoint.AddY(20).MoveTo();
        }

        public static void ClearLine()
        {
            Console.CursorLeft = 0;
            Console.Write("".PadRight(Console.BufferWidth - 1));
            Console.CursorLeft = 0;
        }
    }
}


