using System;
using System.Runtime.InteropServices;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;

namespace Archiver.Shared
{
    public enum SysInfoTemplate
    {
        None = 0,
        Archiver = 1,
        TapeServer = 2
    }

    public static class SysInfo
    {
        private static OSType _osType = OSType.Unknown;
        private static ArchiverConfig _config = null;

        public static ArchiverConfig Config => _config; 


        public static OSType OSType => _osType;
        public static bool IsOpticalDrivePresent => OpticalDriveUtils.IsDrivePresent();
        public static Architecture Architecture => System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
        public static string Description => System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        public static string Identifier => System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
        
        
        static SysInfo()
        {            
            if (OperatingSystem.IsWindows())
                _osType = OSType.Windows;
            else if (OperatingSystem.IsLinux())
                _osType = OSType.Linux;

            // these are commented out because we have not yet built support for them into the app. For now, they will 
            // be identified as "Unknown" and the app will not lauch on an unknown platform.
            // else if (OperatingSystem.IsFreeBSD())
            //     _osType = OSType.FreeBSD;
            // else if (OperatingSystem.IsMacOS())
            //     _osType = OSType.OSX;
        }

        internal static void SetConfig(ArchiverConfig config)
        {
            if (_config == null)
                _config = config;
        }

        public static void WriteSystemInfo(bool color = false)
            => WriteSystemInfo(null, SysInfoTemplate.None, color);

        public static void WriteSystemInfo(ArchiverConfig config, SysInfoTemplate template, bool color = false)
        {
            // TODO: Add GUI window for system info
            PrintHeader(color, "System Information:");
            PrintField(color, 11, "OS Platform", $"{SysInfo.OSType.ToString()} ({SysInfo.Architecture.ToString()})");
            PrintField(color, 11, "Description", SysInfo.Description);
            PrintField(color, 11, "Identifier", SysInfo.Identifier);

            if (config != null && template == SysInfoTemplate.TapeServer)
            {
                Console.WriteLine();
                PrintHeader(color, "Configuration:");
                PrintField(color, 12, "Tape Drive", config.TapeServer.DrivePath);
            }
        }

        private static void PrintHeader(bool color, string value)
        {
            if (color)
                Formatting.WriteLineC(ConsoleColor.Magenta, value);
            else
                Console.WriteLine(value);
        }

        private static void PrintField(bool color, int width, string fieldName, string value)
        {
            if (color == true)
            {
                Formatting.WriteC(ConsoleColor.Cyan, $"  {fieldName.PadLeft(width)}: ");
                Console.WriteLine(value);
            }
            else
                Console.WriteLine($"  {fieldName.PadLeft(width)}: {value}");
        }
    }
}