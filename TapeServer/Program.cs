using System;
using System.Runtime.InteropServices;
using System.Text;
using Archiver.Shared;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;
using Archiver.TapeServer.TapeDrivers;

namespace Archiver.TapeServer
{
    class Program
    {
        [DllImport("libc.so.6", EntryPoint = "getpid")]
        private static extern int GetPid();

        static void Main()
        {
            ArchiverConfig config = Utils.ReadConfig();
            // ITapeDrive tapeDrive = TapeServerHelpers.GetTapeDrive(config);

            // NetworkServer server = new NetworkServer(config, tapeDrive);

            int pid = GetPid();
            
            Console.WriteLine($"Archive TapeServer component starting up. (PID: {pid})");
            Console.WriteLine();
            SystemInformation.WriteSystemInfo(true);
            Console.WriteLine();
            Console.WriteLine();

            uint blockSize = 512; //512 * 512;
            int maxBlocks = 512;
            int currentBlock = 0;
            string device = "/home/flip/archive/test.txt"; // config.TapeDrive;
            string text = String.Empty;

            using (NativeLinuxTapeDriver tapeDrive = new NativeLinuxTapeDriver(device, blockSize))
            {
                tapeDrive.Open();
                byte[] buffer = new byte[blockSize];
                bool endOfData = false;

                do
                {
                    endOfData = tapeDrive.Read(buffer);

                    int strlen = Array.IndexOf(buffer, (byte)0);

                    if (!endOfData)
                    {
                        if (strlen > 0)
                            text += Encoding.UTF8.GetString(buffer, 0, strlen);
                        else
                            text += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    }

                    currentBlock++;
                }
                while (!endOfData && currentBlock <= maxBlocks);

                Console.WriteLine(text);
            }
            

            // server.StartControlServer();
        }
    }
}
