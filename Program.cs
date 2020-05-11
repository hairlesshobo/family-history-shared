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

            // Console.Write("Reading disc index... ");
            // Helpers.ReadIndex();
            // Console.WriteLine("done");

            Console.Clear();

            //Console.WriteLine(ReadSummary());

            // we can calculate the exact amount if we factor in the file and directory headers. 
            // headers are 512 bytes each, but need to also know exactly how many directories exist as well. 
            // hopefully we can accurately capture this information prior to generating the tar and store
            // it in the tape json file
            long dataSize = 48469841375;
            dataSize += 140 * 512;
            dataSize += 12 * 512; // estimate about 12 directories
            
            MD5_Tape md5 = new MD5_Tape(dataSize);
            md5.OnProgressChanged += (progress) => {
                Console.CursorLeft = 0;

                string left = String.Empty;
                left += Utilities.Formatting.GetFriendlySize(progress.TotalCopiedBytes).PadLeft(10);
                left += " ";
                left += $"[{Utilities.Formatting.GetFriendlyTransferRate(progress.InstantTransferRate).PadLeft(12)}]";
                left += " ";
                left += $"[{Utilities.Formatting.GetFriendlyTransferRate(progress.AverageTransferRate).PadLeft(12)}]";

                Console.Write(left + StatusHelpers.GeneratePercentBar(Console.BufferWidth, left.Length, 0, progress.PercentCopied, (progress.PercentCopied == 100.0)));
            };

            Thread thread = new Thread(md5.GenerateHash);
            thread.Start();
            thread.Join();


            //Console.WriteLine(TapeUtils.ReadSummaryFromTape());
            //TapeUtils.ListContentsFromTape();

            //MainMenu.StartOperation();

            Console.ReadLine();
        }
    }
}
