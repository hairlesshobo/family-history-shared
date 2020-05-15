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
            
            Helpers.ReadDiscIndex();

            Console.Clear();

            

            // MD5_Tape md5 = new MD5_Tape();
            // md5.OnProgressChanged += (progress) => {
            //     Console.CursorLeft = 0;

            //     string left = String.Empty;
            //     left += Formatting.GetFriendlySize(progress.TotalCopiedBytes).PadLeft(10);
            //     left += " ";
            //     left += $"[{Formatting.GetFriendlyTransferRate(progress.InstantTransferRate).PadLeft(12)}]";
            //     left += " ";
            //     left += $"[{Formatting.GetFriendlyTransferRate(progress.AverageTransferRate).PadLeft(12)}]";

            //     Console.Write(left + StatusHelpers.GeneratePercentBar(Console.BufferWidth, left.Length, 0, progress.PercentCopied, (progress.PercentCopied == 100.0)));
            // };

            // Thread thread = new Thread(md5.GenerateHash);
            // thread.Start();
            // thread.Join();

            MainMenu.StartOperation();

            Console.ReadLine();
        }
    }
}
