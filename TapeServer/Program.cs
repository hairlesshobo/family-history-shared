using System;
using System.Runtime.InteropServices;
using Archiver.Shared;
using Archiver.Shared.Interfaces;
using Archiver.TapeServer.Classes.Config;
using Archiver.TapeServer.TapeDrivers;

namespace Archiver.TapeServer
{
    class Program
    {
        [DllImport("libc.so.6", EntryPoint = "getpid")]
        private static extern int GetPid();

        static void Main()
        {
            TapeServerConfig config = TapeServerHelpers.ReadConfig();
            // ITapeDrive tapeDrive = TapeServerHelpers.GetTapeDrive(config);

            // NetworkServer server = new NetworkServer(config, tapeDrive);
            NativeLinuxTapeDriver tapeDriver = new NativeLinuxTapeDriver(config.TapeDrive);

            

            // byte[] buffer = new byte[1024];
            // bool endOfData = tapeDriver.Read(buffer);
            tapeDriver.Eject();

            int pid = GetPid();
            
            Console.WriteLine($"Archive TapeServer component starting up. (PID: {pid})");
            SystemInformation.WriteSystemInfo();
            Console.WriteLine();
            Console.WriteLine();
            // server.StartControlServer();
        }
    }
}
