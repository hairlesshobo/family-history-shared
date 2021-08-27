using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Classes.Disc;
using Archiver.Operations;
using Archiver.Shared;
using Archiver.Shared.Classes;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver
{
    class Program
    {
        static async Task Main(string[] args)
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


                //% slax bootloader, known good MD5: 3c78799690d95bd975e352020fc2acb8 linux dd OK, linux archiver OK, windows dd ??, windows archiver ??
                //% archive 0001   , known good MD5: d8f3a48ab0205c2debe1aa55bc0bb6ea linux dd OK, linux archiver OK, windows dd ??, windows archiver ??

                // using (LinuxNativeStreamReader reader = new LinuxNativeStreamReader(LinuxNativeStreamReader.StreamSourceType.Disk, "/dev/sr0"))
                // {
                //     Md5StreamGenerator generator = new Md5StreamGenerator(reader);
                //     generator.OnProgressChanged += (progress) =>
                //     {
                //         kvtVerified.UpdateValue($"{Formatting.GetFriendlySize(progress.TotalCopiedBytes)} / {Formatting.GetFriendlySize(progress.TotalBytes)}");
                //         kvtAvgRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.AverageTransferRate));
                //         kvtCurrentRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.InstantTransferRate));

                //         progressBar.UpdateProgress(progress.PercentCopied / 100.0);
                //         // Console.WriteLine($"{progress.PercentCopied}%");
                //     };

                //     generator.OnComplete += (hash) =>
                //     {
                //         // Console.WriteLine(hash);
                //     };

                //     generator.Generate();

                //     // Console.WriteLine(md5hash);
                // }

                // KeyInput.ListenForKeys();
                


                Task.Run(() => MainMenu.StartOperationAsync());
                Terminal.Start();
                // KeyInput.StopListening();

                // await RunTestAsync();
            }
            catch (Exception e)
            {
                // Application.Shutdown();
                // Console.Clear();
                Console.CursorLeft = 0;
                Console.CursorTop = Console.CursorTop + 5;
                Formatting.WriteLineC(ConsoleColor.Red, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine();
                Formatting.WriteLineC(ConsoleColor.Red, $"Unhandled exception occurred: {e.Message}");
                Console.WriteLine();
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

            //Console.ReadLine();
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


