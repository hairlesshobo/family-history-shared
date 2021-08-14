using System;
using System.Runtime.InteropServices;
using Archiver.Shared;
using Archiver.Shared.Interfaces;
using Archiver.TapeServer.Classes.Config;

namespace Archiver.TapeServer
{
    class Program
    {
        [DllImport("libc.so.6", EntryPoint = "getpid")]
        private static extern int GetPid();

        static void Main()
        {
            TapeServerConfig config = TapeServerHelpers.ReadConfig();
            ITapeDrive tapeDrive = TapeServerHelpers.GetTapeDrive(config);

            NetworkServer server = new NetworkServer(config, tapeDrive);
            TapeOperator_Linux tol = new TapeOperator_Linux("/dev/random");

            byte[] buffer = new byte[1024];
            bool endOfData = tol.Read(buffer);

            int pid = GetPid();
            
            Console.WriteLine($"Archive TapeServer component starting up. (PID: {pid})");
            SystemInformation.WriteSystemInfo();
            Console.WriteLine();
            Console.WriteLine();
            // server.StartControlServer();
        }
    }
}
