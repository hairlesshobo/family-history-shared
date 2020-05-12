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

            uint fileSize = 768;

            Console.WriteLine(Helpers.RoundToNextMultiple(fileSize, 512));

            Console.ReadLine();
            return;


            // we can calculate the exact amount if we factor in the file and directory headers. 
            // each file headers is 512 bytes each
            // each file is padded to the next 512 byte multiple
            // each directory is 512 bytes each
            // the end of the file has 1024 bytes of padding
            // the total size is rounded up to the next block size multiple 
            //    if blocking factor is 512 this means that the output size is rounded to the next (512*512)
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
