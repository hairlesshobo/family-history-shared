using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DiscArchiver.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;

namespace Archiver
{
    class Program
    {
        private static IConfiguration _config;   

        private static void ReadConfig()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            Globals._stagingDir = _config["StagingDir"];
            Globals._indexDiscDir = Globals._stagingDir + $"/index";
        }

        private static void ReadIndex()
        {
            string jsonDir = Globals._indexDiscDir + "/json";

            if (Directory.Exists(jsonDir))
            {
                string[] jsonFiles = Directory.GetFiles(jsonDir, "*.json");

                foreach (string jsonFile in jsonFiles)
                    Globals._destinationDiscs.Add(JsonConvert.DeserializeObject<DestinationDisc>(File.ReadAllText(jsonFile)));
            }
        }

        private static void ReadIsoHash(DestinationDisc disc, Stopwatch masterSw)
        {
            // Status.WriteDiscIsoHash(disc, masterSw.Elapsed, 0.0);

            // var md5 = new MD5_Generator(disc.IsoPath);

            // md5.OnProgressChanged += (currentPercent) => {
            //     Status.WriteDiscIsoHash(disc, masterSw.Elapsed, currentPercent);
            // };

            // md5.OnComplete += (hash) => {
            //     disc.Hash = hash;
            // };

            // Thread generateThread = new Thread(md5.GenerateHash);
            // generateThread.Start();
            // generateThread.Join();

            // Status.WriteDiscIsoHash(disc, masterSw.Elapsed, 100.0);
        }



        static void Main(string[] args)
        {
            ReadConfig();

            Console.WriteLine();
            Console.Write("Reading existing index... ");
            ReadIndex();
            Console.WriteLine("done");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Preparing...");
            Console.ResetColor();

            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
