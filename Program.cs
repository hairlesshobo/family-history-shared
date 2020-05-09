using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Archiver.Classes;
using Archiver.Operations;
using Archiver.Utilities;
using Newtonsoft.Json;

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
            Console.BackgroundColor = ConsoleColor.Black;

            Console.Write("Reading configuration... ");
            Config.ReadConfig();
            Console.WriteLine("done");

            Console.Write("Reading disc index... ");
            Helpers.ReadIndex();
            Console.WriteLine("done");

            Console.Clear();

            MainMenu.StartOperation();

            Console.ReadLine();
        }
    }
}
