using System;
using Archiver.Operations;
using Archiver.Utilities.Shared;
using Terminal.Gui;
using Archiver.Shared;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;

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
                Console.CancelKeyPress += (sender, e) => {
                    e.Cancel = true;
                };
                Console.TreatControlCAsInput = true;
                Console.BackgroundColor = ConsoleColor.Black;

                // Console.Write("Reading configuration... ");
                // Config.ReadConfig();
                // Console.WriteLine("done");
                
                Console.Clear();

                MainMenu.StartOperation();
            }
            catch (Exception e)
            {
                Application.Shutdown();
                Console.Clear();
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
            Console.Write("".PadRight(Console.BufferWidth-1));
            Console.CursorLeft = 0;
        }
    }
}


