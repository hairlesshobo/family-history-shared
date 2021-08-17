using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Archiver.Shared;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;
using Archiver.Shared.TapeDrivers;

namespace Archiver.TapeServer
{
    class Program
    {
        [DllImport("libc.so.6", EntryPoint = "getpid")]
        private static extern int GetPid();

        static void Main()
        {
            Utils.RequireSupportedOS();
            SysInfo.InitPlatform();
            

            int pid = GetPid();
            
            Formatting.WriteLineC(ConsoleColor.Green, $"Archive TapeServer component starting up. (PID: {pid})");
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            SysInfo.WriteSystemInfo(true);

            if (SysInfo.ConfigErrors.Count > 0)
            {
                Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Formatting.WriteLineC(ConsoleColor.Red, $"{SysInfo.ConfigErrors.Count} Configuration ERROR(s) Found!");
                Console.WriteLine();

                int fieldWidth = SysInfo.ConfigErrors.Max(x => x.Field.Length)+2;

                foreach (ValidationError error in SysInfo.ConfigErrors)
                {
                    Formatting.WriteC(ConsoleColor.Cyan, "Field: ");
                    Console.WriteLine(error.Field.PadRight(fieldWidth));
                    Formatting.WriteC(ConsoleColor.Red, "Error: ");
                    Console.WriteLine(error.Error);
                    Console.WriteLine();
                }

                Formatting.WriteLineC(ConsoleColor.Red, "Application terminating due to error!");
                Environment.Exit(1);
            }

            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine();
            // ITapeDrive tapeDrive = TapeServerHelpers.GetTapeDrive(config);

            // NetworkServer server = new NetworkServer(config, tapeDrive);



            // uint blockSize = 512; //512 * 512;
            // int maxBlocks = 512;
            // int currentBlock = 0;
            // string device = "/home/flip/archive/test.txt"; // config.TapeDrive;
            // string text = String.Empty;

            // using (NativeLinuxTapeDriver tapeDrive = new NativeLinuxTapeDriver(device, blockSize))
            // {
            //     tapeDrive.Open();
            //     byte[] buffer = new byte[blockSize];
            //     bool endOfData = false;

            //     do
            //     {
            //         endOfData = tapeDrive.Read(buffer);

            //         int strlen = Array.IndexOf(buffer, (byte)0);

            //         if (!endOfData)
            //         {
            //             if (strlen > 0)
            //                 text += Encoding.UTF8.GetString(buffer, 0, strlen);
            //             else
            //                 text += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            //         }

            //         currentBlock++;
            //     }
            //     while (!endOfData && currentBlock <= maxBlocks);

            //     Console.WriteLine(text);
            // }
            

            // server.StartControlServer();
        }
    }
}
