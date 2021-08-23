using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        // public static void DrawBox(int width, int height, string text)
        // {
        //     if (width < 3 || height < 3)
        //         throw new InvalidOperationException();

        //     Console.Write(BoxChars.ThinTopLeft);

        //     for (int curWidth = 1; curWidth < (width-1); curWidth++)
        //         Console.Write(BoxChars.ThinHorizontal);

        //     Console.Write(BoxChars.ThinTopRight);

        //     Console.CursorLeft = 0;
        //     Console.CursorTop = Console.CursorTop+1;
        //     Console.Write(BoxChars.ThinVertical);

        //     Console.Write(text);

        //     Console.CursorLeft = width-1;
        //     Console.Write(BoxChars.ThinVertical);
        // }
        

        private static volatile bool cancelMd5 = false;        

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

                // var kvtDiscName = new KeyValueText("Disc Name", OpticalDriveUtils.GetDriveLabel("sr0"), 14);
                // Terminal.NextLine();

                // var kvtVerified = new KeyValueText("Verified", Formatting.GetFriendlySize(0), 14);
                // Terminal.NextLine();

                // var kvtCurrentRate = new KeyValueText("Current Rate", Formatting.GetFriendlyTransferRate(0), 14);
                // Terminal.NextLine();

                // var kvtAvgRate = new KeyValueText("Average Rate", Formatting.GetFriendlyTransferRate(0), 14);
                // Terminal.NextLine();
                // Terminal.NextLine();

                // var progressBar = new ProgressBar();
                // Terminal.NextLine(); 

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
                
                // Terminal.InitStatusBar(
                //     new StatusBarItem(
                //         "Cancel",
                //         (key) => cancelMd5 = true,
                //         Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                //     )
                // );

                // for (int i = 0; i < 1000; i += 27)
                // {
                //     Thread.Sleep(150);
                //     progressBar.UpdateProgress((double)i / 1000.0);
                //     kvtVerified.UpdateValue(i.ToString());

                //     if (cancelMd5)
                //         break;
                // }

                // Terminal.Clear();
                // Terminal.InitHeader("Main Menu", "Meow");
                // Terminal.InitStatusBar(
                //     new StatusBarItem(
                //         "Quit",
                //         (key) => {
                //             // Console.Clear();
                //             Terminal.NextLine();
                //             Terminal.WriteLine("MEOW!");
                //             // Console.ReadKey();
                //             Environment.Exit(0);
                //         },
                //         Key.MakeKey(ConsoleKey.Q)
                //     ),
                //     new StatusBarItem(
                //         "Navigate",
                //         (key) => { },
                //         Key.MakeKey(ConsoleKey.UpArrow),
                //         Key.MakeKey(ConsoleKey.DownArrow)
                //     )
                // );

                // Terminal.WriteLineColor(ConsoleColor.Black, "Black");
                // Terminal.WriteLineColor(ConsoleColor.Blue, "Blue");
                // Terminal.WriteLineColor(ConsoleColor.DarkBlue, "DarkBlue");
                // Terminal.WriteLineColor(ConsoleColor.Green, "Green");
                // Terminal.WriteLineColor(ConsoleColor.DarkGreen, "DarkGreen");
                // Terminal.WriteLineColor(ConsoleColor.Cyan, "Cyan");
                // Terminal.WriteLineColor(ConsoleColor.DarkCyan, "DarkCyan");
                // Terminal.WriteLineColor(ConsoleColor.Red, "Red");
                // Terminal.WriteLineColor(ConsoleColor.DarkRed, "DarkRed");
                // Terminal.WriteLineColor(ConsoleColor.Magenta, "Magenta");
                // Terminal.WriteLineColor(ConsoleColor.DarkMagenta, "DarkMagenta");
                // Terminal.WriteLineColor(ConsoleColor.Yellow, "Yellow");
                // Terminal.WriteLineColor(ConsoleColor.DarkYellow, "DarkYellow");
                // Terminal.WriteLineColor(ConsoleColor.Gray, "Gray");
                // Terminal.WriteLineColor(ConsoleColor.DarkGray, "DarkGray");
                // Terminal.WriteLineColor(ConsoleColor.White, "White");

                // Terminal.Clear();

                // Terminal.WriteLine("Black");
                // Terminal.WriteLine("Blue");
                // Terminal.WriteLine("DarkBlue");
                // Terminal.WriteLine("Green");
                // Terminal.WriteLine("DarkGreen");
                // Terminal.WriteLine("Cyan");
                // Terminal.WriteLine("DarkCyan");
                // Terminal.WriteLine("Red");
                // Terminal.WriteLine("DarkRed");
                // Terminal.WriteLine("Magenta");
                // Terminal.WriteLine("DarkMagenta");
                // Terminal.WriteLine("Yellow");
                // Terminal.WriteLine("DarkYellow");
                // Terminal.WriteLine("Gray");
                // Terminal.WriteLine("DarkGray");
                // Terminal.WriteLine("White");

                // ShortcutKeyHelper.StopListening();

                KeyInput.ListenForKeys();

                List<DiscDetail> discs = Helpers.ReadDiscIndex();

                string searchString = "jpg"; //searchString.Trim().ToLower();

                List<DiscSourceFile> files = discs.SelectMany(x => x.Files).Where(x => x.RelativePath.ToLower().Contains(searchString)).ToList();
                Console.WriteLine("Matching files: " + files.Count().ToString("N0"));

                using (Pager pager = new Pager())
                {
                    // pager.StartLine = 1;
                    pager.ShowHeader = true;
                    pager.HeaderText = $"{"Disc".PadRight(4)}   {"Update Date/Time".PadRight(22)}   {"File"}";
                    pager.HighlightText = searchString;
                    pager.Highlight = true;
                    pager.HighlightColor = ConsoleColor.DarkYellow;

                    foreach (DiscSourceFile file in files)
                        pager.AppendLine($"{file.DestinationDisc.DiscNumber.ToString("0000")}   {file.LastWriteTimeUtc.ToLocalTime().ToString().PadRight(22)}   {file.RelativePath}");

                    pager.Start();
                    pager.WaitForExit();
                }
                KeyInput.StopListening();
                // MainMenu.StartOperation();
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

        public static void ClearLine()
        {
            Console.CursorLeft = 0;
            Console.Write("".PadRight(Console.BufferWidth - 1));
            Console.CursorLeft = 0;
        }
    }
}


