using System;
using System.Text;
using System.Threading;
using Archiver.Operations;
using Archiver.Shared;
using Archiver.Shared.Classes;
using Archiver.Shared.Utilities;
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

                Terminal.SetDefaultBackgroundColor(ConsoleColor.Black);
                Terminal.ResetColor();
                Terminal.Clear();

                TerminalColor.ProgressBarFilled = ConsoleColor.DarkRed;

                Console.CursorVisible = false;

                var splitHeader = new SplitLine("Verify Disc MD5 Hash", "Archiver");
                Terminal.NextLine();
                var hl1 = new HorizontalLine(ConsoleColor.Magenta);
                Terminal.NextLine();

                // Key key = Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control | ConsoleModifiers.Shift);
                // Key key2 = Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Shift | ConsoleModifiers.Control);

                // Terminal.WriteLine(key.ToString());
                // Terminal.WriteLine(key2.ToString());

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

                var kvtDiscName = new KeyValueText("Disc Name", OpticalDriveUtils.GetDriveLabel("sr0"), 14);
                Terminal.NextLine();

                var kvtVerified = new KeyValueText("Verified", Formatting.GetFriendlySize(0), 14);
                Terminal.NextLine();

                var kvtCurrentRate = new KeyValueText("Current Rate", Formatting.GetFriendlyTransferRate(0), 14);
                Terminal.NextLine();

                var kvtAvgRate = new KeyValueText("Average Rate", Formatting.GetFriendlyTransferRate(0), 14);
                Terminal.NextLine();
                Terminal.NextLine();

                var progressBar = new ProgressBar();
                Terminal.NextLine(); 

                StatusBar.GetInstance().ShowItems(
                    new StatusBarItem(
                        "Quit",
                        (key) => {
                            // Console.Clear();
                            Terminal.NextLine();
                            Terminal.WriteLine("MEOW!");
                            // Console.ReadKey();
                            Environment.Exit(0);
                        },
                        Key.MakeKey(ConsoleKey.Q)
                    ),
                    new StatusBarItem(
                        "Navigate",
                        (key) => { },
                        Key.MakeKey(ConsoleKey.UpArrow),
                        Key.MakeKey(ConsoleKey.DownArrow)
                    )
                );

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

                KeyInput.ListenForKeys();

                for (int i = 0; i < 1000; i += 27)
                {
                    Thread.Sleep(400);
                    progressBar.UpdateProgress((double)i / 1000.0);
                    kvtVerified.UpdateValue(i.ToString());
                }

                // ShortcutKeyHelper.StopListening();

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


