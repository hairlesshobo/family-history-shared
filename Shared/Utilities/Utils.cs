using System;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class Utils
    {
        public static void RequireSupportedOS()
        {
            if (SysInfo.OSType == OSType.Unknown)
            {
                Console.Clear();
                Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                Console.WriteLine("The current operating system is unsupported.");
                Console.WriteLine();
                SysInfo.WriteSystemInfo();
                Console.WriteLine();
                Console.WriteLine("The application will now exit.");
                Environment.Exit(2);
            }
        }
    }
}