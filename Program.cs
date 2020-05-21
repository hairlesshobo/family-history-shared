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

namespace Archiver
{
    class Program
    {
        // private static void DoScanOnly()
        // {
        //     Status.Initialize();

        //     DiscProcessing.IndexAndCountFiles();

        //     Status.ProcessComplete();

        //     Summary.StartOperation();
        // }

        static void Main(string[] args)
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


