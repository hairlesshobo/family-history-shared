using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Archiver.Shared;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;

namespace Archiver.TestCLI
{
    class Program
    {
        [DllImport("libc.so.6", EntryPoint = "getpid")]
        private static extern int GetPid();

        static void Main()
        {
            Utils.RequireSupportedOS();

            List<ValidationError> configErrors;
            ArchiverConfig config = Utils.ReadConfig(out configErrors);

            int pid = GetPid();
            
            Formatting.WriteLineC(ConsoleColor.Green, $"Archive TestCLI component starting up. (PID: {pid})");
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            SysInfo.WriteSystemInfo(config, SysInfoTemplate.TapeServer, true);

            if (configErrors.Count > 0)
            {
                Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Formatting.WriteLineC(ConsoleColor.Red, $"{configErrors.Count} Configuration ERROR(s) Found!");
                Console.WriteLine();

                int fieldWidth = configErrors.Max(x => x.Field.Length)+2;

                foreach (ValidationError error in configErrors)
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

            // TODO: Test this with disc in the drive
            //var drives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.CDRom);

            Console.WriteLine(SysInfo.IsOpticalDrivePresent);
        }

        
    }
}
