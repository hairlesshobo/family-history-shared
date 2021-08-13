using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Archiver.Classes;
using Archiver.Operations;
using Archiver.Utilities;
using Archiver.Utilities.Tape;
using ICSharpCode.SharpZipLib.Tar;
using Archiver.Utilities.Shared;
using Archiver.Classes.Tape;
using Archiver.Operations.Disc;
using Terminal.Gui;
using System.Runtime.InteropServices;
using Archiver.Classes.Shared;

namespace Archiver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (SystemInformation.OperatingSystemType == OSType.Unknown)
            {
                Console.Clear();
                Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                Console.WriteLine("The current operating system is unsupported.");
                Console.WriteLine();
                SystemInformation.WriteSystemInfo();
                Console.WriteLine();
                Console.WriteLine("The application will now exit.");
            }
            else
            {
                try
                {
                    Console.CancelKeyPress += (sender, e) => {
                        e.Cancel = true;
                    };
                    Console.TreatControlCAsInput = true;
                    Console.BackgroundColor = ConsoleColor.Black;

                    Console.Write("Reading configuration... ");
                    Config.ReadConfig();
                    Console.WriteLine("done");
                    
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
                    SystemInformation.WriteSystemInfo();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Write("Press ");
                    Formatting.WriteC(ConsoleColor.DarkYellow, "<any key>");
                    Console.WriteLine(" to terminate application");

                    Console.ReadKey(true);

                    return;
                }
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


