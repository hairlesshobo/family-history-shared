/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;

namespace Archiver.Shared
{
    public enum SysInfoMode
    {
        Unknown = 0,
        Archiver = 1,
        TapeServer = 2,
        TestCLI = 3
    }

    public static class SysInfo
    {
        private static bool _didInit = false;
        private static ArchiverConfig _config = null;
        private static SysInfoMode _mode = SysInfoMode.Unknown;
        private static OSType _osType = OSType.Unknown;
        private static bool _isOpticalDrivePresent = false;
        private static bool _isReadonlyFilesystem = true;
        private static bool _isTapeDrivePresent = true;
        private static SystemDirectories _directories = null;
        private static List<ValidationError> _configErrors;


        public static ArchiverConfig Config => _config;
        public static SystemDirectories Directories => _directories;
        public static List<ValidationError> ConfigErrors => _configErrors;

        public static OSType OSType => _osType;
        public static SysInfoMode Mode => _mode;
        public static bool IsOpticalDrivePresent => _isOpticalDrivePresent;
        public static bool IsReadonlyFilesystem => _isReadonlyFilesystem;
        public static bool IsTapeDrivePresent => _isTapeDrivePresent;
        public static string TapeDrive => _mode == SysInfoMode.Archiver ? Config.Tape.Drive : Config.TapeServer.Drive;
        public static Architecture Architecture => System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
        public static string Description => System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        public static string Identifier => System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
        public static int PID => Process.GetCurrentProcess().Id; //(SysInfo.OSType == OSType.Linux ? Linux.GetPid() : Process.GetCurrentProcess().Id);
        
        
        static SysInfo()
        {
            _mode = GetMode();

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

        public static void InitPlatform()
        {
            if (!_didInit)
            {
                _config = ConfigUtils.ReadConfig(out _configErrors);

                _directories = new SystemDirectories();
                _directories.Index = PathUtils.ResolveRelativePath(Path.Join(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "../"));
                _directories.JSON = PathUtils.CleanPathCombine(_directories.Index, "json");
                _directories.DiscStaging = (Config.Disc.StagingDir != null ? PathUtils.ResolveRelativePath(Config.Disc.StagingDir) : PathUtils.CleanPath(Path.GetTempPath()));
                _directories.ISO = PathUtils.CleanPathCombine(_directories.Index, "../iso");

                _isOpticalDrivePresent = OpticalDriveUtils.GetDriveNames().Any();
                _isReadonlyFilesystem = TestForReadonlyFs();
                _isTapeDrivePresent = TapeUtilsNew.IsTapeDrivePresent();
            }
        }

        internal static void SetConfig(ArchiverConfig config)
        {
            if (_config == null)
                _config = config;
        }

        public static void WriteSystemInfo(bool writeConfig = false, bool color = false)
        {
            // TODO: Add GUI window for system info
            PrintHeader(color, "System Information:");
            PrintField(color, 11, "OS Platform", $"{SysInfo.OSType.ToString()} ({SysInfo.Architecture.ToString()})");
            PrintField(color, 11, "Description", SysInfo.Description);
            PrintField(color, 11, "Identifier", SysInfo.Identifier);

            if (writeConfig && Mode == SysInfoMode.TapeServer)
            {
                Console.WriteLine();
                PrintHeader(color, "Configuration:");
                PrintField(color, 12, "Tape Drive", Config.TapeServer.Drive);
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

        private static SysInfoMode GetMode()
        {
            string assemblyName = Assembly.GetEntryAssembly().GetName().Name;

            switch (assemblyName)
            {
                case "Archiver":
                    return SysInfoMode.Archiver;

                case "TapeServer":
                    return SysInfoMode.TapeServer;

                case "TestCLI":
                    return SysInfoMode.TestCLI;
                
                default:
                    return SysInfoMode.Unknown;
            }
        }

        private static bool TestForReadonlyFs()
        {
            string currentdir = Directory.GetCurrentDirectory();
            string testFile = Path.Join(currentdir, "__accesstest.tmp");
            bool canWrite = true;

            if (File.Exists(testFile))
            {
                try
                {
                    File.Delete(testFile);
                }
                catch
                {
                    canWrite = false;
                }
            }

            try
            {
                using (FileStream stream = File.Create(testFile))
                { }

                File.Delete(testFile);
            }
            catch
            {
                canWrite = false;
            }

            return !canWrite;
        }

        public class SystemDirectories
        {
            public string Index { get; internal protected set; }
            public string JSON { get; internal protected set; }
            public string DiscStaging { get; internal protected set; }
            public string ISO { get; internal protected set; }
        }
    }
}